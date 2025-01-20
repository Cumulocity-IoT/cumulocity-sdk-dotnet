using System.Diagnostics.CodeAnalysis;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class Locker : IDisposable
{
  private readonly SemaphoreSlim _semaphore = new(1, 1);

  public async Task<TResult> GetValueAsync<TResult, TParam>(Func<TParam, CancellationToken, Task<TResult>> func, TParam param, int timeoutInSeconds, CancellationToken token = default)
  {
    var granted = false;
    try
    {
      granted = await _semaphore.WaitAsync(TimeSpan.FromSeconds(timeoutInSeconds), token).ConfigureAwait(false);
      EnsureIsGranted(ref granted);

      return await func(param, token).ConfigureAwait(false);
    }
    finally
    {
      ReleaseIfGranted(ref granted);
    }
  }

  private static void EnsureIsGranted(ref bool granted)
  {
    if (!granted)
    {
      throw new TimeoutException("Timeout during obtaining lock.");
    }
  }

  private void ReleaseIfGranted(ref bool granted)
  {
    if (granted)
    {
      _semaphore.Release();
    }
  }

  public void Dispose() =>
    _semaphore.Dispose();
}