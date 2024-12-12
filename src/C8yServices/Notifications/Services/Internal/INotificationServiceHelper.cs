using C8yServices.Common.Models;
using C8yServices.Notifications.Models.Internal;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal interface INotificationServiceHelper
{
  Task<OneOf<TokenClaimWithToken, TenantSubscriptionError, ApiError>> GetToken(string tenantId, Subscription subscription, CancellationToken cancellationToken = default);
  Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken = default);
  Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> Unsubscribe(string tenantId, string token, CancellationToken cancellationToken = default);
}