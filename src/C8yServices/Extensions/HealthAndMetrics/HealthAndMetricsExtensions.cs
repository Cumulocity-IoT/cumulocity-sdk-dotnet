using C8yServices.HealthAndMetrics;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

using Prometheus;

namespace C8yServices.Extensions.HealthAndMetrics;

/// <summary>
/// Extensions to easily configure health and metrics endpoints
/// </summary>
public static class HealthAndMetricsExtensions
{
  /// <summary>
  /// Event sources to be included in metrics collection
  /// </summary>
  public static readonly string[] EventSources = ["System.Runtime"];

  /// <summary>
  /// configures health and metrics endpoints, listens on all IP addresses at given port
  /// </summary>
  /// <param name="hostBuilder"><c>IHostBuilder</c> instance</param>
  /// <param name="port">port to be used (default 5000)</param>
  /// <returns>this <c>IHostBuilder</c> instance</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public static IHostBuilder ConfigureHealthAndMetrics(this IHostBuilder hostBuilder, int port = 5000)
  {
    return hostBuilder.ConfigureHealthAndMetrics<HealthAndMetricsStartup>(port);
  }

  /// <summary>
  /// configures health and metrics endpoints by given startup configuration type, listens on all IP addresses at given port
  /// </summary>
  /// <typeparam name="T">type of the startup configuration to be used</typeparam>
  /// <param name="hostBuilder"><c>IHostBuilder</c> instance</param>
  /// <param name="port">port to be used (default 5000)</param>
  /// <returns>this <c>IHostBuilder</c> instance</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public static IHostBuilder ConfigureHealthAndMetrics<T>(this IHostBuilder hostBuilder, int port = 5000) where T : HealthAndMetricsStartup
  {
    if (hostBuilder == null)
      throw new ArgumentNullException(nameof(hostBuilder));

    Metrics.SuppressDefaultMetrics(new SuppressDefaultMetricOptions
    {
      SuppressDebugMetrics = true,
      SuppressEventCounters = false,
      SuppressMeters = true,
      SuppressProcessMetrics = false
    });

    Metrics.ConfigureEventCounterAdapter(options => options.EventSourceFilterPredicate = source => EventSources.Contains(source));

    if (port <= 0)
      port = 5000;

    return hostBuilder
      .ConfigureWebHostDefaults(builder => builder
        .UseStartup<T>()
        .UseUrls($"http://*:{port}"))
      .ConfigureServices(services => services.AddHealthChecks());
  }

  /// <summary>
  /// adds an individual <c>IHealthCheck</c> implementation 
  /// </summary>
  /// <typeparam name="T">type of the <c>IHealthCheck</c> implementation</typeparam>
  /// <param name="hostBuilder"><c>IHostBuilder</c> instance to be used</param>
  /// <param name="name">name of the health check</param>
  /// <param name="failureStatus">(optional) failure status</param>
  /// <param name="tags">(optional) tags</param>
  /// <returns>the used <c>IHostBuilder</c> instance</returns>
  /// <exception cref="ArgumentNullException"></exception>
  public static IHostBuilder AddHealthCheck<T>(this IHostBuilder hostBuilder, string name, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null) where T : class, IHealthCheck
  {
    if (hostBuilder is null)
    {
      throw new ArgumentNullException(nameof(hostBuilder));
    }
    if (name is null)
    {
      throw new ArgumentNullException(nameof(name));
    }

    return hostBuilder.ConfigureServices(services => services.AddHealthChecks().AddCheck<T>(name, failureStatus, tags ?? Array.Empty<string>()));
  }
}