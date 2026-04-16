using System.Diagnostics.CodeAnalysis;

using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Serialization.Json;

[ExcludeFromCodeCoverage]
public sealed class AlarmJsonConverter<T> : BaseJsonConverter<T> where T : Alarm, new()
{
  public AlarmJsonConverter()
    : base(AddCustomFragment, nameof(Alarm.CustomFragments), Alarm.Serialization.AdditionalPropertyClasses)
  {
  }

  private static void AddCustomFragment(T alarm, IDictionary<string, object?> dictionary) =>
    alarm.CustomFragments = dictionary;
}