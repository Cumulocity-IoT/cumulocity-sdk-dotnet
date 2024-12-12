using System.Diagnostics.CodeAnalysis;

using C8yServices.Configuration;
using C8yServices.Extensions.Configuration;
using C8yServices.Notifications.Models;

using Microsoft.Extensions.Options;

namespace C8yServices.Notifications.Services;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public sealed class NotificationServiceConfigurationProvider : INotificationServiceConfigurationProvider
{
  private readonly IOptions<NotificationServiceConfiguration> _notificationOptions;
  private readonly IOptions<C8YConfiguration> _c8YOptions;

  public NotificationServiceConfigurationProvider(IOptions<NotificationServiceConfiguration> notificationOptions, IOptions<C8YConfiguration> c8YOptions)
  {
    _notificationOptions = notificationOptions;
    _c8YOptions = c8YOptions;
  }

  public ValueTask<NotificationServiceConfiguration> Get(CancellationToken token) =>
    new(new NotificationServiceConfiguration
    {
      BaseUrl = new Uri(_c8YOptions.Value.GetWebSocketUrl()),
      NotificationEndpoint = _notificationOptions.Value.NotificationEndpoint,
      OperationTimeout = _notificationOptions.Value.OperationTimeout,
      WebSocketClientMonitorInterval = _notificationOptions.Value.WebSocketClientMonitorInterval,
      TokenExpirationOffset = _notificationOptions.Value.TokenExpirationOffset
    });
}