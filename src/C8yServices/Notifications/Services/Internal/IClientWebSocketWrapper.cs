using System.Net.WebSockets;

using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Notifications.Services.Internal;

internal interface IClientWebSocketWrapper : IAsyncDisposable
{
  Task<Error?> ReConnect(CancellationToken cancellationToken = default);
  Task<Error?> Connect(TokenClaimWithToken tokenClaimWithToken, CancellationToken cancellationToken = default);
  Task Close(CancellationToken cancellationToken = default);
  ValueTask Send(ReadOnlyMemory<byte> utf8Bytes, CancellationToken cancellationToken = default);
  WebSocketState? State { get; }
  string? Token { get; }
}