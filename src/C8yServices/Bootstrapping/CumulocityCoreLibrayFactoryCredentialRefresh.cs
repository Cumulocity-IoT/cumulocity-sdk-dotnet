using C8yServices.Hosting;

using Microsoft.Extensions.Logging;

namespace C8yServices.Bootstrapping;

public sealed class CumulocityCoreLibrayFactoryCredentialRefresh : IDisposable
{
  private readonly BackgroundTask _backgroundTask;
  private readonly ICumulocityCoreLibrayFactory _cumulocityApiFactory;
  private readonly ILogger _logger;
  private const int IntervalInMinutes = 1;

  public CumulocityCoreLibrayFactoryCredentialRefresh(ILoggerFactory loggerFactory, ICumulocityCoreLibrayFactory cumulocityApiFactory)
  {
    _cumulocityApiFactory = cumulocityApiFactory;
    _logger = loggerFactory.CreateLogger(GetType());
    _backgroundTask = new BackgroundTask(new BackgroundTaskOptions(TimeSpan.FromMinutes(IntervalInMinutes), (token, _) => DoWork(token), BackgroundTaskMode.TryFixedTriggerTime), loggerFactory);
  }

  public void Start()
  {
    _logger.LogInformation("Starting api credential refresh background task.");
    _backgroundTask.Start();
  }

  public Task Stop() => _backgroundTask.Stop();

  private Task DoWork(CancellationToken token = default)
  {
    _logger.LogDebug("Refreshing api credentials.");

    return _cumulocityApiFactory.InitOrRefresh(token);
  }

  public void Dispose() =>
    _backgroundTask.Dispose();
}