using System.Net.WebSockets;

using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Notifications.Services.Internal;

internal interface IRealTimeWebSocketClient : IAsyncDisposable
{
  Task<Error?> Connect(string tenantId, TokenClaimWithToken tokenClaimWithToken, CancellationToken cancellationToken = default);

  Task Disconnect(CancellationToken cancellationToken = default);

  WebSocketState? State { get; }

  string? Token { get; }
}