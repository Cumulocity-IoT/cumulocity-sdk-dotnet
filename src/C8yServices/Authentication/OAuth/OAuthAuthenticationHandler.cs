using System.Text.Encodings.Web;

using C8yServices.Authentication.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace C8yServices.Authentication.OAuth;

public sealed class OAuthAuthenticationHandler : AuthenticationHandler<OAuthAuthenticationOptions>
{
  private readonly IAuthenticationVerifier _authenticationVerifier;

  public OAuthAuthenticationHandler(
    IAuthenticationVerifier authCredentialVerifier,
    IOptionsMonitor<OAuthAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder)
  {
    _authenticationVerifier = authCredentialVerifier;
  }

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (!Request.Headers.TryGetValue(OAuthAuthenticationDefaults.XXsrfToken, out var token) || !Request.Headers.TryGetValue(OAuthAuthenticationDefaults.Cookie, out var cookie))
    {
      return AuthenticateResult.NoResult();
    }
    string? tokenAsString = token;
    string? cookieAsString = cookie;
    if (tokenAsString is null || cookieAsString is null)
    {
      return AuthenticateResult.Fail("Failed to validate oauth credentials.");
    }
    var headers = new Dictionary<string, string>
    {
      { OAuthAuthenticationDefaults.XXsrfToken, tokenAsString },
      { OAuthAuthenticationDefaults.Cookie, cookieAsString }
    };
    var authResult = await _authenticationVerifier.AuthenticateAsync(headers, Scheme.Name).ConfigureAwait(false);

    return authResult is not null ? AuthenticateResult.Success(authResult) : AuthenticateResult.Fail("Failed to validate oauth credentials.");
  }
}
