using C8yServices.Subscriptions;

using Client.Com.Cumulocity.Client.Supplementary;

namespace C8yServices.RestApi;

public interface ICumulocityCoreLibraryProvider : ICredentialAwareService
{
  IReadOnlyCollection<string> GetAllSubscribedTenants();
  ICumulocityCoreLibrary? GetForTenant(string tenantId);
}
