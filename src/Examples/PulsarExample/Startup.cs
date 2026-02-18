using C8yServices.HealthAndMetrics;

using Microsoft.AspNetCore.Builder;

namespace PulsarExample;

public class Startup : HealthAndMetricsStartup
{
  public IConfiguration Configuration { get; }

  public Startup(IConfiguration configuration) : base(false)
  {
    Configuration = configuration;
  }

  override protected void AdditionalServiceConfiguration(IServiceCollection services)
  {
    // No additional services needed for console application
  }

  override protected void AdditionalConfiguration(IApplicationBuilder applicationBuilder, IHostEnvironment hostEnvironment)
  {
    // No additional middleware needed for console application
  }
}
