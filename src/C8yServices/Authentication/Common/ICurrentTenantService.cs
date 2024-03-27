

using System.Collections.Generic;
using System.Threading.Tasks;

using Client.Com.Cumulocity.Client.Model;


namespace C8yServices.Authentication.Common;
public interface ICurrentTenantService
{
  Task<CurrentTenant<CustomProperties>?> GetCurrentTenant(IReadOnlyDictionary<string, string> headers);
}
