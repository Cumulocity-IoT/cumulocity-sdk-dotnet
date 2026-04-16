using System.Diagnostics.CodeAnalysis;

using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Serialization.Json;

[ExcludeFromCodeCoverage]
public sealed class EventJsonConverter<T> : BaseJsonConverter<T> where T : Event, new()
{
  public EventJsonConverter()
    : base(AddCustomFragment, nameof(Event.CustomFragments), Event.Serialization.AdditionalPropertyClasses)
  {
  }

  private static void AddCustomFragment(T @event, IDictionary<string, object?> dictionary) =>
    @event.CustomFragments = dictionary;
}