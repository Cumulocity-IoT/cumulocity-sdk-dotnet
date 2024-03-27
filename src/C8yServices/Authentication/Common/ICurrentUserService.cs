

using System.Collections.Generic;
using System.Threading.Tasks;

using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Authentication.Common;
public interface ICurrentUserService
{
  Task<CurrentUser?> GetCurrentUser(IReadOnlyDictionary<string, string> headers);
}
