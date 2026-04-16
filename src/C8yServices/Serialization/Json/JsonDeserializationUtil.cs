using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using C8yServices.Extensions.Json;

namespace C8yServices.Serialization.Json;

public static class JsonDeserializationUtil
{
  private static readonly IReadOnlyDictionary<JsonValueKind, Type> JsonValueKindTypeMap = new ReadOnlyDictionary<JsonValueKind, Type>(new Dictionary<JsonValueKind, Type>
{
  {
    JsonValueKind.Array,
    typeof(object)
  },
  {
    JsonValueKind.False,
    typeof(bool)
  },
  {
    JsonValueKind.Null,
    typeof(object)
  },
  {
    JsonValueKind.Number,
    typeof(double)
  },
  {
    JsonValueKind.Object,
    typeof(Dictionary<string, object>)
  },
  {
    JsonValueKind.String,
    typeof(string)
  },
  {
    JsonValueKind.True,
    typeof(bool)
  },
  {
    JsonValueKind.Undefined,
    typeof(object)
  }
});

  private static readonly ConcurrentDictionary<Type, object> ValueTypeDefaults = new ConcurrentDictionary<Type, object>();

  public static Type GetDeserializedType(this JsonValueKind valueKind)
  {
    return JsonValueKindTypeMap[valueKind];
  }

  public static bool CanSetJsonProperty(PropertyInfo propertyInfo, JsonSerializerOptions? serializerOptions = null)
  {
    ArgumentNullException.ThrowIfNull(propertyInfo);
    if (propertyInfo.CanWrite)
    {
      return propertyInfo.GetJsonIgnoreCondition(serializerOptions) != JsonIgnoreCondition.Always;
    }

    return false;
  }

  public static bool TryGetSettableJsonProperty(Type classType, string jsonPropertyName, out PropertyInfo? propertyInfo, JsonSerializerOptions? serializerOptions = null)
  {
    propertyInfo = null;
    if (string.IsNullOrWhiteSpace(jsonPropertyName))
    {
      return false;
    }

    if (!classType.GetJsonPropertyInfos(serializerOptions).TryGetValue(jsonPropertyName, out propertyInfo) || (object)propertyInfo == null || !propertyInfo.CanWrite)
    {
      return false;
    }

    if (propertyInfo.GetJsonIgnoreCondition(serializerOptions) == JsonIgnoreCondition.Always)
    {
      propertyInfo = null;
    }

    return propertyInfo != null;
  }

  public static object GetValueTypeDefault(Type type)
  {
    if (!type.IsValueType)
    {
      throw new InvalidOperationException(type.FullName + " is not a value type");
    }

    if (!ValueTypeDefaults.TryGetValue(type, out object? value))
    {
      value = RuntimeHelpers.GetUninitializedObject(type);
      ValueTypeDefaults[type] = value;
    }

    return value;
  }

  public static T CreateInstance<T>(Type instanceType, object?[]? parameters = null) where T : class
  {
    JsonExceptionThrower.ThrowIfNull(instanceType, "No type for instance creation given");
    JsonExceptionThrower.ThrowIfNotAssignableTo<T>(instanceType);
    try
    {
      return (Activator.CreateInstance(instanceType, parameters) as T) ?? throw new JsonException($"Can't create instance of {instanceType.Name} by constructor with {((parameters != null) ? parameters.Length : 0)} parameter(s)");
    }
    catch (Exception ex)
    {
      throw new JsonException($"Error in creating instance of type {instanceType.Name} by a constructor with {((parameters != null) ? parameters.Length : 0)} parameter(s): {ex.Message}", ex);
    }
  }

  public static bool TryDeserialize(JsonElement? jsonElement, out object? deserializedObject, out string? errorMessage, JsonSerializerOptions? serializerOptions = null)
  {
    ArgumentNullException.ThrowIfNull(jsonElement);
    if (jsonElement.Value.ValueKind == JsonValueKind.Object)
    {
      return TryDeserializeJsonObject(jsonElement.Value, out deserializedObject, out errorMessage, serializerOptions);
    }

    return TryDeserializeNonJsonObject(jsonElement.Value, out deserializedObject, out errorMessage, serializerOptions);
  }

  public static void SetJsonProperty(object classInstance, string jsonPropertyName, PropertyInfo propertyInfo, JsonElement propertyValue, JsonSerializerOptions? serializerOptions = null)
  {
    JsonExceptionThrower.ThrowIfNull(classInstance, "No class instance given to set JSON property");
    JsonExceptionThrower.ThrowIfNullOrWhitespace(jsonPropertyName, "No JSON property name given");
    JsonExceptionThrower.ThrowIfNull(propertyInfo, "No property info given to set JSON property " + jsonPropertyName);
    JsonExceptionThrower.ThrowIfNull(propertyValue, "No value given to set JSON property " + jsonPropertyName);
    JsonExceptionThrower.ThrowIfFalse(CanSetJsonProperty(propertyInfo, serializerOptions));
    object? value;
    try
    {
      value = propertyValue.Deserialize(propertyInfo.PropertyType, serializerOptions);
    }
    catch (Exception ex)
    {
      throw new JsonException("Error in deserializing JSON property " + jsonPropertyName + ": " + ex.Message, ex);
    }

    JsonExceptionThrower.ThrowIfNull(value, "Deserialization of JSON property " + jsonPropertyName + " failed");
    try
    {
      propertyInfo.SetValue(classInstance, value);
    }
    catch (Exception ex2)
    {
      throw new JsonException("Error in setting JSON property value " + jsonPropertyName + ": " + ex2.StackTrace);
    }
  }

  private static bool TryDeserializeJsonObject(JsonElement jsonElement, out object? deserializedObject, out string? errorMessage, JsonSerializerOptions? serializerOptions)
  {
    deserializedObject = null;
    errorMessage = null;
    Dictionary<string, object>? dictionary;
    try
    {
      dictionary = jsonElement.Deserialize<Dictionary<string, object>>(serializerOptions);
      if (dictionary == null)
      {
        errorMessage = $"Deserialization of JSON element of value kind {jsonElement.ValueKind} failed";
        return false;
      }
    }
    catch (Exception ex)
    {
      errorMessage = $"Error in deserializing JSON element of value kind {jsonElement.ValueKind}: {ex.StackTrace}";
      return false;
    }

    foreach (KeyValuePair<string, object> item in dictionary)
    {
      if (item.Value is JsonElement jsonElement2)
      {
        if (!TryDeserialize(jsonElement2, out object? deserializedObject2, out errorMessage, serializerOptions) || deserializedObject2 == null)
        {
          return false;
        }

        dictionary[item.Key] = deserializedObject2;
      }
    }

    deserializedObject = dictionary;
    return true;
  }

  private static bool TryDeserializeNonJsonObject(JsonElement jsonElement, out object? deserializedObject, out string? errorMessage, JsonSerializerOptions? serializerOptions)
  {
    Type deserializedType = jsonElement.ValueKind.GetDeserializedType();
    try
    {
      deserializedObject = jsonElement.Deserialize(deserializedType, serializerOptions);
      errorMessage = ((deserializedObject == null) ? $"Deserialization of JSON element of value kind {jsonElement.ValueKind} failed" : null);
    }
    catch (Exception ex)
    {
      errorMessage = $"Error in deserializing JSON element of value kind {jsonElement.ValueKind}: {ex.StackTrace}";
      deserializedObject = null;
    }

    if (deserializedObject != null)
    {
      return errorMessage == null;
    }

    return false;
  }
}