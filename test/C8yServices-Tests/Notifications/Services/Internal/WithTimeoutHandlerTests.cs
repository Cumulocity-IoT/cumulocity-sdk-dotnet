using Microsoft.Extensions.Time.Testing;

namespace C8yServices.Notifications.Services.Internal;
public class WithTimeoutHandlerTests
{
  private readonly FakeTimeProvider _timeProvider = new();

  [Fact]
  public async Task HandleOneCallWithTimeoutNoTimeout()
  {
    const int durationInSeconds = 1;
    const int timeoutInSeconds = 10;
    const int externalTimeoutInSeconds = 10;
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(externalTimeoutInSeconds), _timeProvider);
    const string value = "value";
    var task = WithTimeoutHandler.HandleOneCallWithTimeout(false, TimeSpan.FromSeconds(timeoutInSeconds), _timeProvider, async (_, token) =>
      {
        await Task.Delay(TimeSpan.FromSeconds(durationInSeconds), _timeProvider, token);

        return value;
      },
      (_, _) => "timeout", cts.Token);
    _timeProvider.Advance(TimeSpan.FromSeconds(2));
    var result = await task;
    Assert.Equal(value, result);
  }

  [Fact]
  public async Task HandleOneCallWithTimeoutTimeout()
  {
    const int durationInSeconds = 5;
    const int timeoutInSeconds = 1;
    const int externalTimeoutInSeconds = 10;
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(externalTimeoutInSeconds), _timeProvider);
    const string value = "value";
    const string timeout = "timeout";
    var task = WithTimeoutHandler.HandleOneCallWithTimeout(false, TimeSpan.FromSeconds(timeoutInSeconds), _timeProvider, async (_, token) =>
      {
        await Task.Delay(TimeSpan.FromSeconds(durationInSeconds), _timeProvider, token);

        return value;
      },
      (_, _) => timeout, cts.Token);
    _timeProvider.Advance(TimeSpan.FromSeconds(2));
    var result = await task;
    Assert.Equal(timeout, result);
  }

  [Fact]
  public Task HandleOneCallWithTimeoutExternalCancellation() =>
    Assert.ThrowsAsync<TaskCanceledException>(async () =>
    {
      const int durationInSeconds = 5;
      const int timeoutInSeconds = 10;
      const int externalTimeoutInSeconds = 1;
      const string value = "value";
      const string timeout = "timeout";
      using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(externalTimeoutInSeconds), _timeProvider);
      var task = WithTimeoutHandler.HandleOneCallWithTimeout(false, TimeSpan.FromSeconds(timeoutInSeconds), _timeProvider, async (_, token) =>
        {
          await Task.Delay(TimeSpan.FromSeconds(durationInSeconds), _timeProvider, token);

          return value;
        },
        (_, _) => timeout, cts.Token);
      _timeProvider.Advance(TimeSpan.FromSeconds(2));
      await task;
    });
}