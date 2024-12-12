using System.Net.Http.Json;

using C8yServices.Configuration;

using Client.Com.Cumulocity.Client.Model;

using Microsoft.Extensions.Options;

namespace C8yServices.Authentication.Common;

public sealed class CurrentTenantService : ICurrentTenantService
{
  public const string CurrentUserPath = "tenant/currentTenant";

  private readonly Uri _currentUserUri;
  private readonly HttpClient _httpClient;

  public CurrentTenantService(HttpClient httpClient, IOptions<C8YConfiguration> configuration)
  {
    _currentUserUri = new Uri($"{configuration.Value.BaseUrl}{CurrentUserPath}");
    _httpClient = httpClient;
  }

  public async Task<CurrentTenant<CustomProperties>?> GetCurrentTenant(IReadOnlyDictionary<string, string> headers)
  {
    var requestMessage = new HttpRequestMessage
    {
      RequestUri = _currentUserUri
    };
    foreach (var header in headers)
    {
      requestMessage.Headers.Add(header.Key, header.Value);
    }
    var responseMessage = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

    return responseMessage.IsSuccessStatusCode
      ? await responseMessage.Content.ReadFromJsonAsync<CurrentTenant<CustomProperties>>().ConfigureAwait(false)
      : null;
  }
}
