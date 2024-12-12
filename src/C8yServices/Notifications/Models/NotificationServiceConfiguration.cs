using System.Diagnostics.CodeAnalysis;

namespace C8yServices.Notifications.Models;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public sealed class NotificationServiceConfiguration
{
  public static string Section { get; set; } = "NotificationService";

  public TimeSpan OperationTimeout { get; init; } = TimeSpan.FromSeconds(30);

  public TimeSpan WebSocketClientMonitorInterval { get; init; } = TimeSpan.FromSeconds(20);

  public string NotificationEndpoint { get; init; } = "notification2";

  public TimeSpan TokenExpirationOffset { get; init; } = TimeSpan.FromSeconds(30);

  public required Uri BaseUrl { get; set; }
}