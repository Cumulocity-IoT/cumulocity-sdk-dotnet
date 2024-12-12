namespace C8yServices.Hosting;

/// <summary>
/// Background service job injected as strategy into background service with job
/// </summary>
public interface IBackgroundServiceJob
{
  /// <summary>
  /// Executes the before loop. If returned result will be Exit then the job will exit.
  /// </summary>
  Task<ResultType> ExecuteBeforeLoop(CancellationToken token = default);

  /// <summary>
  /// Executes the in loop. If returned result will be Exit then the job will exit.
  /// </summary>
  Task<ResultType> ExecuteInLoop(CancellationToken token = default);
}