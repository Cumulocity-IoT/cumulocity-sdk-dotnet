using System.Net.WebSockets;

using C8yServices.Common.Models;

using C8yServices.Notifications.Models;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services;

/// <summary>
/// The wrapper of Notification Service based on Cumulocity Notification Api 2.0
/// </summary>
public interface INotificationService : IAsyncDisposable
{
  /// <summary>
  /// Creates or gets subscription in Cumulocity, creates new token and start listening on the handler.
  /// </summary>
  Task<OneOf<Success, TenantSubscriptionError, ApiError>> Register(WithHandlerRegisterNotification withHandlerRegisterNotification, CancellationToken cancellationToken = default);

  /// <summary>
  /// Stops listening on the handler and unsubscribe token in Cumulocity. The subscription is still existing afterwards.
  /// </summary>
  Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> Unregister(string subscriptionName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Stops listening on the handler and subscribe token in Cumulocity. This operation also deletes the subscription in Cumulocity.
  /// </summary>
  Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> DeleteSubscription(string subscriptionName, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets web socket state.
  /// </summary>
  OneOf<WebSocketState, NotFound> GetWebSocketState(string subscriptionName);
}