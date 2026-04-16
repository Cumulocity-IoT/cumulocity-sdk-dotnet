using System.Diagnostics.CodeAnalysis;

using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Serialization.Json;

[ExcludeFromCodeCoverage]
public sealed class ManagedObjectJsonConverter<T> : BaseJsonConverter<T> where T : ManagedObject, new()
{
  public ManagedObjectJsonConverter()
    : base(AddCustomFragment, nameof(ManagedObject.CustomFragments), ManagedObject.Serialization.AdditionalPropertyClasses)
  {
  }

  private static void AddCustomFragment(T managedObject, IDictionary<string, object?> dictionary) =>
    managedObject.CustomFragments = dictionary;
}