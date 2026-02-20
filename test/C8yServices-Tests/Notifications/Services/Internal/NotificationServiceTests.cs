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
  private readonly string _tenantId = "100";
  private readonly string _subscriptionName = "subscriptionName";

  public NotificationServiceTests()
  {
    // By default, use _realTimeWebSocketClientMock for all client creation
    var factoryMock = new Mock<IRealTimeWebSocketClientFactory>();
    factoryMock.Setup(factory => factory.Create(It.IsAny<IDataFeedHandler>(), It.IsAny<CancellationToken>())).ReturnsAsync(_realTimeWebSocketClientMock.Object);
    _notificationService = new NotificationService(_tenantId, factoryMock.Object, _notificationServiceHelperMock.Object);
  }

  [Fact]
  public async Task GetWebSocketStateFound()
  {
    _realTimeWebSocketClientMock.Setup(c => c.State).Returns(WebSocketState.Open);
    var registerNotification = CreateRegisterNotification();
    await _notificationService.Register(new WithHandlerRegisterNotification(registerNotification, _dataFeedHandlerMock.Object));
    var result = _notificationService.GetWebSocketState(_subscriptionName);

    Assert.True(result.IsT0);
    Assert.Equal(WebSocketState.Open, result.AsT0);
  }

  [Fact]
  public void GetWebSocketStateNotFound()
  {
    var result = _notificationService.GetWebSocketState(_subscriptionName);

    Assert.True(result.IsT1);
  }

  [Fact]
  public async Task RegisterGetTokenFailure()
  {
    _notificationServiceHelperMock.Setup(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error", null));
    var result = await _notificationService.Register(CreateWithHandlerRegisterNotification());

    Assert.True(result.IsT2);
    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task RegisterConnectFailure()
  {
    _notificationServiceHelperMock.Setup(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new TokenClaimWithToken(new TokenClaim("s1", _subscriptionName), "token"));
    _realTimeWebSocketClientMock.Setup(client => client.Connect(_tenantId, It.IsAny<TokenClaimWithToken>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Error(false, "error"));
    var result = await _notificationService.Register(CreateWithHandlerRegisterNotification());

    Assert.True(result.IsT2);
    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task RegisterConnectHappyPath()
  {
    _notificationServiceHelperMock.Setup(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new TokenClaimWithToken(new TokenClaim("s1", "subscriptionName"), "token"));
    var result = await _notificationService.Register(CreateWithHandlerRegisterNotification());

    Assert.True(result.IsT0);
  }

  [Fact]
  public async Task UnregisterHappyPathClientNotFound()
  {
    var result = await _notificationService.Unregister(_subscriptionName);

    Assert.True(result.IsT1);
  }

  [Fact]
  public async Task UnregisterHappyPathClientFound()
  {
    _realTimeWebSocketClientMock.Setup(client => client.Token).Returns("value");
    await _notificationService.Register(new WithHandlerRegisterNotification(CreateRegisterNotification(), _dataFeedHandlerMock.Object));
    _notificationServiceHelperMock.Setup(helper => helper.Unsubscribe(_tenantId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new Success());
    var result = await _notificationService.Unregister(_subscriptionName);

    Assert.True(result.IsT0);
  }

  [Fact]
  public async Task UnregisterNoToken()
  {
    // Setup client to simulate missing token
    _realTimeWebSocketClientMock.Setup(client => client.Token).Returns(value: null);
    await _notificationService.Register(new WithHandlerRegisterNotification(CreateRegisterNotification(), _dataFeedHandlerMock.Object));
    var result = await _notificationService.Unregister(_subscriptionName);

    Assert.True(result.IsT1);
    _notificationServiceHelperMock.Verify(helper => helper.GetToken(_tenantId, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UnregisterErrorsClientFound()
  {
    _realTimeWebSocketClientMock.Setup(client => client.Token).Returns("token");
    await _notificationService.Register(new WithHandlerRegisterNotification(CreateRegisterNotification(), _dataFeedHandlerMock.Object));
    _notificationServiceHelperMock.Setup(helper => helper.Unsubscribe(_tenantId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error1", null));
    var result = await _notificationService.Unregister(_subscriptionName);

    Assert.True(result.IsT3);
  }

  [Fact]
  public async Task DisposeAsyncShouldWork()
  {
    await _notificationService.Register(new WithHandlerRegisterNotification(CreateRegisterNotification(), _dataFeedHandlerMock.Object));
    await _notificationService.DisposeAsync();

    // No direct access to _clients; check that DisposeAsync does not throw and completes.
    Assert.True(true);
  }

  public async ValueTask DisposeAsync() => 
    await _notificationService.DisposeAsync();

  private WithHandlerRegisterNotification CreateWithHandlerRegisterNotification() =>
    new(CreateRegisterNotification(), _dataFeedHandlerMock.Object);

  private TenantRegisterNotification CreateRegisterNotification() =>
    TenantRegisterNotification.TryCreate(_subscriptionName, new[] { ApiType.Alarms }, null, null, null).AsT0;

}