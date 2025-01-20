using System.Diagnostics.CodeAnalysis;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class TaskData<TParam> : IAsyncDisposable
{
  private readonly CancellationTokenSource _cancellationTokenSource;
  private readonly Task _task;

  public TaskData(TParam param, Func<TParam, CancellationTokenSource, Task> task)
  {
    _cancellationTokenSource = new CancellationTokenSource();
    _task = task(param, _cancellationTokenSource);
  }

  private async Task Stop()
  {
    await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
    await _task.ConfigureAwait(false);
    _cancellationTokenSource.Dispose();
  }

  public async ValueTask DisposeAsync() =>
      await Stop().ConfigureAwait(false);
}