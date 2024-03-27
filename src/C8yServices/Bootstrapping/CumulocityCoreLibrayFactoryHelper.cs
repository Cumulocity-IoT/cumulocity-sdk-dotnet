
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Bootstrapping;

public sealed class CumulocityCoreLibrayFactoryHelper
{
  private readonly ICurrentApplicationApi _currentApplicationApi;

  public CumulocityCoreLibrayFactoryHelper(ICurrentApplicationApi currentApplicationApi)
  {
    _currentApplicationApi = currentApplicationApi;
  }

  public async Task<IEnumerable<Credentials>> GetApiCredentials(CancellationToken token = default)
  {
    var subscribedUsers = await GetUsers(token);
    return subscribedUsers.Select(subscribedUser => new Credentials(subscribedUser.Tenant ?? string.Empty, subscribedUser.Name ?? string.Empty, subscribedUser.Password ?? string.Empty)).ToList();
  }

  private async Task<IEnumerable<ApplicationUserCollection.Users>> GetUsers(CancellationToken token = default)
  {
    var subscriptions = await _currentApplicationApi.GetSubscribedUsers(token);

    return subscriptions is not null ? subscriptions.PUsers : new();
  }
}