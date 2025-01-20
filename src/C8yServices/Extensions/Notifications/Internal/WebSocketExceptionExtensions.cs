using System.Net.WebSockets;

namespace C8yServices.Extensions.Notifications.Internal;

internal static class WebSocketExceptionExtensions
{
  public static bool IsConnectionError(this WebSocketException exception) =>
      exception.WebSocketErrorCode == WebSocketError.Faulted;
}