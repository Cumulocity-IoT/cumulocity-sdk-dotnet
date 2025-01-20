using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using C8yServices.Extensions.Notifications.Internal;
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class RealTimeWebSocketClient : IRealTimeWebSocketClient
{
  private readonly TimeProvider _timeProvider;
  private readonly TimeSpan _operationTimeout;
  private readonly RealTimeWebSocketClientHelper<RealTimeWebSocketClient> _helper;

  public RealTimeWebSocketClient(ILogger logger, IClientWebSocketWrapperFactory clientWebSocketWrapperFactory, IDataFeedHandler dataFeedHandler, NotificationServiceConfiguration realTimeConfiguration,
    IMessageExtractor messageExtractor, TimeProvider timeProvider)
  {
    _timeProvider = timeProvider;
    _operationTimeout = realTimeConfiguration.OperationTimeout;
    _helper = new RealTimeWebSocketClientHelper<RealTimeWebSocketClient>(new Uri($"{realTimeConfiguration.BaseUrl}{realTimeConfiguration.NotificationEndpoint}"),
      logger, clientWebSocketWrapperFactory, dataFeedHandler, static (p, token) => p.ReConnect(token), messageExtractor, this);
  }

  public Task<Error?> Connect(string tenantId, TokenClaimWithToken tokenClaimWithToken, CancellationToken cancellationToken = default) =>
    WithTimeoutHandler.HandleOneCallWithTimeout((This: this, tokenClaimWithToken), _operationTimeout, _timeProvider, (p, cancellationToken) => p.This._helper.Connect(tenantId, p.tokenClaimWithToken, cancellationToken), static (_, _) => GetTimeoutError(), cancellationToken);

  public Task Disconnect(CancellationToken cancellationToken = default) =>
    _helper.ClientWebSocketWrapperIsNullClientWebSocketWrapper() ? Task.CompletedTask : _helper.Disconnect(cancellationToken);

  public WebSocketState? State => 
    _helper.ClientWebSocketWrapper.State;

  public string? Token =>
    _helper.ClientWebSocketWrapper.Token;

  private Task<Error?> ReConnect(CancellationToken cancellationToken = default) =>
    WithTimeoutHandler.HandleOneCallWithTimeout(this, _operationTimeout, _timeProvider, static (p, sources) => p._helper.ReConnect(sources), static (_, _) => GetTimeoutError(), cancellationToken);

  private static Error GetTimeoutError()
    => "Operation timed out.".GetError(true);

  public async ValueTask DisposeAsync()
  {
    if (!_helper.ClientWebSocketWrapperIsNullClientWebSocketWrapper())
    {
      await _helper.ClientWebSocketWrapper.DisposeAsync().ConfigureAwait(false);
    }
    _helper.SetClientWebSocketWrapperToNullObject();
  }
}