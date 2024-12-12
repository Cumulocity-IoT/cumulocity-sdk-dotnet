using System.Net.WebSockets;
using System.Text;

using C8yServices.Extensions.Notifications.Internal;
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class RealTimeWebSocketClientHelper<TParam>
{
  private readonly Uri _uri;
  private readonly ILogger _logger;
  private readonly IClientWebSocketWrapperFactory _clientWebSocketWrapperFactory;
  private readonly IDataFeedHandler _dataFeedHandler;
  private readonly Func<TParam, CancellationToken, Task<Error?>> _reconnectFunc;
  private readonly IMessageExtractor _messageExtractor;
  private readonly TParam _param;

  public IClientWebSocketWrapper ClientWebSocketWrapper { get; private set; }

  public RealTimeWebSocketClientHelper(Uri uri, ILogger logger,
    IClientWebSocketWrapperFactory clientWebSocketWrapperFactory, IDataFeedHandler dataFeedHandler,
    Func<TParam, CancellationToken, Task<Error?>> reconnectFunc, IMessageExtractor messageExtractor, TParam param)
  {
    _uri = uri;
    _logger = logger;
    _clientWebSocketWrapperFactory = clientWebSocketWrapperFactory;
    _dataFeedHandler = dataFeedHandler;
    _reconnectFunc = reconnectFunc;
    _messageExtractor = messageExtractor;
    _param = param;
    ClientWebSocketWrapper = NullClientWebSocketWrapper.Instance;
  }

  public async Task<Error?> Connect(string tenantId, TokenClaimWithToken tokenClaimWithToken, CancellationToken cancellationToken)
  {
    var uri = new Uri($"{_uri}/consumer/");
    var clientWebSocket = _clientWebSocketWrapperFactory.GetNewInstance(_logger, tenantId, uri, this, static (bytes, p, token) => p.DataHandler(bytes, token),
      static (state, p, cancellationToken) => p.MonitorHandler(state, cancellationToken));
    var connectResult = await clientWebSocket.Connect(tokenClaimWithToken, cancellationToken).ConfigureAwait(false);
    if (connectResult is not null)
    {
      return connectResult;
    }
    ClientWebSocketWrapper = clientWebSocket;

    return null;
  }

  public async Task<Error?> ReConnect(CancellationToken cancellationToken)
  {
    if (ClientWebSocketWrapperIsNullClientWebSocketWrapper())
    {
      throw new InvalidOperationException("Not connected.");
    }

    return ClientWebSocketWrapper.State == WebSocketState.Open
      ? null
      : await ClientWebSocketWrapper.ReConnect(cancellationToken).ConfigureAwait(false);
  }

  public async Task Disconnect(CancellationToken cancellationToken)
  {
    await ClientWebSocketWrapper.Close(cancellationToken).ConfigureAwait(false);
    await ClientWebSocketWrapper.DisposeAsync().ConfigureAwait(false);
    SetClientWebSocketWrapperToNullObject();
  }

  public async Task MonitorHandler(WebSocketState state, CancellationToken cancellationToken)
  {
    _logger.LogDebugWebSocketState(state);
    if (state is WebSocketState.Aborted or WebSocketState.Closed)
    {
      var result = await _reconnectFunc(_param, cancellationToken).ConfigureAwait(false);
      if (result is not null)
      {
        _logger.LogDebugErrorDuringPhase("reconnect", result.Message, result.Transient);
      }
    }
  }

  public async Task DataHandler(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
  {
    var dataAsString = Encoding.UTF8.GetString(bytes.Span);
    var messageData = _messageExtractor.GetMessageData(dataAsString);
    await _dataFeedHandler.Handle(new MessageObject(messageData.RawMessage, new Api(messageData.ApiUrl), new Models.Action(messageData.Action)), cancellationToken).ConfigureAwait(false);
    await SendAck(messageData.Acknowledgement, cancellationToken).ConfigureAwait(false);
  }

  public bool ClientWebSocketWrapperIsNullClientWebSocketWrapper() =>
    ClientWebSocketWrapper is NullClientWebSocketWrapper;

  public void SetClientWebSocketWrapperToNullObject() =>
    ClientWebSocketWrapper = NullClientWebSocketWrapper.Instance;

  private async Task SendAck(string acknowledgement, CancellationToken token = default) =>
    await ClientWebSocketWrapper.Send(Encoding.UTF8.GetBytes(acknowledgement), token).ConfigureAwait(false);
}