using System.Net.WebSockets;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

internal interface IClientWebSocketWrapperFactory
{
  IClientWebSocketWrapper GetNewInstance<TParam>(ILogger logger, string tenantId, Uri uri, TParam param, Func<ReadOnlyMemory<byte>, TParam, CancellationToken, Task> dataHandler,
      Func<WebSocketState, TParam, CancellationToken, Task> monitorHandler);
}