
using System;

namespace IotLib.AgentServices.Hosting;

public interface IAdjustable
{
  TimeSpan Interval { get; }
  void ChangeInterval(TimeSpan interval);
}