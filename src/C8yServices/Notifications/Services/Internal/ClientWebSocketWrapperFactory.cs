using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using C8yServices.Notifications.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class ClientWebSocketWrapperFactory : IClientWebSocketWrapperFactory
{
  private readonly TimeProvider _timeProvider;
  private readonly ITokenProvider _tokenProvider;
  private readonly TimeSpan _monitorDelay;

  public ClientWebSocketWrapperFactory(IOptions<NotificationServiceConfiguration> options, TimeProvider timeProvider, ITokenProvider tokenProvider)
  {
    _timeProvider = timeProvider;
    _tokenProvider = tokenProvider;
    _monitorDelay = options.Value.WebSocketClientMonitorInterval;
  }

  public IClientWebSocketWrapper GetNewInstance<TParam>(ILogger logger, string tenantId, Uri uri, TParam param, Func<ReadOnlyMemory<byte>, TParam, CancellationToken, Task> dataHandler,
    Func<WebSocketState, TParam, CancellationToken, Task> monitorHandler) => new ClientWebSocketWrapper<TParam>(uri, tenantId, logger, _monitorDelay, dataHandler, monitorHandler, param, _timeProvider, _tokenProvider);
}