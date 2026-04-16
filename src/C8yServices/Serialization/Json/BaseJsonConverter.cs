using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using C8yServices.Extensions.Collections;
using C8yServices.Extensions.Json;

namespace C8yServices.Serialization.Json;

public abstract class BaseJsonConverter<T> : JsonConverter<T> where T : class, new()
{
  private readonly Action<T, IDictionary<string, object?>> _customPropertyAction;
  private readonly string _customFragmentName;
  private readonly IDictionary<string, Type> _additionalPropertyClasses;

  protected BaseJsonConverter(Action<T, IDictionary<string, object?>> customPropertyAction, string customFragmentName,
    IDictionary<string, Type> additionalPropertyClasses)
  {
    _customPropertyAction = customPropertyAction;
    _customFragmentName = customFragmentName;
    _additionalPropertyClasses = additionalPropertyClasses;
  }

  /// <inheritdoc/>
  public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    var instance = JsonDeserializationUtil.CreateInstance<T>(typeToConvert);
    var dictionary = new Dictionary<string, object?>();
    var instanceProperties = GetJsonPropertyInfos(typeof(T));
    using var jsonDocument = JsonDocument.ParseValue(ref reader);
    var objectEnumerator = jsonDocument.RootElement.EnumerateObject();
    while (objectEnumerator.MoveNext())
    {
      ReadProperty(instance, objectEnumerator.Current, instanceProperties, options, _additionalPropertyClasses, dictionary);
    }
    _customPropertyAction(instance, dictionary);

