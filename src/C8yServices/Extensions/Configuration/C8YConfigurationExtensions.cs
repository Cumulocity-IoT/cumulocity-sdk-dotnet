using C8yServices.Configuration;

namespace C8yServices.Extensions.Configuration;

public static class C8YConfigurationExtensions
{
  public static string? GetHostName(this C8YConfiguration configuration) =>
    configuration.BaseUrl?.Host;

  public static bool IsHttps(this C8YConfiguration configuration) =>
    configuration.BaseUrl?.Scheme.ToLowerInvariant() == "https";

  public static int? GetPort(this C8YConfiguration configuration) =>
    configuration.BaseUrl?.Port;

  public static string GetWebSocketUrl(this C8YConfiguration configuration)
  {
    var port = GetPort(configuration);
    var host = GetHostName(configuration);

    return !string.IsNullOrEmpty(host) ? GetUrlString(configuration, host, port) : throw new InvalidOperationException("host is null.");
  }

  private static string GetUrlString(C8YConfiguration configuration, string? host, int? port) => 
    $"{(IsHttps(configuration) ? "wss" : "ws")}://{host}{(port is null ? string.Empty : $":{port}")}/";
}