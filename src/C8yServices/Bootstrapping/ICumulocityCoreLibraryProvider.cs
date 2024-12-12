using Client.Com.Cumulocity.Client.Supplementary;

namespace C8yServices.Bootstrapping;

public interface ICumulocityCoreLibraryProvider
{
  IReadOnlyCollection<string> GetAllSubscribedTenants();
  ICumulocityCoreLibrary? GetForTenant(string tenantId);
  void UpdateCumulocityApiCredentials(IEnumerable<Credentials> credentials);
  event EventHandler<string> SubscriptionAddedEventHandler;
  event EventHandler<string> SubscriptionRemovedEventHandler;
}
