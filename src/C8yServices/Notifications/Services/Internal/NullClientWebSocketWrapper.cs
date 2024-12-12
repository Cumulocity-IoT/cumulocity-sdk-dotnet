using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class NullClientWebSocketWrapper : IClientWebSocketWrapper
{
  public ValueTask DisposeAsync() => 
    ValueTask.CompletedTask;

  public Task<Error?> ReConnect(CancellationToken cancellationToken = default) => 
    Task.FromResult<Error?>(null);

  public Task<Error?> Connect(TokenClaimWithToken tokenClaimWithToken, CancellationToken cancellationToken = default) => 
    Task.FromResult<Error?>(null);

  public Task Close(CancellationToken cancellationToken = default) => 
    Task.CompletedTask;

  public ValueTask Send(ReadOnlyMemory<byte> utf8Bytes, CancellationToken cancellationToken = default) => 
    ValueTask.CompletedTask;

  public WebSocketState? State => 
    null;

  public string? Token =>
    null;

  private NullClientWebSocketWrapper()
  {
  }

  public static readonly NullClientWebSocketWrapper Instance = new();
}