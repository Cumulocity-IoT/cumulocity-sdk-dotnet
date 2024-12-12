using C8yServices.Common.Models;
using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Notifications.Services.Internal;

internal interface ITokenProvider
{
  Task<OneOf.OneOf<string?, TenantSubscriptionError, ApiError>> GetTokenIfExpired(string tenantId, TokenClaim tokenClaim, string token, CancellationToken cancellationToken);
}