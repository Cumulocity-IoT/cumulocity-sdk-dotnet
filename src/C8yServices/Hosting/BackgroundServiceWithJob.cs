using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace C8yServices.Hosting;

/// <summary>
/// Background service with separated background service job injected as a strategy
/// </summary>
/// <seealso cref="BackgroundService" />
public sealed class BackgroundServiceWithJob<T> : BackgroundService where T : IBackgroundServiceJob
{
  private readonly ILogger<BackgroundServiceWithJob<T>> _logger;
  public T Job { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="BackgroundServiceWithJob{T}"/> class.
  /// </summary>
  /// <param name="logger">The logger.</param>
  /// <param name="job">The job.</param>
  public BackgroundServiceWithJob(ILogger<BackgroundServiceWithJob<T>> logger, T job)
  {
    _logger = logger;
    Job = job;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      var beforeLoop = await Job.ExecuteBeforeLoop(stoppingToken).ConfigureAwait(false);
      if (beforeLoop == ResultType.Exit)
      {
        return;
      }
      while (!stoppingToken.IsCancellationRequested)
      {
        var inLoop = await HandleInLoop(stoppingToken).ConfigureAwait(false);
        if (inLoop == ResultType.Exit)
        {
          break;
        }
      }
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Generic exception before loop.");
    }
  }

  private async Task<ResultType> HandleInLoop(CancellationToken stoppingToken)
  {
    try
    {
      return await Job.ExecuteInLoop(stoppingToken).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      _logger.LogError(exception, "Generic exception in loop.");

      return ResultType.Continue;
    }
  }
}