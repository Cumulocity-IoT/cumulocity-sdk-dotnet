using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Extensions.Notifications.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal static class ClientWebSocketExtensions
{
  public static ValueTask Send(this ClientWebSocket clientWebSocket, ReadOnlyMemory<byte> utf8Bytes, CancellationToken cancellationToken = default) =>
    clientWebSocket.SendAsync(utf8Bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);

  public static async Task<ReceiveResult> Receive(this ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
  {
    var buffer = new byte[1024];
    var list = new List<byte>();
    while (true)
    {
      var result = await clientWebSocket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
      if (result.MessageType == WebSocketMessageType.Close)
      {
        list.AddRange(buffer.Take(result.Count));

        return new(true, list.ToArray());
      }
      if (result.EndOfMessage)
      {
        list.AddRange(buffer.Take(result.Count));

        return new(false, list.ToArray());
      }

      list.AddRange(buffer.Take(result.Count));
    }
  }

  public static Task Close(this ClientWebSocket clientWebSocket, CancellationToken cancellationToken)
      => clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);

  public static async Task<OneOf.Types.Error<string>?> Connect(this ClientWebSocket clientWebSocket, Uri uri, CancellationToken cancellationToken)
  {
    try
    {
      await clientWebSocket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

      return null;
    }
    catch (WebSocketException e)
    {
      if (e.IsConnectionError())
      {
        return new OneOf.Types.Error<string>($"Failed to connect. {e.Message}");
      }
      throw;
    }
  }
}