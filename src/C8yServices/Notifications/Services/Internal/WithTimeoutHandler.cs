namespace C8yServices.Notifications.Services.Internal;

internal static class WithTimeoutHandler
{
  public static async Task<TResult> HandleOneCallWithTimeout<TResult, TParam>(TParam param, TimeSpan timeOut, TimeProvider timeProvider, Func<TParam, CancellationToken, Task<TResult>> getOkResult, Func<TParam, TimeSpan, TResult> getTimeoutResult, CancellationToken cancellationToken)
  {
    using var cancellationTokenSource = CreateCancellationTokenSource(timeOut, timeProvider, cancellationToken);
    try
    {
      return await getOkResult(param, cancellationTokenSource.Token).ConfigureAwait(false);
    }
    catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
    {
      return getTimeoutResult(param, timeOut);
    }
  }

  private static CancellationTokenSource CreateCancellationTokenSource(TimeSpan timeout, TimeProvider timeProvider, CancellationToken token) =>
    CancellationTokenSource.CreateLinkedTokenSource(new CancellationTokenSource(timeout, timeProvider).Token, token);
}