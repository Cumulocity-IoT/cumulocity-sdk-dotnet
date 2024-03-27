using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using C8yServices.Authentication.Common;

namespace C8yServices.Extensions.Security;
public static class ClaimsPrincipalExtensions
{
  public static string? GetC8yUsername(this ClaimsPrincipal claimsPrincipal)
  {
    return claimsPrincipal.FindFirstValue(ClaimTypes.Name);
  }

  public static string? GetC8yTenant(this ClaimsPrincipal claimsPrincipal)
  {
    return claimsPrincipal.FindFirstValue(CustomClaimTypes.UserTenant);
  }

  public static IEnumerable<string> GetC8yRoles(this ClaimsPrincipal claimsPrincipal)
  {
    return claimsPrincipal.FindAll(ClaimTypes.Role).Select(claim => claim.Value);
  }
}
