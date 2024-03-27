

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;

namespace C8yServices.Authentication.Common;

public sealed class AuthenticationVerifier : IAuthenticationVerifier
{
  private readonly ICurrentUserService _currentUserService;
  private readonly ICurrentTenantService _currentTenantService;

  public AuthenticationVerifier(ICurrentUserService currentUserService, ICurrentTenantService currentTenantService)
  {
    _currentUserService = currentUserService;
    _currentTenantService = currentTenantService;
  }

  public async Task<AuthenticationTicket?> AuthenticateAsync(IReadOnlyDictionary<string, string> headers, string schemeName)
  {
    var currentUser = await _currentUserService.GetCurrentUser(headers).ConfigureAwait(false);
    var currentTenant = await _currentTenantService.GetCurrentTenant(headers).ConfigureAwait(false);
    if (currentUser?.UserName is null || currentTenant?.Name is null)
    {
      return null;
    }
    var claims = new List<Claim> {
      new(ClaimTypes.Name, currentUser.UserName),
      new(CustomClaimTypes.UserTenant, currentTenant.Name)
    };
    foreach (var effectiveRole in currentUser.EffectiveRoles.Where(effectiveRole => effectiveRole.Id is not null))
    {
      claims.Add(new(ClaimTypes.Role, effectiveRole.Id!));
    }
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, schemeName));
    var ticket = new AuthenticationTicket(principal, schemeName);

    return ticket;
  }
}