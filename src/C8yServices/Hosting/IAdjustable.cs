namespace C8yServices.Hosting;

public interface IAdjustable
{
  TimeSpan Interval { get; }
  void ChangeInterval(TimeSpan interval);
}