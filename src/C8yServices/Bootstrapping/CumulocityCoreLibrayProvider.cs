using System.Collections.Concurrent;

using C8yServices.Configuration;
using C8yServices.Extensions.Http;

using Client.Com.Cumulocity.Client.Supplementary;

using Microsoft.Extensions.Options;

namespace C8yServices.Bootstrapping;

internal class CumulocityCoreLibrayProvider : ICumulocityCoreLibraryProvider
{
  private readonly ConcurrentDictionary<string, CumulocityApiCacheEntry> _cumulocityApiCache = [];
  private readonly Uri _baseUrl;
  public event EventHandler<string> SubscriptionAddedEventHandler = delegate { };
  public event EventHandler<string> SubscriptionRemovedEventHandler = delegate { };

  public CumulocityCoreLibrayProvider(IOptions<C8YConfiguration> c8YConfiguration)
  {
    _baseUrl = c8YConfiguration.Value.BaseUrl!;
  }

  public IReadOnlyCollection<string> GetAllSubscribedTenants()
  {
    return _cumulocityApiCache.Keys.ToList();
  }

  public ICumulocityCoreLibrary? GetForTenant(string tenantId)
  {
    return _cumulocityApiCache.TryGetValue(tenantId, out var api) ? api.CumulocityCoreLibrary : null;
  }

  public void UpdateCumulocityApiCredentials(IEnumerable<Credentials> credentials)
  {
    var newSubscriptions = new List<string>();
    foreach (var credentialsItem in credentials)
    {
      _cumulocityApiCache.AddOrUpdate(credentialsItem.Tenant, (key) =>
      {
        var httpClient = new HttpClient
        {
          BaseAddress = _baseUrl,
        };
        httpClient.UpdateBasicAuth(credentialsItem.Tenant, credentialsItem.User, credentialsItem.Password);
        var cumulocityCoreLibrary = new CumulocityCoreLibrary(httpClient);
        newSubscriptions.Add(credentialsItem.Tenant);
        return new CumulocityApiCacheEntry(cumulocityCoreLibrary, httpClient);
      }, (key, cumulocityCoreLibrary) =>
      {
        cumulocityCoreLibrary.HttpClient.UpdateBasicAuth(credentialsItem.Tenant, credentialsItem.User, credentialsItem.Password);
        return cumulocityCoreLibrary;
      });
    }

    // invoke event handlers for new subscriptions
    foreach (var newSubscription in newSubscriptions) 
    {
      SubscriptionAddedEventHandler.Invoke(this, newSubscription);
    }

    foreach (var tenant in _cumulocityApiCache.Keys)
    {
      if (!credentials.Any(credential => credential.Tenant.Equals(tenant, StringComparison.Ordinal)))
      {
        SubscriptionRemovedEventHandler.Invoke(this, tenant);
        _cumulocityApiCache.Remove(tenant, out _);
      }
    }
  }

  public record CumulocityApiCacheEntry(ICumulocityCoreLibrary CumulocityCoreLibrary, HttpClient HttpClient);
}
