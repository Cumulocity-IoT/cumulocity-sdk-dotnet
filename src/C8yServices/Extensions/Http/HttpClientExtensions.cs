using System.Net.Http.Headers;
using System.Text;

namespace C8yServices.Extensions.Http;

/// <summary>
/// provides extensions for <c>HttpClient</c>s
/// </summary>
public static class HttpClientExtensions
{
  /// <summary>
  /// Updates the basic authentication.
  /// </summary>
  /// <param name="httpClient">The HTTP client.</param>
  /// <param name="tenant">The tenant.</param>
  /// <param name="user">The user.</param>
  /// <param name="password">The password.</param>
  public static void UpdateBasicAuth(this HttpClient httpClient, string tenant, string user, string password) =>
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{tenant}/{user}:{password}")));
}