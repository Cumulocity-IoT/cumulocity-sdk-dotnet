using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using C8yServices.Common.Models;

using C8yServices.Extensions.Notifications.Internal;
using C8yServices.Notifications.Models;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class NotificationService : INotificationService
{
  private readonly IRealTimeWebSocketClientFactory _realTimeWebSocketClientFactory;
  private readonly INotificationServiceHelper _notificationServiceHelper;
  private readonly ConcurrentDictionary<string, IRealTimeWebSocketClient> _clients;

  public NotificationService(IRealTimeWebSocketClientFactory realTimeWebSocketClientFactory,
    INotificationServiceHelper notificationServiceHelper, ConcurrentDictionary<string, IRealTimeWebSocketClient> clients)
  {
    _clients = clients;
    _realTimeWebSocketClientFactory = realTimeWebSocketClientFactory;
    _notificationServiceHelper = notificationServiceHelper;
  }

  public async Task<OneOf<Success, TenantSubscriptionError, ApiError>> Register(string tenantId, WithHandlerRegisterNotification withHandlerRegisterNotification, CancellationToken cancellationToken = default)
  {
    var subscriptionName = withHandlerRegisterNotification.RegisterNotification.SubscriptionName;
    var subscription = withHandlerRegisterNotification.RegisterNotification.ToSubscription();
    var getTokenResult = await _notificationServiceHelper.GetToken(tenantId, subscription, cancellationToken).ConfigureAwait(false);
    if (getTokenResult.IsT1)
    {
      return getTokenResult.AsT1;
    }
    if (getTokenResult.IsT2)
    {
      return getTokenResult.AsT2;
    }
    var tokenResult = getTokenResult.AsT0;
    var realTimeWebSocketClient = await _realTimeWebSocketClientFactory.Create(withHandlerRegisterNotification.DataFeedHandler, cancellationToken).ConfigureAwait(false);
    var connectResult = await realTimeWebSocketClient.Connect(tenantId, tokenResult, cancellationToken).ConfigureAwait(false);
    if (connectResult is not null)
    {
      return new ApiError(connectResult.Message, null);
    }
    _clients.AddOrUpdate(tenantId + "_" + subscriptionName, static (_, p) => p, static (_, _, p) => p, realTimeWebSocketClient);

    return OneOf<Success, TenantSubscriptionError, ApiError>.FromT0(new Success());
  }

  [ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
  public async Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken = default)
  {
    if (_clients.TryRemove(tenantId + "_" + subscriptionName, out var client))
    {
      await client.Disconnect(cancellationToken).ConfigureAwait(false);
    }
    return await _notificationServiceHelper.DeleteSubscription(tenantId, subscriptionName, cancellationToken);
  }


  public OneOf<WebSocketState, NotFound> GetWebSocketState(string tenantId, string subscriptionName) => 
    _clients.TryGetValue(tenantId + "_" + subscriptionName, out var value) ? GetResult(value) : new NotFound();
  
  public async Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> Unregister(string tenantId, string subscriptionName, CancellationToken cancellationToken = default)
  {
    if (!_clients.TryRemove(tenantId + "_" + subscriptionName, out var value) || value.Token is null)
    {
      return new NotFound();
    }
    string tokenValue = value.Token;
    await value.Disconnect(cancellationToken).ConfigureAwait(false);

    return await _notificationServiceHelper.Unsubscribe(tenantId, tokenValue, cancellationToken).ConfigureAwait(false);
  }

  public async ValueTask DisposeAsync()
  {
    foreach (var realTimeWebSocketClient in _clients)
    {
      await realTimeWebSocketClient.Value.DisposeAsync();
    }
    _clients.Clear();
  }

  private static OneOf<WebSocketState, NotFound> GetResult(IRealTimeWebSocketClient client) =>
    client.State is not null ? client.State.Value : new NotFound();
}