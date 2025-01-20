using System.Collections.Concurrent;
using System.Net.WebSockets;

using C8yServices.Common.Models;
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Moq;

using OneOf.Types;

using Error = C8yServices.Notifications.Models.Internal.Error;

namespace C8yServices.Notifications.Services.Internal;

public sealed class NotificationServiceTests : IAsyncDisposable
{
  private readonly NotificationService _notificationService;
  private readonly Mock<INotificationServiceHelper> _notificationServiceHelperMock = new();
  private readonly Mock<IRealTimeWebSocketClient> _realTimeWebSocketClientMock = new();
  private readonly Mock<IDataFeedHandler> _dataFeedHandlerMock = new();
  private readonly ConcurrentDictionary<string, IRealTimeWebSocketClient> _clients = [];
  private readonly string _tenantId = "100";

  public NotificationServiceTests()
  {
    var mock = new Mock<IRealTimeWebSocketClientFactory>();
    mock.Setup(factory => factory.Create(It.IsAny<IDataFeedHandler>(), It.IsAny<CancellationToken>())).ReturnsAsync(_realTimeWebSocketClientMock.Object);
    _notificationService = new NotificationService(mock.Object, _notificationServiceHelperMock.Object, _clients);
  }

  [Fact]
  public void GetWebSocketStateFound()
  {
    var subscriptionName = "subscriptionName";
    var mock = new Mock<IRealTimeWebSocketClient>();
    mock.Setup(client => client.State).Returns(WebSocketState.Open);
    _clients.TryAdd(_tenantId + "_" + subscriptionName, mock.Object);
    var result = _notificationService.GetWebSocketState(_tenantId, subscriptionName);

    Assert.True(result.IsT0);
    Assert.Equal(WebSocketState.Open, result.AsT0);
  }

  [Fact]
  public void GetWebSocketStateNotFound()
  {
    var subscriptionName = "subscriptionName";
    var result = _notificationService.GetWebSocketState(_tenantId, subscriptionName);

    Assert.True(result.IsT1);
  }

  [Fact]
  public async Task RegisterGetTokenFailure()
  {
    _notificationServiceHelperMock.Setup(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error", null));
    var result = await _notificationService.Register(_tenantId, CreateWithHandlerRegisterNotification());

    Assert.True(result.IsT2);
    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task RegisterConnectFailure()
  {
    _notificationServiceHelperMock.Setup(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"));
    _realTimeWebSocketClientMock.Setup(client => client.Connect(_tenantId, It.IsAny<TokenClaimWithToken>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Error(false, "error"));
    var result = await _notificationService.Register(_tenantId, CreateWithHandlerRegisterNotification());

    Assert.True(result.IsT2);
    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task RegisterConnectHappyPath()
  {
    _notificationServiceHelperMock.Setup(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"));
    var result = await _notificationService.Register(_tenantId, CreateWithHandlerRegisterNotification());

    Assert.True(result.IsT0);
  }

  [Fact]
  public async Task UnregisterHappyPathClientNotFound()
  {
    var result = await _notificationService.Unregister(_tenantId, "subscriptionName");

    Assert.True(result.IsT1);
  }

  [Fact]
  public async Task UnregisterHappyPathClientFound()
  {
    var subscriptionName = "subscriptionName";
    var mock = new Mock<IRealTimeWebSocketClient>();
    mock.Setup(client => client.Token).Returns("value");
    var value = mock.Object;
    _clients.AddOrUpdate(_tenantId + "_" + subscriptionName, value, (_, _) => value);
    _notificationServiceHelperMock.Setup(helper => helper.Unsubscribe(_tenantId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Success());
    var result = await _notificationService.Unregister(_tenantId, subscriptionName);

    Assert.True(result.IsT0);
  }

  [Fact]
  public async Task UnregisterNoToken()
  {
    var subscriptionName = "subscriptionName";
    var mock = new Mock<IRealTimeWebSocketClient>();
    var value = mock.Object;
    _clients.AddOrUpdate(_tenantId + "_" + subscriptionName, value, (_, _) => value);
    var result = await _notificationService.Unregister(_tenantId, subscriptionName);

    Assert.True(result.IsT1);
    _notificationServiceHelperMock.VerifyNoOtherCalls();
  }

  [Fact]
  public async Task UnregisterErrorsClientFound()
  {
    var subscriptionName = "subscriptionName";
    var mock = new Mock<IRealTimeWebSocketClient>();
    mock.Setup(client => client.Token).Returns("token");
    var value = mock.Object;
    _clients.AddOrUpdate(_tenantId + "_" + subscriptionName, value, (_, _) => value);
    _notificationServiceHelperMock.Setup(helper => helper.Unsubscribe(_tenantId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error1", null));
    var result = await _notificationService.Unregister(_tenantId, subscriptionName);

    Assert.True(result.IsT3);
  }

  [Fact]
  public async Task DisposeAsyncShouldWork()
  {
    var subscriptionName = "subscriptionName";
    var value = new Mock<IRealTimeWebSocketClient>().Object;
    _clients.AddOrUpdate(_tenantId + "_" + subscriptionName, value, (_, _) => value);
    await _notificationService.DisposeAsync();

    Assert.Empty(_clients);
  }

  public async ValueTask DisposeAsync() => 
    await _notificationService.DisposeAsync();

  private WithHandlerRegisterNotification CreateWithHandlerRegisterNotification() =>
    new(CreateRegisterNotification(), _dataFeedHandlerMock.Object);

  private static ApiRegisterNotification CreateRegisterNotification() =>
    ApiRegisterNotification.TryCreate("serviceName", ApiType.Alarms, null).AsT0;

}