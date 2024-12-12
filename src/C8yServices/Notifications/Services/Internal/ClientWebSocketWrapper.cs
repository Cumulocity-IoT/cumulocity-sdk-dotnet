using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

using C8yServices.Common.Models;

using C8yServices.Extensions.Notifications.Internal;
using C8yServices.Notifications.Models.Internal;

using Microsoft.Extensions.Logging;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class ClientWebSocketWrapper<TParam> : IClientWebSocketWrapper
{
  private readonly Uri _uri;
  private readonly ILogger _logger;
  private readonly TimeSpan _monitorDelay;
  private readonly Func<ReadOnlyMemory<byte>, TParam, CancellationToken, Task> _dataHandler;
  private readonly Func<WebSocketState, TParam, CancellationToken, Task> _monitorHandler;
  private readonly TParam _param;
  private readonly TimeProvider _timeProvider;
  private readonly ITokenProvider _tokenProvider;
  private readonly TimeSpan _minimalDelay;
  private Argument<ClientWebSocketWrapper<TParam>>? _argument;
  private readonly string _tenantId;

  public ClientWebSocketWrapper(Uri uri, string tenantId, ILogger logger, TimeSpan monitorDelay, Func<ReadOnlyMemory<byte>, TParam, CancellationToken, Task> dataHandler,
    Func<WebSocketState, TParam, CancellationToken, Task> monitorHandler, TParam param, TimeProvider timeProvider, ITokenProvider tokenProvider)
  {
    _uri = uri;
    _tenantId = tenantId;
    _logger = logger;
    _monitorDelay = monitorDelay;
    _dataHandler = dataHandler;
    _monitorHandler = monitorHandler;
    _param = param;
    _timeProvider = timeProvider;
    _tokenProvider = tokenProvider;
    _minimalDelay = TimeSpan.FromMilliseconds(1);
  }

  public async Task<Error?> ReConnect(CancellationToken cancellationToken = default)
  {
    if (_argument is null)
    {
      throw new InvalidOperationException("Connect should be called first.");
    }
    var recreateClientResult = await _argument.RecreateClient(_tenantId, cancellationToken).ConfigureAwait(false);
    if (recreateClientResult is not null)
    {
      return new Error(false, $"Message: {recreateClientResult.Message}, Status code: {recreateClientResult.StatusCode}");
    }
    var connectResult = await _argument.Connect(_uri, cancellationToken).ConfigureAwait(false);

    return connectResult is not null
      ? new Error(true, connectResult.Value.Value)
      : null;
  }

  public async Task<Error?> Connect(TokenClaimWithToken tokenClaimWithToken, CancellationToken cancellationToken = default)
  {
    if (_argument is not null)
    {
      return null;
    }
    _argument = new Argument<ClientWebSocketWrapper<TParam>>(new TaskData<ClientWebSocketWrapper<TParam>>(this, static (wrapper, source) => wrapper.ReceiveHandler(source.Token)),
      new TaskData<ClientWebSocketWrapper<TParam>>(this, static (wrapper, source) => wrapper.MonitorHandler(source.Token)), tokenClaimWithToken, _logger, _tokenProvider);
    var connectResult = await _argument.Connect(_uri, cancellationToken).ConfigureAwait(false);

    return connectResult is not null ? new Error(true, connectResult.Value.Value) : null;
  }

  public async Task Close(CancellationToken cancellationToken = default)
  {
    if (_argument is null)
    {
      return;
    }
    await _argument.Close(cancellationToken).ConfigureAwait(false);
    _argument = null;
  }

  public ValueTask Send(ReadOnlyMemory<byte> utf8Bytes, CancellationToken cancellationToken = default) =>
    _argument?.Send(utf8Bytes, cancellationToken) ?? throw new InvalidOperationException("Client is null.");

  public WebSocketState? State => 
    _argument?.State;

  public string? Token => 
    _argument?.Token;

  public async ValueTask DisposeAsync()
  {
    if (_argument is not null)
    {
      await _argument.Close(CancellationToken.None).ConfigureAwait(false);
    }
    _argument = null;
  }

  private Task HandlerWrapper<T>(T param, Action<Exception, T> outerLoggerAction, Action<Exception, T> innerLoggerAction,
    Func<T, Argument<ClientWebSocketWrapper<TParam>>, CancellationToken, Task> jobTask, CancellationToken cancellationToken = default) =>
    FlowWrappers.HandleInLoopWithException(param, () => _argument, _minimalDelay, outerLoggerAction, innerLoggerAction,
      jobTask, _timeProvider, cancellationToken);

  private Task MonitorHandler(CancellationToken cancellationToken) =>
    HandlerWrapper(this,
      static (exception, p) => p._logger.LogDebugGenericErrorInExternalMonitorHandler(exception),
      static (exception, p) => p._logger.LogDebugGenericErrorInMonitorHandler(exception),
      static (p, argument, token) => p.InternalMonitorHandler(argument, token), cancellationToken);

  private async Task InternalMonitorHandler(Argument<ClientWebSocketWrapper<TParam>> argument, CancellationToken token)
  {
    await Task.Delay(_monitorDelay, _timeProvider, token).ConfigureAwait(false);
    await _monitorHandler(argument.State, _param, token).ConfigureAwait(false);
  }

  private Task ReceiveHandler(CancellationToken cancellationToken) =>
    HandlerWrapper(this,
      static (exception, p) => p._logger.LogDebugGenericErrorInExternalReceiveHandler(exception),
      static (exception, p) => p._logger.LogDebugGenericErrorInReceiveHandler(exception),
      static (p, argument, token) => p.InternalReceiveHandler(argument, token), cancellationToken);

  private async Task InternalReceiveHandler(Argument<ClientWebSocketWrapper<TParam>> argument, CancellationToken token)
  {
    if (argument.State != WebSocketState.Open)
    {
      await Task.Delay(_minimalDelay, _timeProvider, token).ConfigureAwait(false);

      return;
    }
    var result = await argument.Receive(token).ConfigureAwait(false);
    if (result.Close)
    {
      return;
    }
    await _dataHandler(result.Data, _param, token).ConfigureAwait(false);
  }

  [ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
  private sealed class Argument<T> : IDisposable
  {
    private readonly TaskData<T> _receiveTaskData;
    private readonly TaskData<T> _monitorTaskData;
    private readonly TokenClaim _tokenClaim;
    private readonly ILogger _logger;
    private readonly ITokenProvider _tokenProvider;
    private ClientWebSocket _clientWebSocket;

    public Argument(TaskData<T> receiveTaskData, TaskData<T> monitorTaskData, TokenClaimWithToken tokenClaimWithToken, ILogger logger, ITokenProvider tokenProvider)
    {
      _clientWebSocket = new ClientWebSocket();
      _receiveTaskData = receiveTaskData;
      _monitorTaskData = monitorTaskData;
      _tokenClaim = tokenClaimWithToken.TokenClaim;
      Token = tokenClaimWithToken.Token;
      _logger = logger;
      _tokenProvider = tokenProvider;
    }

    public string Token { get; private set; }

    public async Task<ApiError?> RecreateClient(string tenantId, CancellationToken cancellationToken)
    {
      var result = await _tokenProvider.GetTokenIfExpired(tenantId, _tokenClaim, Token, cancellationToken).ConfigureAwait(false);
      if (result.IsT2)
      {
        return result.AsT2;
      }
      var newToken = result.AsT0;
      if (newToken is not null)
      {
        Token = newToken;
      }
      _clientWebSocket.Dispose();
      _clientWebSocket = new ClientWebSocket();

      return null;
    }

    public Task<OneOf.Types.Error<string>?> Connect(Uri uri, CancellationToken cancellationToken)
    {
      var uriWithToken = GetUriWithToken(uri, Token);
      _logger.LogInformationStartWebsocketConnection(uriWithToken.ToString());

      return _clientWebSocket.Connect(uriWithToken, cancellationToken);
    }

    public WebSocketState State => 
      _clientWebSocket.State;

    public ValueTask Send(ReadOnlyMemory<byte> utf8Bytes, CancellationToken cancellationToken = default) =>
      _clientWebSocket.Send(utf8Bytes, cancellationToken);

    public Task<ReceiveResult> Receive(CancellationToken cancellationToken) =>
      _clientWebSocket.Receive(cancellationToken);

    public async Task Close(CancellationToken cancellationToken)
    {
      if (_clientWebSocket.State == WebSocketState.Open)
      {
        await _clientWebSocket.Close(cancellationToken).ConfigureAwait(false);
      }
      _clientWebSocket.Dispose();
      await _receiveTaskData.DisposeAsync().ConfigureAwait(false);
      await _monitorTaskData.DisposeAsync().ConfigureAwait(false);
    }

    private static Uri GetUriWithToken(Uri uri, string token) =>
      new(uri, $"?token={token}");

    public void Dispose() =>
      _clientWebSocket.Dispose();
  }
}