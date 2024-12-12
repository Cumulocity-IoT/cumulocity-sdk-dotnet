using C8yServices.Common.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal interface ITokenService
{
  Task<OneOf<string, ApiError>> CreateToken(ITokensApi tokensApi, TokenClaim tokenClaim, CancellationToken token = default);
  Task<OneOf<Success, NotFound, ApiError>> Unsubscribe(ITokensApi tokensApi, string token, CancellationToken cancellationToken = default);
}