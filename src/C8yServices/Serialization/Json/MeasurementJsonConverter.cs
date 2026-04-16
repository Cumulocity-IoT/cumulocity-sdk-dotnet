using System.Diagnostics.CodeAnalysis;

using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Serialization.Json;

[ExcludeFromCodeCoverage]
public sealed class MeasurementJsonConverter<T> : BaseJsonConverter<T> where T : Measurement, new()
{
  public MeasurementJsonConverter()
    : base(AddCustomFragment, nameof(Measurement.CustomFragments), Measurement.Serialization.AdditionalPropertyClasses)
  {
  }

  private static void AddCustomFragment(T measurement, IDictionary<string, object?> dictionary) =>
    measurement.CustomFragments = dictionary;
}