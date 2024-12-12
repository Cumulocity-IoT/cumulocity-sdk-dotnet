using System.Diagnostics.CodeAnalysis;

using C8yServices.Notifications.Models;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class RealTimeWebSocketClientFactory : IRealTimeWebSocketClientFactory
{
  private readonly ILogger<RealTimeWebSocketClientFactory> _logger;
  private readonly IClientWebSocketWrapperFactory _clientWebSocketWrapperFactory;
  private readonly INotificationServiceConfigurationProvider _notificationServiceConfigurationProvider;
  private readonly IMessageExtractor _messageExtractor;
  private readonly TimeProvider _timeProvider;
  private readonly Locker _locker = new();
  private NotificationServiceConfiguration? _notificationServiceConfiguration;

  public RealTimeWebSocketClientFactory(ILogger<RealTimeWebSocketClientFactory> logger, IClientWebSocketWrapperFactory clientWebSocketWrapperFactory, INotificationServiceConfigurationProvider notificationServiceConfigurationProvider,
    IMessageExtractor messageExtractor, TimeProvider timeProvider)
  {
    _logger = logger;
    _clientWebSocketWrapperFactory = clientWebSocketWrapperFactory;
    _notificationServiceConfigurationProvider = notificationServiceConfigurationProvider;
    _messageExtractor = messageExtractor;
    _timeProvider = timeProvider;
  }

  public async Task<IRealTimeWebSocketClient> Create(IDataFeedHandler dataFeedHandler, CancellationToken token)
  {
    var configuration = await _locker.GetValueAsync(static (p, token) => p.GetNotificationServiceConfiguration(token), this, 10, token).ConfigureAwait(false);

    return new RealTimeWebSocketClient(_logger, _clientWebSocketWrapperFactory, dataFeedHandler, configuration, _messageExtractor, _timeProvider);
  }

  private async Task<NotificationServiceConfiguration> GetNotificationServiceConfiguration(CancellationToken token)
  {
    if (_notificationServiceConfiguration is not null)
    {
      return _notificationServiceConfiguration;
    }
    _notificationServiceConfiguration = await _notificationServiceConfigurationProvider.Get(token).ConfigureAwait(false);

    return _notificationServiceConfiguration;
  }

  public void Dispose() => 
    _locker.Dispose();
}