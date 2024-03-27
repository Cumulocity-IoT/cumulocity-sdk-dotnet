using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace C8yServices.HealthAndMetrics;

/// <summary>
/// utility class to format the body of health endpoint responses as JSON
/// </summary>
public static class HealthResponseJsonFormatter
{
  private static readonly Dictionary<HealthStatus, string> HealthStatusText = new()
  {
    { HealthStatus.Healthy, "UP" },
    { HealthStatus.Degraded, "DEGRADED" },
    { HealthStatus.Unhealthy, "DOWN" }
  };

  /// <summary>
  /// formats the body of an health endpoint response as JSON
  /// </summary>
  public static Task FormatResponse(HttpContext context, HealthReport healthReport)
  {
    context.Response.ContentType = "application/json; charset=utf-8";

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, new JsonWriterOptions { Indented = true }))
    {
      jsonWriter.WriteStartObject();
      jsonWriter.WriteString("status", HealthStatusText[healthReport.Status]);
      jsonWriter.WriteComponents(healthReport.Entries);
      jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
  }

  /// <summary>
  /// adds a JSON array of given <c>HealthReportEntry</c>s as property 'components'
  /// </summary>
  private static void WriteComponents(this Utf8JsonWriter jsonWriter, IReadOnlyDictionary<string, HealthReportEntry> healthReportEntries)
  {
    jsonWriter.WriteStartArray("components");
    foreach (var entry in healthReportEntries)
    {
      var healthReportEntry = entry.Value;
      jsonWriter.WriteStartObject();
      jsonWriter.WriteString("name", entry.Key);
      jsonWriter.WriteString("status", HealthStatusText[healthReportEntry.Status]);
      jsonWriter.WriteComponentData(healthReportEntry.Data);
      jsonWriter.WriteEndObject();
    }
    jsonWriter.WriteEndArray();
  }

  /// <summary>
  /// adds a JSON object as property 'data' that contains the given key/value pairs
  /// </summary>
  private static void WriteComponentData(this Utf8JsonWriter jsonWriter, IReadOnlyDictionary<string, object> componentData)
  {
    if (componentData == null || componentData.Count == 0)
      return;

    jsonWriter.WriteStartObject("data");
    foreach (var entry in componentData)
      jsonWriter.WriteObjectValue(entry.Key, entry.Value);
    jsonWriter.WriteEndObject();
  }

  /// <summary>
  /// writes the property with given name depending on the type of 'value'
  /// </summary>
  private static void WriteObjectValue(this Utf8JsonWriter jsonWriter, string propertyName, object value)
  {
    if (value is decimal decimalValue)
      jsonWriter.WriteNumber(propertyName, decimalValue);
    else if (value is double doubleValue)
      jsonWriter.WriteNumber(propertyName, doubleValue);
    else if (value is float floatValue)
      jsonWriter.WriteNumber(propertyName, floatValue);
    else if (value is int intValue)
      jsonWriter.WriteNumber(propertyName, intValue);
    else if (value is long longValue)
      jsonWriter.WriteNumber(propertyName, longValue);
    else if (value is uint uintValue)
      jsonWriter.WriteNumber(propertyName, uintValue);
    else if (value is ulong ulongValue)
      jsonWriter.WriteNumber(propertyName, ulongValue);
    else
      jsonWriter.WriteString(propertyName, $"{value}");
  }
}