using System.Diagnostics.CodeAnalysis;

using C8yServices.Extensions.Notifications.Internal;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class BackgroundTimer<TParam> : IDisposable
{
  private readonly PeriodicTimer _timer;
  private TaskData<BackgroundTimer<TParam>>? _taskData;
  private readonly TParam _param;
  private readonly Func<TParam, CancellationToken, Task> _workAction;
  private readonly ILogger _logger;

  public BackgroundTimer(TimeSpan interval, TimeProvider timeProvider, TParam param, Func<TParam, CancellationToken, Task> workAction, ILogger logger)
  {
    _param = param;
    _workAction = workAction;
    _logger = logger;
    _timer = new PeriodicTimer(interval, timeProvider);
  }

  public void Start()
  {
    if (_taskData is not null)
    {
      return;
    }
    _taskData = new TaskData<BackgroundTimer<TParam>>(this, static (p, token) => p.DoWork(token));
  }

  private async Task DoWork(CancellationToken cancellationToken)
  {
    try
    {
      while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
      {
        try
        {
          await _workAction(_param, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
          throw;
        }
        catch (Exception ex)
        {
          _logger.LogErrorMethodGenericException(ex, nameof(DoWork));
        }
      }
    }
    catch (OperationCanceledException ex)
    {
      _logger.LogErrorMethodGenericException(ex, nameof(DoWork));
    }
  }

  public async Task Stop()
  {
    if (_taskData is null)
    {
      return;
    }
    await _taskData.Cancel().ConfigureAwait(false);
    _taskData.Dispose();
    _taskData = null;
  }

  public void Dispose() =>
    _timer.Dispose();

  [ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
  private sealed class TaskData<TP> : IDisposable
  {
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _timerTask;

    public TaskData(TP param, Func<TP, CancellationToken, Task> timerTask)
    {
      _cancellationTokenSource = new CancellationTokenSource();
      _timerTask = timerTask(param, _cancellationTokenSource.Token);
    }

    public async Task Cancel()
    {
      await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
      await _timerTask.ConfigureAwait(false);
    }

    public void Dispose() => 
      _cancellationTokenSource.Dispose();
  }
}