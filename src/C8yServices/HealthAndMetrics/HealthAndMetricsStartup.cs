using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Prometheus;

namespace C8yServices.HealthAndMetrics;

/// <summary>
/// Startup configuration for the health and metrics endpoints (used by <c>HealthAndMetricsExtensions</c>)
/// </summary>
public class HealthAndMetricsStartup
{
  /// <summary>
  /// base path for health and metrics endpoints
  /// </summary>
  public const string BasePath = "/data";

  /// <summary>
  /// path of the health endpoint
  /// </summary>
  public const string HealthEndpointPath = $"{BasePath}/health";

  /// <summary>
  /// path of the (Prometheus) metrics endpoint
  /// </summary>
  public const string MetricsEndpointPath = $"{BasePath}/metrics";

  /// <summary>
  /// indicates if MVC is enabled
  /// </summary>
  public bool EnableMVC { get; }

  /// <summary>
  /// default constructor
  /// </summary>
  /// <param name="enableMVC">enable MVC</param>
  public HealthAndMetricsStartup(bool enableMVC = false)
  {
    EnableMVC = enableMVC;
  }

  /// <summary>
  /// overridable method for additional configuration (is called by <c>HealthAndMetricsStartup.Configure</c>)
  /// </summary>
  /// <param name="applicationBuilder"><c>IApplicationBuilder</c> instance</param>
  /// <param name="hostEnvironment"><c>IHostEnvironment</c> instance</param>
  protected virtual void AdditionalConfiguration(IApplicationBuilder applicationBuilder, IHostEnvironment hostEnvironment)
  {
  }

  /// <summary>
  /// overridable method for additional service configuration (is called by <c>HealthAndMetricsStartup.ConfigureServices</c>)
  /// </summary>
  /// <param name="services"><c>IServiceCollection</c> instance</param>
  protected virtual void AdditionalServiceConfiguration(IServiceCollection services)
  {
  }

  /// <summary>
  /// base configuration for endpoints
  /// </summary>
  /// <param name="applicationBuilder"><c>IApplicationBuilder</c> instance</param>
  /// <param name="hostEnvironment"><c>IHostEnvironment</c> instance</param>
  public void Configure(IApplicationBuilder applicationBuilder, IHostEnvironment hostEnvironment)
  {
    applicationBuilder
      .UseRouting()
      .UseEndpoints(endpoints =>
        {
          endpoints.MapHealthChecks(HealthEndpointPath, new HealthCheckOptions { ResponseWriter = HealthResponseJsonFormatter.FormatResponse });
          endpoints.MapMetrics(MetricsEndpointPath);
        });

    if (EnableMVC)
      applicationBuilder.UseMvc();

    AdditionalConfiguration(applicationBuilder, hostEnvironment);
  }

  /// <summary>
  /// base service configuration for endpoints
  /// </summary>
  /// <param name="services"><c>IServiceCollection</c> instance</param>
  public void ConfigureServices(IServiceCollection services)
  {
    services.AddHealthChecks();

    if (EnableMVC)
      services.AddMvc(options => options.EnableEndpointRouting = false);

    AdditionalServiceConfiguration(services);
  }
}