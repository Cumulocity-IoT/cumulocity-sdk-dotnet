using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;

namespace C8yServices.Subscriptions;

public sealed class ServiceCredentialsFactoryHelper : IServiceCredentialsFactoryHelper
{
  private readonly ICurrentApplicationApi _currentApplicationApi;

  public ServiceCredentialsFactoryHelper(ICurrentApplicationApi currentApplicationApi)
  {
    _currentApplicationApi = currentApplicationApi;
  }

  public async Task<IEnumerable<ServiceCredentials>> GetApiCredentials(CancellationToken token = default)
  {
    var subscribedUsers = await GetUsers(token);
    return subscribedUsers.Select(subscribedUser => new ServiceCredentials(subscribedUser.Tenant ?? string.Empty, subscribedUser.Name ?? string.Empty, subscribedUser.Password ?? string.Empty)).ToList();
  }

  private async Task<IEnumerable<ApplicationUserCollection.Users>> GetUsers(CancellationToken token = default)
  {
    var subscriptions = await _currentApplicationApi.GetSubscribedUsers(token);

    return subscriptions is not null ? subscriptions.PUsers : new();
  }
}