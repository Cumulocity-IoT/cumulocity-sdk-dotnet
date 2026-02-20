using System.Collections.Concurrent;

using C8yServices.Configuration;
using C8yServices.Extensions.Http;
using C8yServices.Subscriptions;

using Client.Com.Cumulocity.Client.Supplementary;

using Microsoft.Extensions.Options;

namespace C8yServices.RestApi;

internal class CumulocityCoreLibrayProvider : ICumulocityCoreLibraryProvider
{
  private readonly ConcurrentDictionary<string, CumulocityApiCacheEntry> _cumulocityApiCache = new();
  private readonly Uri _baseUrl;

  public CumulocityCoreLibrayProvider(IOptions<C8YConfiguration> c8YConfiguration, IServiceCredentialsFactory serviceCredentialsFactory)
  {
    _baseUrl = c8YConfiguration.Value.BaseUrl!;
    serviceCredentialsFactory.ApiCredentialsUpdated += UpdateCumulocityApiCredentials;
  }

  public IReadOnlyCollection<string> GetAllSubscribedTenants()
  {
    return _cumulocityApiCache.Keys.ToList();
  }

  public ICumulocityCoreLibrary? GetForTenant(string tenantId)
  {
    return _cumulocityApiCache.TryGetValue(tenantId, out var api) ? api.CumulocityCoreLibrary : null;
  }

  private void UpdateCumulocityApiCredentials(object? sender, ServiceCredentials credentials)
  {
    var newSubscriptions = new List<string>();
    foreach (var credentialsItem in new[] { credentials })
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
  }

  public record CumulocityApiCacheEntry(ICumulocityCoreLibrary CumulocityCoreLibrary, HttpClient HttpClient);
}
