using C8yServices.Authentication.Basic;
using C8yServices.Authentication.Bearer;
using C8yServices.Authentication.Common;
using C8yServices.Authentication.OAuth;
using C8yServices.HealthAndMetrics;

using Microsoft.AspNetCore.Builder;

namespace SubscriptionHandlingExample;

public class Startup : HealthAndMetricsStartup
{
  public IConfiguration Configuration { get; }

  public Startup(IConfiguration configuration) : base(false)
  {
    Configuration = configuration;
  }

  override protected void AdditionalServiceConfiguration(IServiceCollection services)
  {
    services.AddAuthentication().AddBasicAuthentication<AuthenticationVerifier>();  // adds authentication scheme for BasicAuthentication
    services.AddAuthentication().AddOAuthAuthentication<AuthenticationVerifier>();  // adds authentication scheme for OAuth
    services.AddAuthentication().AddBearerToken<AuthenticationVerifier>();  // adds authentication scheme for Bearer tokens
    services.AddMemoryCache();
    services.AddControllers(options => options.EnableEndpointRouting = false);      // adds services to use controllers
  }

  override protected void AdditionalConfiguration(IApplicationBuilder applicationBuilder, IHostEnvironment hostEnvironment)
  {
    applicationBuilder.UseAuthentication();
    applicationBuilder.UseAuthorization();
    applicationBuilder.UseMvcWithDefaultRoute();
  }
}

