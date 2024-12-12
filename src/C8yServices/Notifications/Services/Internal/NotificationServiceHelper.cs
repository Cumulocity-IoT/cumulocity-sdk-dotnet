using System.Diagnostics.CodeAnalysis;

using C8yServices.Bootstrapping;

using C8yServices.Common.Models;

using C8yServices.Notifications.Models.Internal;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class NotificationServiceHelper : INotificationServiceHelper
{
  private readonly ICumulocityCoreLibraryProvider _cumulocityCoreLibraryProvider;
  private readonly ITokenService _tokenService;
  private readonly ISubscriptionService _subscriptionService;

  public NotificationServiceHelper(ICumulocityCoreLibraryProvider cumulocityCoreLibraryProvider, ITokenService tokenService, ISubscriptionService subscriptionService)
  {
    _cumulocityCoreLibraryProvider = cumulocityCoreLibraryProvider;
    _tokenService = tokenService;
    _subscriptionService = subscriptionService;
  }

  public async Task<OneOf<TokenClaimWithToken, TenantSubscriptionError, ApiError>> GetToken(string tenantId, Subscription subscription, CancellationToken cancellationToken = default)
  {    
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenantId);
    if (cumulocityCoreLibrary is null) 
    {
      return OneOf<TokenClaimWithToken, TenantSubscriptionError, ApiError>.FromT1(new TenantSubscriptionError(tenantId));
    }
    var subscriptionsApi = cumulocityCoreLibrary.Notifications20.SubscriptionsApi;
    var tokensApi = cumulocityCoreLibrary.Notifications20.TokensApi;
    var getSubscriptionResult = await _subscriptionService.Get(subscriptionsApi, subscription.Name, cancellationToken).ConfigureAwait(false);
    if (getSubscriptionResult.IsT1)
    {
      return getSubscriptionResult.AsT1;
    }
    var subscriptionId = getSubscriptionResult.AsT0;
    if (subscriptionId is not null)
    {
      var tokenClaim1 = new TokenClaim(subscriptionId, subscription.Name);
      var createTokenResult1 = await _tokenService.CreateToken(tokensApi, tokenClaim1, cancellationToken).ConfigureAwait(false);

      return createTokenResult1.IsT0 ? new TokenClaimWithToken(tokenClaim1, createTokenResult1.AsT0) : createTokenResult1.AsT1;
    }
    var createSubscriptionResult = await _subscriptionService.Create(subscriptionsApi, subscription, cancellationToken).ConfigureAwait(false);
    if (createSubscriptionResult.IsT1)
    {
      return createSubscriptionResult.AsT1;
    }
    subscriptionId = createSubscriptionResult.AsT0;
    var tokenClaim2 = new TokenClaim(subscriptionId, subscription.Name);
    var createTokenResult2 = await _tokenService.CreateToken(tokensApi, tokenClaim2, cancellationToken).ConfigureAwait(false);

    return createTokenResult2.IsT0 ? new TokenClaimWithToken(tokenClaim2, createTokenResult2.AsT0) : createTokenResult2.AsT1;
  }

  [ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
  public async Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken = default)
  {
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenantId);
    if (cumulocityCoreLibrary is null) 
    {
      return OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT2(new TenantSubscriptionError(tenantId));
    }
    var result = await _subscriptionService.Delete(cumulocityCoreLibrary.Notifications20.SubscriptionsApi, subscriptionName, cancellationToken);
    return result.Match(
      OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT0,
      OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT1,
      OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT3
    );
  }


  [ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
  public async Task<OneOf<Success, NotFound, TenantSubscriptionError, ApiError>> Unsubscribe(string tenantId, string token, CancellationToken cancellationToken = default)
  {    
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenantId);
    if (cumulocityCoreLibrary is null) 
    {
      return OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT2(new TenantSubscriptionError(tenantId));
    }
    var result = await _tokenService.Unsubscribe(cumulocityCoreLibrary.Notifications20.TokensApi, token, cancellationToken);
    return result.Match(
      OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT0,
      OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT1,
      OneOf<Success, NotFound, TenantSubscriptionError, ApiError>.FromT3
    );
  }
}