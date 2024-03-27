
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace IotLib.AgentServices.Hosting;

public sealed class BackgroundTaskHelper
{
  private readonly Func<CancellationToken, IAdjustable, Task> _workAction;
  private readonly BackgroundTaskMode _backgroundTaskMode;
  private readonly TimeSpan _interval;
  private readonly ILogger _logger;
  private readonly Func<CancellationToken, Exception, Task>? _onException;

  public BackgroundTaskHelper(Func<CancellationToken, IAdjustable, Task> workAction,
    BackgroundTaskMode backgroundTaskMode, TimeSpan interval, ILogger logger,
    Func<CancellationToken, Exception, Task>? onException)
  {
    _workAction = workAction;
    _backgroundTaskMode = backgroundTaskMode;
    _interval = interval;
    _logger = logger;
    _onException = onException;
  }

  public async Task WorkHandler(CancellationTokenSource cancellationTokenSource, IAdjustable adjustable, ExceptionRetryData? exceptionRetryData)
  {
    try
    {
      await _workAction(cancellationTokenSource.Token, adjustable).ConfigureAwait(false);
      exceptionRetryData?.ResetTryCount();
      if (_backgroundTaskMode == BackgroundTaskMode.EvenBreaksBetweenTriggers)
      {
        await Task.Delay(_interval, cancellationTokenSource.Token).ConfigureAwait(false);
      }
    }
    catch (OperationCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      if (exceptionRetryData is not null)
      {
        exceptionRetryData.IncreaseTryCount();
        await Task.Delay(exceptionRetryData.GetInterval(), cancellationTokenSource.Token).ConfigureAwait(false);
      }

      await HandleOnException(ex, _logger, _onException, cancellationTokenSource.Token).ConfigureAwait(false);
    }
  }

  private static async Task HandleOnException(Exception exception, ILogger logger, Func<CancellationToken, Exception, Task>? onException, CancellationToken token)
  {
    if (onException is null)
    {
      logger.LogError(exception, "Unhandled exception");
      return;
    }
    try
    {
      await onException(token, exception).ConfigureAwait(false);
    }
    catch (OperationCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Exception during handling exception. Original exception: {Exception}", exception);
    }
  }
}