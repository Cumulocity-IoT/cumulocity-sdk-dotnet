using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class BackgroundTimerWithJob : IDisposable
{
  private readonly BackgroundTimer<IJob> _backgroundTimer;

  public BackgroundTimerWithJob(TimeSpan interval, IJob job, ILogger logger, TimeProvider timeProvider) =>
    _backgroundTimer = new BackgroundTimer<IJob>(interval, timeProvider, job, static (p, token) => p.Execute(token), logger);

  public void Start() =>
    _backgroundTimer.Start();

  public Task Stop() =>
    _backgroundTimer.Stop();

  public void Dispose() =>
    _backgroundTimer.Dispose();
}