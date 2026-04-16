using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace C8yServices.Extensions.Json;

public static class JsonInfoExtensions
{
  private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> JsonPropertiesCache = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();

  public static bool TryGetJsonAttribute<T>(this MemberInfo memberInfo, out T? jsonAttribute) where T : JsonAttribute
  {
    ArgumentNullException.ThrowIfNull(memberInfo);
    jsonAttribute = Attribute.GetCustomAttribute(memberInfo, typeof(T)) as T;
    return jsonAttribute != null;
  }

  public static string GetJsonPropertyName(this MemberInfo memberInfo, JsonSerializerOptions? serializerOptions = null)
  {
    object? obj;
    if (!memberInfo.TryGetJsonAttribute(out JsonPropertyNameAttribute? jsonAttribute))
    {
      obj = serializerOptions?.PropertyNamingPolicy?.ConvertName(memberInfo.Name);
      if (obj == null)
      {
        return memberInfo.Name;
      }
    }
    else
    {
      obj = jsonAttribute?.Name;
      if (obj == null)
      {
        return memberInfo.Name;
      }
    }

    return (string)obj;
  }

  public static JsonIgnoreCondition GetJsonIgnoreCondition(this MemberInfo memberInfo, JsonSerializerOptions? serializerOptions = null)
  {
    if (!memberInfo.TryGetJsonAttribute(out JsonIgnoreAttribute? jsonAttribute) || jsonAttribute == null)
    {
      return serializerOptions?.DefaultIgnoreCondition ?? JsonIgnoreCondition.Never;
    }

    return jsonAttribute.Condition;
  }

  public static IReadOnlyDictionary<string, PropertyInfo> GetJsonPropertyInfos(this Type type, JsonSerializerOptions? serializerOptions = null)
  {
    ArgumentNullException.ThrowIfNull(type);
    if (JsonPropertiesCache.TryGetValue(type, out IReadOnlyDictionary<string, PropertyInfo>? value))
    {
      return value;
    }

    PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
    Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>();
    PropertyInfo[] array = properties;
    foreach (PropertyInfo propertyInfo in array)
    {
      string jsonPropertyName = propertyInfo.GetJsonPropertyName(serializerOptions);
      if (!dictionary.TryAdd(jsonPropertyName, propertyInfo) && propertyInfo.DeclaringType == type)
      {
        dictionary[jsonPropertyName] = propertyInfo;
      }
    }

    JsonPropertiesCache[type] = new ReadOnlyDictionary<string, PropertyInfo>(dictionary);
    return JsonPropertiesCache[type];
  }
}