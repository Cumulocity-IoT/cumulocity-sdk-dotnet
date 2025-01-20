using Microsoft.Extensions.Logging;

using Moq;

namespace C8yServices.TestUtils;

public static class LoggerMockExtensions
{
  public static void VerifyWithException<TException>(this Mock<ILogger> loggerMock, LogLevel logLevel, string startMessage, Times times) 
    where TException : Exception =>
    loggerMock.Verify(logger => logger.Log(logLevel, It.IsAny<EventId>(), It.Is<It.IsAnyType>((any, _) => StartWith(any, startMessage)), It.Is<Exception?>(e => IsExpectedExceptionType<TException>(e)), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);

  private static bool StartWith<T>(T obj, string startMessage)
  {
    var value = obj?.ToString();

    return value is not null && value.StartsWith(startMessage, StringComparison.InvariantCulture);
  }

  private static bool IsExpectedExceptionType<TException>(Exception? exception)
    where TException : Exception =>
    exception switch
    {
      null => false,
      _ => exception is TException
    };
}