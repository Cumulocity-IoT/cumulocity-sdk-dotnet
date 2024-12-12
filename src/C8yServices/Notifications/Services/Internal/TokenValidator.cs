using C8yServices.Notifications.Models;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class TokenValidator : ITokenValidator
{
  private readonly TimeProvider _timeProvider;
  private readonly TimeSpan _tokenExpirationOffset;
  private readonly JsonWebTokenHandler _jsonWebTokenHandler = new();

  public TokenValidator(TimeProvider timeProvider, IOptions<NotificationServiceConfiguration> options)
  {
    _timeProvider = timeProvider;
    _tokenExpirationOffset = options.Value.TokenExpirationOffset;
  }

  public bool IsExpired(string tokenString)
  {
    var token = _jsonWebTokenHandler.ReadJsonWebToken(tokenString);
    var now = _timeProvider.GetUtcNow();
    var validTo = token.ValidTo;
    var validToWithOffset = validTo.Subtract(_tokenExpirationOffset);

    return now > validToWithOffset;
  }
}