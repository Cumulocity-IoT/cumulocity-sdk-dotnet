namespace C8yServices.Extensions.DateTime;

public static class DateTimeOffsetExtensions
{
  public static bool WithinTimespan(this DateTimeOffset lastFired, int maxNotificationTimespanSeconds, DateTimeOffset now) =>
    now.Subtract(lastFired).TotalSeconds <= maxNotificationTimespanSeconds;
}