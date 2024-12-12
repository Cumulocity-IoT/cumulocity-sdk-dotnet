using System.Net;

namespace C8yServices.Extensions.Http;

public static class HttpStatusCodeExtensions
{
  public static bool IsTransientHttpStatusCode(this HttpStatusCode code) =>
      code is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;
}