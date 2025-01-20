using System.Reflection;
using System.Text.Json.Serialization;

using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Inventory;

/// <summary>
/// <c>QueryableManagedObject</c> is an abstract base class for all managed object classes that are queryable by
/// request parameters like 'type', 'fragmentType', 'text' or 'query'
/// 
/// <seealso href="https://cumulocity.com/api/core/#operation/getManagedObjectCollectionResource"/>
/// </summary>
public abstract class QueryableManagedObject : ManagedObject
{
  /// <summary>
  /// Enum of types of request parameters
  /// </summary>
  public enum RequestParameterType { Type, FragmentType, Text, Query }

  /// <summary>
  /// Parameters to be used for requesting managed objects
  /// </summary>
  [JsonIgnore]
  public static Dictionary<RequestParameterType, string> RequestParameters => throw new NotImplementedException();

  /// <summary>
  /// checks if given type is a <see cref="QueryableManagedObject"/> and returns the dictionary of request parameters (static method 'RequestParameters')
  /// </summary>
  public static Dictionary<RequestParameterType, string> GetRequestParameters<T>() where T : ManagedObject
  {
    if (!typeof(T).IsAssignableTo(typeof(QueryableManagedObject)))
    {
      return [];
    }

    try
    {
      var requestParametersProperty = GetRequestParametersProperty(typeof(T));
      return requestParametersProperty is null
        ? []
        : requestParametersProperty.GetValue(null) as Dictionary<RequestParameterType, string> ?? new();
    }
    catch (Exception)
    {
      return [];
    }
  }

  /// <summary>
  /// tries to get the property 'QueryableManagedObject.RequestParameters' from given type (subclass of 'QueryableManagedObject')
  /// </summary>
  private static PropertyInfo? GetRequestParametersProperty(Type type)
  {
    if (type == null)
      return null;

    try
    {
      var property = type.GetProperty(nameof(RequestParameters), typeof(Dictionary<RequestParameterType, string>));
      return property ?? GetRequestParametersProperty(type.BaseType!);
    }
    catch (Exception)
    {
      return null;
    }
  }
}