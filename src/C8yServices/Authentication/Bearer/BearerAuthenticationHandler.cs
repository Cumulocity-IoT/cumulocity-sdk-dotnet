

using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using C8yServices.Authentication.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace C8yServices.Authentication.Bearer;

public sealed class BearerAuthenticationHandler : AuthenticationHandler<BearerAuthenticationOptions>
{
  private readonly IAuthenticationVerifier _authenticationVerifier;

  public BearerAuthenticationHandler(
    IAuthenticationVerifier authenticationVerifier,
    IOptionsMonitor<BearerAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder)
    => _authenticationVerifier = authenticationVerifier ?? throw new ArgumentNullException(nameof(authenticationVerifier));

  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    string? auth = Request.Headers.Authorization;
    if (string.IsNullOrEmpty(auth) || !auth.StartsWith($"{BearerAuthenticationDefaults.AuthenticationScheme} ", StringComparison.OrdinalIgnoreCase))
    {
      return AuthenticateResult.NoResult();
    }

    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, auth }
    };

    var authResult = await _authenticationVerifier.AuthenticateAsync(headers, Scheme.Name).ConfigureAwait(false);
    return authResult is not null ? AuthenticateResult.Success(authResult) : AuthenticateResult.Fail("Failed to validate bearer credentials.");
  }
}