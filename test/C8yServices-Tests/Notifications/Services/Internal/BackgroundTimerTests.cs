using C8yServices.TestUtils;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

using Moq;

namespace C8yServices.Notifications.Services.Internal;

public class BackgroundTimerTests
{
  private readonly FakeTimeProvider _timeProvider = new();
  private readonly Mock<ILogger> _loggerMock = new();
  private readonly TimeSpan _twoSeconds = TimeSpan.FromSeconds(2);
  private readonly TimeSpan _second = TimeSpan.FromSeconds(1);
  private readonly TimeSpan _twoHundredMilliSeconds = TimeSpan.FromMilliseconds(200);
  private readonly TimeSpan _halfASecond = TimeSpan.FromMilliseconds(500);

  [Fact]
  public async Task TickShouldWork()
  {
    _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    var counter = 0;
    using var timer = new BackgroundTimer<bool>(_second, _timeProvider, false, (_, _) =>
    {
      Interlocked.Increment(ref counter);

      return counter == 2 ? throw new InvalidOperationException() : Task.CompletedTask;
    }, _loggerMock.Object);
    timer.Start();
    _timeProvider.Advance(_second);
    _timeProvider.Advance(_second);
    _timeProvider.Advance(_second);
    _timeProvider.Advance(_halfASecond);
    await timer.Stop();
    Assert.Equal(3, counter);
    _loggerMock.VerifyWithException<InvalidOperationException>(LogLevel.Error, "Generic exception has been thrown in DoWork.", Times.Once());
  }

  [Fact]
  public async Task StopTaskCompleted()
  {
    _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    var startCounter = 0;
    var finishCounter = 0;
    using var timer = new BackgroundTimer<bool>(_second, _timeProvider, false, async (_, token) =>
    {
      Interlocked.Increment(ref startCounter);
      await Task.Delay(_twoHundredMilliSeconds, _timeProvider, token);
      Interlocked.Increment(ref finishCounter);
    }, _loggerMock.Object);
    timer.Start();
    _timeProvider.Advance(_second);
    _timeProvider.Advance(_halfASecond);
    await timer.Stop();
    Assert.Equal(1, startCounter);
    Assert.Equal(1, finishCounter);
    _loggerMock.VerifyWithException<OperationCanceledException>(LogLevel.Error, "Generic exception has been thrown in DoWork.", Times.Once());
  }

  [Fact]
  public async Task StopTaskInProgress()
  {
    _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    var startCounter = 0;
    var finishCounter = 0;
    using var timer = new BackgroundTimer<bool>(_second, _timeProvider, false, async (_, token) =>
    {
      Interlocked.Increment(ref startCounter);
      await Task.Delay(_twoSeconds, _timeProvider, token);
      Interlocked.Increment(ref finishCounter);
    }, _loggerMock.Object);
    timer.Start();
    _timeProvider.Advance(_second);
    _timeProvider.Advance(_second);
    await timer.Stop();
    Assert.Equal(1, startCounter);
    Assert.Equal(0, finishCounter);
    _loggerMock.VerifyWithException<OperationCanceledException>(LogLevel.Error, "Generic exception has been thrown in DoWork.", Times.Once());
  }
}