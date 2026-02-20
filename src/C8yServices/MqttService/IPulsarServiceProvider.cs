using C8yServices.Subscriptions;

namespace C8yServices.MqttService;

public interface IPulsarServiceProvider : ICredentialAwareService
{
  IEnumerable<string> GetAllSubscribedTenants();
  IPulsarService? GetForTenant(string tenant);
}
