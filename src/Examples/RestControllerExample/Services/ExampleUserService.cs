using System.Net;

using C8yServices.RestApi;

using Client.Com.Cumulocity.Client.Model;
using Client.Com.Cumulocity.Client.Supplementary;

using OneOf;

namespace RestControllerExample.Services;
public sealed class ExampleUserService : IExampleUserService
{
  private readonly ICumulocityCoreLibraryProvider _c8yApiProvider;

  public ExampleUserService(ICumulocityCoreLibraryProvider c8yApiProvider)
  {
    _c8yApiProvider = c8yApiProvider;
  }

  public async Task<OneOf<User<CustomProperties>?, OneOf.Types.Error>> GetOwnUserRepresentation(string tenant, string userName, CancellationToken token)
  {
    var cumulocityCoreLibrary = _c8yApiProvider.GetForTenant(tenant);

    if (cumulocityCoreLibrary is null)
    {
      return new OneOf.Types.Error();
    }

    try
    {
      return await cumulocityCoreLibrary.Users.UsersApi.GetUserByUsername<CustomProperties>(tenant, userName, token);
    }
    catch (Exception ex)
    {
      return ex is HttpRequestException exception && exception.GetStatusCode() == HttpStatusCode.NotFound
        ? null
        : new OneOf.Types.Error();
    }

  }
}
