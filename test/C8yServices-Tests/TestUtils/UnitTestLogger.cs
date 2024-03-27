using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace C8yServices.TestUtils;

public sealed class UnitTestLogger<T> : ILogger<T>, IDisposable
{
  private readonly ITestOutputHelper _output;

  public UnitTestLogger(ITestOutputHelper output)
  {
    _output = output;
  }
  public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
  {
    _output.WriteLine($"{logLevel}: {state?.ToString()}");
  }

  public bool IsEnabled(LogLevel logLevel)
  {
    return true;
  }

  public IDisposable BeginScope<TState>(TState state) where TState : notnull
  {
    return this;
  }

  public void Dispose()
  {
    // Method intentionally left empty.
  }
}