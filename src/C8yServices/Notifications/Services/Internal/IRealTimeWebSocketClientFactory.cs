namespace C8yServices.Notifications.Services.Internal;

internal interface IRealTimeWebSocketClientFactory : IDisposable
{
  Task<IRealTimeWebSocketClient> Create(IDataFeedHandler dataFeedHandler, CancellationToken token);
}