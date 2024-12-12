using Client.Com.Cumulocity.Client.Api;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace C8yServices.HealthAndMetrics;

public sealed class CumulocityApiHealthCheck : IHealthCheck
{
  private readonly ICurrentApplicationApi _currentApplicationApi;

  public CumulocityApiHealthCheck(ICurrentApplicationApi currentApplicationApi) =>
    _currentApplicationApi = currentApplicationApi;

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
  {
    try
    {
      await _currentApplicationApi.GetCurrentApplication(cancellationToken).ConfigureAwait(false);

      return HealthCheckResult.Healthy();
    }
    catch (Exception e)
    {
      return HealthCheckResult.Unhealthy("Generic exception", e);
    }
  }
}