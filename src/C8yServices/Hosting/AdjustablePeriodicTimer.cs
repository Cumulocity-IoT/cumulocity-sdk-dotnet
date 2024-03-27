
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IotLib.AgentServices.Hosting;

public sealed class AdjustablePeriodicTimer : IDisposable
{
  private PeriodicTimer? _timer;
  private TimeSpan _newPeriod;

  public AdjustablePeriodicTimer(TimeSpan period)
  {
    _newPeriod = period;
    Period = period;
    _timer = new PeriodicTimer(period);
  }

  public TimeSpan Period { get; private set; }

  public void ChangePeriod(TimeSpan period) => _newPeriod = period;

  public async ValueTask<bool> WaitForNextTickAsync(CancellationToken token = default)
  {
    var timer = _timer ?? throw new InvalidOperationException("Timer should not be null.");
    var result = await timer.WaitForNextTickAsync(token).ConfigureAwait(false);
    if (Period == _newPeriod)
    {
      return result;
    }
    timer.Dispose();
    Period = _newPeriod;
    _timer = new PeriodicTimer(_newPeriod);

    return result;
  }

  public void Dispose() => _timer?.Dispose();
}