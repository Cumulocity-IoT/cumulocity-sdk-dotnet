using System.Net.WebSockets;

using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Microsoft.Extensions.Logging;

using Moq;

namespace C8yServices.Notifications.Services.Internal;
public class RealTimeWebSocketClientHelperTests
{
  private readonly RealTimeWebSocketClientHelper<string> _helper;
  private readonly Mock<IClientWebSocketWrapperFactory> _clientWebSocketWrapperFactoryMock = new();
  private readonly Mock<IMessageExtractor> _messageExtractorMock = new();
  private readonly Mock<IDataFeedHandler> _dataFeedHandlerMock = new();
  private readonly Mock<Func<string, CancellationToken, Task<Error?>>> _reConnectFuncMock = new(); 
  private readonly Mock<ILogger> _loggerMock = new();
  private readonly string _tenantId = "100";

  public RealTimeWebSocketClientHelperTests()
  {
    _loggerMock.Setup(logger => logger.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    _helper = new RealTimeWebSocketClientHelper<string>(new Uri("http://localhost"),
      _loggerMock.Object, _clientWebSocketWrapperFactoryMock.Object,
      _dataFeedHandlerMock.Object, _reConnectFuncMock.Object, _messageExtractorMock.Object, string.Empty);
  }

  [Fact]
  public async Task ConnectHappyPath()
  {
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(new Mock<IClientWebSocketWrapper>().Object);
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), CancellationToken.None);
    Assert.Null(result);
  }

  [Fact]
  public async Task ConnectError()
  {
    const string error = "error";
    var mock = new Mock<IClientWebSocketWrapper>();
    mock.Setup(wrapper => wrapper.Connect(It.IsAny<TokenClaimWithToken>(),It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Error(false, error));
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(mock.Object);
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), CancellationToken.None);
    Assert.NotNull(result);
    Assert.Equal(error, result.Message);
  }

  [Fact]
  public async Task DataHandlerHappyPath()
  {
    var mock = new Mock<IClientWebSocketWrapper>();
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(mock.Object);
    _messageExtractorMock.Setup(extractor => extractor.GetMessageData(It.IsAny<string>()))
      .Returns(new MessageData("ack", "action", "api", "message"));
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), CancellationToken.None);
    Assert.Null(result);
    await _helper.DataHandler(new ReadOnlyMemory<byte>(), CancellationToken.None);
    _dataFeedHandlerMock.Verify(handler => handler.Handle(It.IsAny<MessageObject>(), It.IsAny<CancellationToken>()));
    mock.Verify(wrapper => wrapper.Send(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()));
  }

  [Theory]
  [InlineData(WebSocketState.Aborted)]
  [InlineData(WebSocketState.Closed)]
  public async Task MonitorHandlerCallsReconnectFunc(WebSocketState state)
  {
    await _helper.MonitorHandler(state, default);
    _reConnectFuncMock.Verify(func => func(It.IsAny<string>(), It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task MonitorHandlerDoesNotCallReconnectFunc()
  {
    await _helper.MonitorHandler(WebSocketState.Connecting, default);
    _reConnectFuncMock.Verify(func => func(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Disconnect()
  {
    var mock = new Mock<IClientWebSocketWrapper>();
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(mock.Object);
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), default);
    Assert.Null(result);
    await _helper.Disconnect(default);
    mock.Verify(wrapper => wrapper.Close(It.IsAny<CancellationToken>()));
    mock.Verify(wrapper => wrapper.DisposeAsync());
    Assert.Equal(NullClientWebSocketWrapper.Instance, _helper.ClientWebSocketWrapper);
  }

  [Fact]
  public async Task ReconnectHappyPath()
  {
    var mock = new Mock<IClientWebSocketWrapper>();
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(mock.Object);
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), CancellationToken.None);
    Assert.Null(result);
    result = await _helper.ReConnect(CancellationToken.None);
    Assert.Null(result);
  }

  [Fact]
  public async Task ReconnectOpen()
  {
    var mock = new Mock<IClientWebSocketWrapper>();
    mock.Setup(wrapper => wrapper.State).Returns(WebSocketState.Open);
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(mock.Object);
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), CancellationToken.None);
    Assert.Null(result);
    result = await _helper.ReConnect(CancellationToken.None);
    Assert.Null(result);
  }

  [Fact]
  public async Task ReconnectError()
  {
    const string error = "error";
    var mock = new Mock<IClientWebSocketWrapper>();
    _clientWebSocketWrapperFactoryMock.Setup(factory => factory.GetNewInstance(It.IsAny<ILogger>(), _tenantId, It.IsAny<Uri>(), It.IsAny<RealTimeWebSocketClientHelper<string>>(), It.IsAny<Func<ReadOnlyMemory<byte>, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>(), It.IsAny<Func<WebSocketState, RealTimeWebSocketClientHelper<string>, CancellationToken, Task>>()))
      .Returns(mock.Object);
    mock.Setup(wrapper => wrapper.ReConnect(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Error(false, error));
    var result = await _helper.Connect(_tenantId, new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"), CancellationToken.None);
    Assert.Null(result);
    result = await _helper.ReConnect(CancellationToken.None);
    Assert.NotNull(result);
    Assert.Equal(error, result.Message);
  }
}