    return instance;
  }

  /// <inheritdoc/>
  public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
  {
    writer.WriteStartObject();
    if (value is null)
    {
      writer.WriteEndObject();

      return;
    }
    foreach (var property in GetJsonPropertyInfos(typeof(T)))
    {
      WriteProperty(property.Key, property.Value, writer, value, options);
    }

    writer.WriteEndObject();
  }

  private static IReadOnlyDictionary<string, PropertyInfo> GetJsonPropertyInfos(Type type)
  {
    var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
    var propertiesMap = new Dictionary<string, PropertyInfo>();

    foreach (var property in properties)
    {
      var jsonPropertyName = property.GetJsonPropertyName();
      if (propertiesMap.TryAdd(jsonPropertyName, property))
        continue;

      if (property.DeclaringType == type)
        propertiesMap[jsonPropertyName] = property;
    }

    return new ReadOnlyDictionary<string, PropertyInfo>(propertiesMap);
  }

  private static void ReadProperty(T instance, JsonProperty jsonProperty, IReadOnlyDictionary<string, PropertyInfo> instanceProperties, JsonSerializerOptions options,
    IDictionary<string, Type> additionalPropertyClasses, IDictionary<string, object?> dictionary)
  {
    var propertyInfo = GetReadableInstanceProperty(jsonProperty, instanceProperties, options);
    if (propertyInfo is null)
    {
      if (JsonDeserializationUtil.TryDeserialize(jsonProperty.Value, out var deserializedObject, out _, options))
        dictionary.Add(jsonProperty.Name, deserializedObject);
      return;
    }

    options = AddPropertySpecificConverters(propertyInfo, options);

    var propertyType = GetPropertyType(propertyInfo, jsonProperty, additionalPropertyClasses);
    object? deserializedValue = JsonSerializer.Deserialize(jsonProperty.Value.GetRawText(), propertyType, options);

    propertyInfo.SetValue(instance, deserializedValue);
  }

  private static PropertyInfo? GetReadableInstanceProperty(JsonProperty jsonProperty, IReadOnlyDictionary<string, PropertyInfo> instanceProperties, JsonSerializerOptions options)
  {
    if (!instanceProperties.TryGetValue(jsonProperty.Name, out var propertyInfo))
    {
      return null;
    }
    if (!propertyInfo.CanWrite)
    {
      throw new InvalidOperationException($"'{typeof(T).Name}' contains a property '{jsonProperty.Name}', but this cannot be set (read only)");
    }
    var jsonIgnoreCondition = GetJsonIgnoreCondition(propertyInfo, options);

    return jsonIgnoreCondition == JsonIgnoreCondition.Always ? null : propertyInfo;
  }

  private static JsonIgnoreCondition GetJsonIgnoreCondition(MemberInfo propertyInfo, JsonSerializerOptions options) =>
    Attribute.IsDefined(propertyInfo, typeof(JsonIgnoreAttribute))
      ? ((JsonIgnoreAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(JsonIgnoreAttribute)))?.Condition ?? JsonIgnoreCondition.Never
      : options.DefaultIgnoreCondition;

  private static Type GetPropertyType(PropertyInfo? propertyInfo, JsonProperty jsonProperty, IDictionary<string, Type> additionalPropertyClasses) =>
    propertyInfo?.PropertyType
    ?? (additionalPropertyClasses.TryGetValue(jsonProperty.Name, out var propertyClass)
      ? propertyClass
      : jsonProperty.Value.ValueKind.GetDeserializedType());

  private void WriteProperty(string name, PropertyInfo propertyInfo, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
  {
    if (!propertyInfo.CanRead)
    {
      return;
    }
    // get JsonIgnoreCondition from the property attribute or the default from the serializer options
    var jsonIgnoreCondition = GetJsonIgnoreCondition(propertyInfo, options);
    if (jsonIgnoreCondition == JsonIgnoreCondition.Always)
    {
      return;
    }
    var propertyValue = propertyInfo.GetValue(value, null);
    var propertyValueIsNull = propertyValue is null;
    if (propertyValueIsNull)
    {
      return;
    }

    options = AddPropertySpecificConverters(propertyInfo, options);
    
    // "special" serialization of the ManagedObject.CustomFragments dictionary
    if (propertyInfo.Name == _customFragmentName)
    {
      if (propertyValue is not IDictionary<string, object?> dictionary)
      {
        return;
      }
      foreach (var item in dictionary)
      {
        writer.WritePropertyName(item.Key);
        Write(writer, item.Value, options);
      }

      return;
    }
    if (WhenWritingNullAndPropertyValueIsNull(jsonIgnoreCondition, propertyValue) || WhenWritingDefaultAndPropertyValueIsDefault(propertyInfo, jsonIgnoreCondition, propertyValue))
    {
      return;
    }
    writer.WritePropertyName(name);
    Write(writer, propertyValue, options);
  }

  private static bool WhenWritingDefaultAndPropertyValueIsDefault(PropertyInfo propertyInfo, JsonIgnoreCondition jsonIgnoreCondition, object? propertyValue) =>
    jsonIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault && (
      !propertyInfo.PropertyType.IsValueType && propertyValue is null
      ||
      propertyInfo.PropertyType.IsValueType && propertyValue is not null && propertyValue.Equals(JsonDeserializationUtil.GetValueTypeDefault(propertyInfo.PropertyType)));

  private static bool WhenWritingNullAndPropertyValueIsNull(JsonIgnoreCondition jsonIgnoreCondition, object? propertyValue) =>
    jsonIgnoreCondition == JsonIgnoreCondition.WhenWritingNull && propertyValue is null;

  private static void Write(Utf8JsonWriter writer, object? propertyValue, JsonSerializerOptions options)
  {
    var type = propertyValue?.GetType();
    if (type is null)
    {
      JsonSerializer.Serialize(writer, propertyValue, options);

      return;
    }

    JsonSerializer.Serialize(writer, propertyValue, GetCorrectType(type), options);
  }

  private static Type GetCorrectType(Type type) =>
    type == typeof(byte[]) ? typeof(IEnumerable<byte>) : type;

  /// <summary>
  /// adds property specific <see cref="JsonConverter"/>s to given <paramref name="serializerOptions"/>
  /// if the property has <see cref="JsonConstructorAttribute"/>(s)
  /// </summary>
  private static JsonSerializerOptions AddPropertySpecificConverters(PropertyInfo propertyInfo, JsonSerializerOptions serializerOptions)
  {
    var converterTypes = propertyInfo.GetCustomAttributes<JsonConverterAttribute>(true)
      .Select(CreateJsonConverter)
      .WhereNotNull()
      .ToList();

    if (converterTypes.Count == 0)
      return serializerOptions;

    var newOptions = new JsonSerializerOptions(serializerOptions);
    converterTypes.ForEach(newOptions.Converters.Add);
    return newOptions;
  }

  /// <summary>
  /// tries tor create an instance of <see cref="JsonConverter"/> by the type in given <see cref="JsonConverterAttribute"/>
  /// </summary>
  private static JsonConverter? CreateJsonConverter(JsonConverterAttribute attribute)
  {
    try
    {
      return attribute.ConverterType is not null
        ? Activator.CreateInstance(attribute.ConverterType) as JsonConverter
        : null;
    }
    catch
    {
      return null;
    }
  }
}