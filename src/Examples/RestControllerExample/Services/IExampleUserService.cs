using System.Threading;
using System.Threading.Tasks;

using Client.Com.Cumulocity.Client.Model;

using OneOf;

namespace RestControllerExample.Services;

public interface IExampleUserService
{
  public Task<OneOf<User<CustomProperties>?, OneOf.Types.Error>> GetOwnUserRepresentation(string tenant, string userName, CancellationToken token);
}
