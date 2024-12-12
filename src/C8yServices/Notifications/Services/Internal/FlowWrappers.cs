using System.Net;

using C8yServices.Common.Models;
using C8yServices.Extensions.Http;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal static class FlowWrappers
{
  public static async Task HandleInLoopWithException<T, TArgument>(T param, Func<TArgument?> argumentFunc, TimeSpan minimalDelay, Action<Exception, T> outerLoggerAction, Action<Exception, T> innerLoggerAction,
    Func<T, TArgument, CancellationToken, Task> jobTask, TimeProvider timeProvider, CancellationToken cancellationToken = default)
  {
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          var argument = argumentFunc();
          if (argument is null)
          {
            await Task.Delay(minimalDelay, timeProvider, cancellationToken).ConfigureAwait(false);

            continue;
          }
          await jobTask(param, argument, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
#pragma warning disable S108
        {
        }
#pragma warning restore S108
        catch (Exception e)
        {
          innerLoggerAction(e, param);
        }
      }
    }
    catch (Exception e)
    {
      outerLoggerAction(e, param);
    }
  }

  public static async Task<OneOf<TResult, ApiError>> HandleOneCallWithHttpRequestException<TResult, TParam>(TParam param, Func<TParam, CancellationToken, Task<OneOf<TResult, ApiError>>> func, CancellationToken token = default)
  {
    try
    {
      return await func(param, token).ConfigureAwait(false);
    }
    catch (HttpRequestException e)
    {
      return e.ToApiError();
    }
  }

  public static async Task<OneOf<Success, NotFound, ApiError>> HandleOneCallWithHttpRequestException<TParam>(TParam param, Func<TParam, CancellationToken, Task<OneOf<Success, NotFound, ApiError>>> func, CancellationToken token = default)
  {
    try
    {
      return await func(param, token).ConfigureAwait(false);
    }
    catch (HttpRequestException e) when (e.GetHttpStatusCode() == HttpStatusCode.NotFound)
    {
      return new NotFound();
    }
    catch (HttpRequestException e)
    {
      return e.ToApiError();
    }
  }
}