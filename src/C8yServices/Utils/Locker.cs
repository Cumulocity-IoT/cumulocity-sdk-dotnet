
using System;
using System.Threading;
using System.Threading.Tasks;

namespace C8yServices.Utils;

public sealed class Locker : IDisposable
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

  public async Task ExecuteAsync<TParam>(Func<TParam, CancellationToken, Task> func, TParam param, int timeoutInSeconds, CancellationToken token = default)
  {
    var granted = false;
    try
    {
      granted = await _semaphore.WaitAsync(TimeSpan.FromSeconds(timeoutInSeconds), token).ConfigureAwait(false);
      EnsureIsGranted(ref granted);

      await func(param, token).ConfigureAwait(false);
    }
    finally
    {
      ReleaseIfGranted(ref granted);
    }
  }

  public TResult GetValue<TResult, TParam>(Func<TParam, TResult> func, TParam param, int timeoutInSeconds)
  {
    var granted = false;
    try
    {
      granted = _semaphore.Wait(TimeSpan.FromSeconds(timeoutInSeconds));
      EnsureIsGranted(ref granted);

      return func(param);
    }
    finally
    {
      ReleaseIfGranted(ref granted);
    }
  }

  public void Execute<TParam>(Action<TParam> action, TParam param, int timeoutInSeconds)
  {
    var granted = false;
    try
    {
      granted = _semaphore.Wait(TimeSpan.FromSeconds(timeoutInSeconds));
      EnsureIsGranted(ref granted);

      action(param);
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

  public void Dispose()
  {
    _semaphore.Dispose();
  }
}