using C8yServices.Bootstrapping;
using C8yServices.Common.Models;
using C8yServices.Notifications.Models.Internal;

using OneOf;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class TokenProvider : ITokenProvider
{
  private readonly ICumulocityCoreLibraryProvider _cumulocityCoreLibraryProvider;
  private readonly ITokenService _tokenService;
  private readonly ITokenValidator _tokenValidator;

  public TokenProvider(ICumulocityCoreLibraryProvider cumulocityCoreLibraryProvider, ITokenService tokenService, ITokenValidator tokenValidator)
  {
    _cumulocityCoreLibraryProvider = cumulocityCoreLibraryProvider;
    _tokenService = tokenService;
    _tokenValidator = tokenValidator;
  }

  public async Task<OneOf<string?, TenantSubscriptionError, ApiError>> GetTokenIfExpired(string tenantId, TokenClaim tokenClaim, string token, CancellationToken cancellationToken)
  {
    if (!_tokenValidator.IsExpired(token))
    {
      return OneOf<string?, TenantSubscriptionError, ApiError>.FromT0(null);
    }
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenantId);
    if (cumulocityCoreLibrary is null) 
    {
      return OneOf<string?, TenantSubscriptionError, ApiError>.FromT1(new TenantSubscriptionError(tenantId));
    }
    var result = await _tokenService.CreateToken(cumulocityCoreLibrary.Notifications20.TokensApi, tokenClaim, cancellationToken).ConfigureAwait(false);

    return result.IsT1 ? result.AsT1 : result.AsT0;
  }
}