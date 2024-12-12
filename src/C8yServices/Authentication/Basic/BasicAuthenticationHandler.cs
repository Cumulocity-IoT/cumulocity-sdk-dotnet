using System.Text.Encodings.Web;

using C8yServices.Authentication.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace C8yServices.Authentication.Basic;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
  private readonly IAuthenticationVerifier _authenticationVerifier;

  public BasicAuthenticationHandler(
    IAuthenticationVerifier authenticationVerifier,
    IOptionsMonitor<BasicAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder)
    => _authenticationVerifier = authenticationVerifier ?? throw new ArgumentNullException(nameof(authenticationVerifier));

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    string? auth = Request.Headers.Authorization;
    if (string.IsNullOrEmpty(auth) || !auth.StartsWith($"{BasicAuthenticationDefaults.AuthenticationScheme} ", StringComparison.OrdinalIgnoreCase))
    {
      return AuthenticateResult.NoResult();
    }

    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, auth }
    };

    var authResult = await _authenticationVerifier.AuthenticateAsync(headers, Scheme.Name).ConfigureAwait(false);
    return authResult is not null ? AuthenticateResult.Success(authResult) : AuthenticateResult.Fail("Failed to validate basic credentials.");
  }
}