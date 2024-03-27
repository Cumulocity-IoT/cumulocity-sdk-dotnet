
using System;

namespace IotLib.AgentServices.Hosting;

public sealed class ExceptionRetryData
{
  private readonly byte _maxExceptionBackOffMultiplier;
  private readonly TimeSpan _interval;

  public ExceptionRetryData(byte maxExceptionBackOffMultiplier, TimeSpan interval)
  {
    _maxExceptionBackOffMultiplier = maxExceptionBackOffMultiplier;
    _interval = interval;
  }

  public void ResetTryCount()
  {
    TryCount = 0;
  }

  public void IncreaseTryCount()
  {
    TryCount += 1;
  }

  public int TryCount { get; private set; }

  public TimeSpan GetInterval()
  {
    return TryCount > _maxExceptionBackOffMultiplier ? _interval * _maxExceptionBackOffMultiplier : _interval * TryCount;
  }
}