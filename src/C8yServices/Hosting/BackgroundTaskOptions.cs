namespace C8yServices.Hosting;

public sealed class BackgroundTaskOptions
{
  public TimeSpan Interval { get; }
  public Func<CancellationToken, IAdjustable, Task> WorkAction { get; }
  public BackgroundTaskMode BackgroundTaskMode { get; }
  public byte? MaxExceptionBackOffMultiplier { get; init; }
  public Func<CancellationToken, Exception, Task>? OnException { get; init; }

  public BackgroundTaskOptions(TimeSpan interval, Func<CancellationToken, IAdjustable, Task> workAction, BackgroundTaskMode backgroundTaskMode)
  {
    Interval = interval;
    WorkAction = workAction;
    BackgroundTaskMode = backgroundTaskMode;
  }
}