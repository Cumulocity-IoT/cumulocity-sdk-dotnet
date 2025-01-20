using System.Net;
using System.Net.Sockets;

using C8yServices.Common.Models;

using Client.Com.Cumulocity.Client.Supplementary;

namespace C8yServices.Extensions.Http;

public static class HttpRequestExceptionExtensions
{
  public static bool ShouldHandleTransientHttpRequestException(this HttpRequestException httpRequestException)
  {
    var statusCode = httpRequestException.GetHttpStatusCode();

    return statusCode is not null && statusCode.Value.IsTransientHttpStatusCode()
           || httpRequestException.ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused();
  }

  public static bool ShouldHandleHttpRequestExceptionSocketErrorConnectionRefused(this HttpRequestException httpRequestException) =>
    httpRequestException.GetHttpStatusCode() is null && httpRequestException.InnerException is SocketException
    {
      SocketErrorCode: SocketError.ConnectionRefused
    };

  public static HttpStatusCode? GetHttpStatusCode(this HttpRequestException httpRequestException) =>
    httpRequestException.StatusCode ?? httpRequestException.GetStatusCode();

  public static ApiError ToApiError(this HttpRequestException exception) =>
    new(exception.Message, exception.GetHttpStatusCode());
}