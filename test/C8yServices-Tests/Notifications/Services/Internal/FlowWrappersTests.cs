using System.Net;

using C8yServices.Common.Models;

using Microsoft.Extensions.Time.Testing;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

public class FlowWrappersTests
{
  [Fact]
  public async Task HandleOneCallWithHttpRequestExceptionHappyPath()
  {
    var result = await FlowWrappers.HandleOneCallWithHttpRequestException(false, (_, _) => Task.FromResult<OneOf<Success, NotFound, ApiError>>(new Success()), CancellationToken.None);
    Assert.True(result.IsT0);
  }

  [Fact]
  public async Task HandleOneCallWithHttpRequestExceptionHttpRequestExceptionNotFound()
  {
    const string error = "error";
    var result = await FlowWrappers.HandleOneCallWithHttpRequestException(false, (_, _) => throw new HttpRequestException(error, null, HttpStatusCode.NotFound), CancellationToken.None);
    Assert.True(result.IsT1);
  }

  [Fact]
  public async Task HandleOneCallWithHttpRequestExceptionHttpRequestException()
  {
    const string error = "error";
    var result = await FlowWrappers.HandleOneCallWithHttpRequestException(false, (_, _) => throw new HttpRequestException(error), CancellationToken.None);
    Assert.True(result.IsT2);
    Assert.Equal(error, result.AsT2.Message);
  }

  [Fact]
  public Task HandleOneCallWithHttpRequestExceptionOtherException() =>
    Assert.ThrowsAsync<InvalidOperationException>(() => FlowWrappers.HandleOneCallWithHttpRequestException(false, (_, _) => throw new InvalidOperationException(), CancellationToken.None));

  [Fact]
  public async Task HandleOneCallWithHttpRequestExceptionWithResultHappyPath()
  {
    const string value = "value";
    var result = await FlowWrappers.HandleOneCallWithHttpRequestException(false, (_, _) => Task.FromResult<OneOf<string, ApiError>>(value), CancellationToken.None);
    Assert.Equal(value, result.AsT0);
  }

  [Fact]
  public async Task HandleOneCallWithHttpRequestExceptionWithResultHttpRequestException()
  {
    const string error = "error";
    var result = await FlowWrappers.HandleOneCallWithHttpRequestException<string, bool>(false, (_, _) => throw new HttpRequestException(error), CancellationToken.None);
    Assert.Equal(error, result.AsT1.Message);
  }

  [Fact]
  public Task HandleOneCallWithHttpRequestExceptionWithResultOtherException() =>
    Assert.ThrowsAsync<InvalidOperationException>(() => FlowWrappers.HandleOneCallWithHttpRequestException<string, bool>(false, (_, _) => throw new InvalidOperationException(), CancellationToken.None));

  [Fact]
  public async Task HandleInLoopWithException()
  {
    var timeProvider = new FakeTimeProvider();
    var argumentFuncCallCounter = 0;
    var jobFuncCounter = 0;
    var innerExceptionLogged = false;
    var outerExceptionLogged = false;
    var task = FlowWrappers.HandleInLoopWithException<bool, object?>(false, () =>
      {
        if (argumentFuncCallCounter == 0)
        {
          argumentFuncCallCounter++;

          return null;
        }
        argumentFuncCallCounter++;

        return new object();
      }, TimeSpan.FromMilliseconds(10),
      (_, _) => { outerExceptionLogged = true; }, (_, _) =>
      {
        innerExceptionLogged = true;

        throw new InvalidOperationException();
      }, (_, _, _) =>
      {
        if (jobFuncCounter == 0)
        {
          jobFuncCounter++;

          throw new OperationCanceledException();
        }
        if (jobFuncCounter == 1)
        {
          jobFuncCounter++;

          return Task.CompletedTask;
        }
        jobFuncCounter++;

        throw new InvalidOperationException();
      }, timeProvider);
    while (!task.IsCompleted)
    {
      timeProvider.Advance(TimeSpan.FromMilliseconds(5));
    }
    await task;
    Assert.Equal(4, argumentFuncCallCounter);
    Assert.Equal(3, jobFuncCounter);
    Assert.True(innerExceptionLogged);
    Assert.True(outerExceptionLogged);
  }
}