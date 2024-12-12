using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace C8yServices.Extensions.HealthAndMetrics;

public static class HealthCheckWrapper
{
  public static async Task<HealthCheckResult> GetWithException<TParam>(TParam param, Func<TParam, CancellationToken, Task<HealthCheckResult>> checkFunc, CancellationToken token)
  {
    try
    {
      return await checkFunc(param, token).ConfigureAwait(false);
    }
    catch (Exception e)
    {
      return new HealthCheckResult(HealthStatus.Unhealthy, exception: e);
    }
  }
}