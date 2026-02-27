using C8yServices.Common.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class TokenService : ITokenService
{
  public Task<OneOf<string, ApiError>> CreateToken(ITokensApi tokensApi, TokenClaim tokenClaim, CancellationToken token = default)
  {
    return FlowWrappers.HandleOneCallWithHttpRequestException((tokenClaim, tokensApi),
      static (p, token) => CreateTokenInt(p.tokensApi, p.tokenClaim, token), token);
  }


  public Task<OneOf<Success, NotFound, ApiError>> Unsubscribe(ITokensApi tokensApi, string token, CancellationToken cancellationToken = default)
  {
    return FlowWrappers.HandleOneCallWithHttpRequestException((token, tokensApi), static (p, cancellationToken) => UnsubscribeInt(p.tokensApi, p.token, cancellationToken), cancellationToken);
  }

  private static async Task<OneOf<string, ApiError>> CreateTokenInt(ITokensApi tokensApi, TokenClaim tokenClaim, CancellationToken token = default)
  {
    var claims = new NotificationTokenClaims(tokenClaim.Subscriber, tokenClaim.Subscription)
    {
      NonPersistent = tokenClaim.NonPersistent
    };
    var result = await tokensApi.CreateToken(claims, cToken: token).ConfigureAwait(false);
    var resultToken = result?.Token;

    return resultToken is null ? Constants.NullResultApiError : resultToken;
  }

  private static async Task<OneOf<Success, NotFound, ApiError>> UnsubscribeInt(ITokensApi tokensApi, string token, CancellationToken cancellationToken = default)
  {
    var result = await tokensApi.UnsubscribeSubscriber(token: token, cToken: cancellationToken).ConfigureAwait(false);
    var pResult = result?.PResult;

    return pResult is null ? Constants.NullResultApiError : new Success();
  }
}