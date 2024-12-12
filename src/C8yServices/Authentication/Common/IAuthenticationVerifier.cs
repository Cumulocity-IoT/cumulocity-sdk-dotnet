using Microsoft.AspNetCore.Authentication;

namespace C8yServices.Authentication.Common;

/// <summary>
/// Basic Credentials Verifier
/// </summary>
public interface IAuthenticationVerifier
{
  /// <summary>
  /// Verifies the credentials received via any authentication scheme
  /// </summary>
  /// <param name="headers">The headers used by the caller.</param>
  /// <param name="schemeName">The scheme that was used by the caller.</param>
  /// <returns></returns>
  Task<AuthenticationTicket?> AuthenticateAsync(IReadOnlyDictionary<string, string> headers, string schemeName);
}
