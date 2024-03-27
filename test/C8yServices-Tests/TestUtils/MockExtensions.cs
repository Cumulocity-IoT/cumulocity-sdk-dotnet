using Moq;

using Xunit.Abstractions;

namespace C8yServices.TestUtils;

/// <summary>
/// Contains (static) methods to handle 'Moq' mocks like awaiting method invocations...
/// </summary>
public static class MockExtensions
{
  /// <summary>
  /// Waits (blocking) for the given number of invocations of the method with the given name on the mock instance
  /// </summary>
  /// <param name="mock">Mock instance on which the invocations are made</param>
  /// <param name="methodName">Name of the method to be invoked</param>
  /// <param name="invocations">Number of invocations to wait for</param>
  /// <param name="timeoutMs">Timeout in ms</param>
  /// <param name="outputHelper">(Optional) <c>ITestOutputHelper</c> to write an output</param>
  /// <exception>Throws an <c>AwaitMethodInvocationException</c> when waiting timed out or an error occurs</exception>
  public static async Task AwaitMethodInvocation(this Mock? mock, string methodName, uint invocations, uint timeoutMs, ITestOutputHelper? outputHelper = null)
  {
    var mockName = mock?.Object.GetType().Name ?? throw new ArgumentNullException(nameof(mock), "no mock instance given to await method invocations");
    var fullMethodName = $"{mockName}.{(!string.IsNullOrEmpty(methodName) ? methodName : throw new ArgumentNullException(nameof(methodName), "no method name given to await method invocations"))}";
    var startTime = DateTime.UtcNow;

    while (DateTime.UtcNow < startTime.AddMilliseconds(timeoutMs))
    {
      var currentInvocations = (uint)mock.Invocations.Count(invocation => invocation.Method.Name == methodName);
      if (currentInvocations >= invocations)
      {
        outputHelper?.WriteLine($"Waited {(DateTime.UtcNow - startTime).TotalMilliseconds} ms for {invocations} invocation(s) of method '{fullMethodName}'");
        return;
      }

      await Task.Delay(1);
    }

    outputHelper?.WriteLine($"Invocations on mock '{mockName}':\n{string.Join('\n', mock.Invocations)}");
    throw new TimeoutException($"Waiting for {invocations} invocation(s) of method '{fullMethodName}' timed out after {timeoutMs} ms");
  }
}