using Microsoft.Extensions.Logging;

namespace C8yServices.Hosting;

public sealed class BackgroundTask : IDisposable, IAdjustable
{
  private readonly byte? _maxExceptionBackOffMultiplier;
  private readonly AdjustablePeriodicTimer _timer;
  private TaskData? _taskData;
  private readonly BackgroundTaskHelper _helper;
  private readonly ILogger _logger;

  public BackgroundTask(BackgroundTaskOptions backgroundTaskOptions, ILogger logger)
  {
    _maxExceptionBackOffMultiplier = backgroundTaskOptions.MaxExceptionBackOffMultiplier;
    _timer = new AdjustablePeriodicTimer(backgroundTaskOptions.Interval);
    _helper = new BackgroundTaskHelper(backgroundTaskOptions.WorkAction, backgroundTaskOptions.BackgroundTaskMode,
      backgroundTaskOptions.Interval, logger, backgroundTaskOptions.OnException);
    _logger = logger;
  }

  public BackgroundTask(BackgroundTaskOptions backgroundTaskOptions, ILoggerFactory loggerFactory)
    : this(backgroundTaskOptions, loggerFactory.CreateLogger(typeof(BackgroundTask)))
  {
  }

  public TimeSpan Interval => _timer.Period;

  public void Start(bool startWorkImmediately = false)
  {
    if (_taskData is not null)
    {
      return;
    }
    var exceptionRetryData = _maxExceptionBackOffMultiplier is not null ? new ExceptionRetryData(_maxExceptionBackOffMultiplier.Value, Interval) : null;
    _taskData = new TaskData(source => DoWork(source, exceptionRetryData, startWorkImmediately));
  }

  private async Task DoWork(CancellationTokenSource cancellationTokenSource, ExceptionRetryData? exceptionRetryData, bool startWorkImmediately)
  {
    try
    {
      if (startWorkImmediately)
      {
        await _helper.WorkHandler(cancellationTokenSource, this, exceptionRetryData).ConfigureAwait(false);
      }
      while (await _timer.WaitForNextTickAsync(cancellationTokenSource.Token).ConfigureAwait(false))
      {
        await _helper.WorkHandler(cancellationTokenSource, this, exceptionRetryData).ConfigureAwait(false);
      }
    }
    catch (OperationCanceledException e)
    {
      _logger.LogDebug(e, "OperationCanceledException was thrown.");
    }
  }

  public async Task Stop()
  {
    if (_taskData is null)
    {
      return;
    }
    await _taskData.CancellationTokenSource.CancelAsync();
    await _taskData.TimerTask.ConfigureAwait(false);
    _taskData.CancellationTokenSource.Dispose();
    _taskData = null;
  }

  public bool IsRunning() => _taskData is not null;

  private sealed class TaskData
  {
    public TaskData(Func<CancellationTokenSource, Task> timerTask)
    {
      CancellationTokenSource = new CancellationTokenSource();
      TimerTask = timerTask(CancellationTokenSource);
    }

    public Task TimerTask { get; }

    public CancellationTokenSource CancellationTokenSource { get; }
  }

  public void Dispose() => _timer.Dispose();
  public void ChangeInterval(TimeSpan interval) => _timer.ChangePeriod(interval);
}