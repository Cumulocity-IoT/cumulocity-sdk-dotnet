using System.Collections.Generic;
using System.Text.Json.Serialization;

using C8yServices.Inventory;

namespace RestControllerExample.Models;
public class ExampleQueryableManagedObject : QueryableManagedObject
{
  public const string TypeName = "example_managedObject";
  public const string ExampleFragmentName = "example_fragment";

  public ExampleQueryableManagedObject(string exampleFragment)
  {
    Type = TypeName;
    ExampleFragment = exampleFragment;
  }

  [JsonPropertyName(ExampleFragmentName)]
  public string ExampleFragment { get; set; }

  /// <summary>
  /// Parameters to be used for requesting example managed objects
  /// </summary>
  [JsonIgnore]
  new public static Dictionary<RequestParameterType, string> RequestParameters
    => new() { { RequestParameterType.Type, TypeName } }; // use the type of this managed object to query for all

}
