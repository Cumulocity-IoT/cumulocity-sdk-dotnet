using Microsoft.Extensions.Logging;

using Moq;

namespace C8yServices.TestUtils;

public static class LoggerMockExtensions
{
  /// <summary>
  /// verifies if logging at given level with given message and optional exception was executed given times on given mock
  /// </summary>
  public static void VerifyWithException<T>(this Mock<T> loggerMock, LogLevel logLevel, string message, Times times, Exception? exception = null) where T : class, ILogger =>
    loggerMock.Verify(x => x.Log(logLevel, It.IsAny<EventId>(), It.Is<It.IsAnyType>((any, _) => any.ToString() == message), It.Is<Exception?>(e => AreExceptionsTheSame(e, exception)), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);


  private static bool AreExceptionsTheSame(Exception? exception, Exception? expectedException) =>
    exception switch
    {
      null when expectedException is null => true,
      _ => exception is not null && expectedException is not null && exception == expectedException
    };
}