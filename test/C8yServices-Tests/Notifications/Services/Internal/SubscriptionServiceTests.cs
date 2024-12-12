using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;

using Moq;

namespace C8yServices.Notifications.Services.Internal;

public class SubscriptionServiceTests
{
  private readonly SubscriptionService _subscriptionService;
  private readonly Mock<ISubscriptionsApi> _subscriptionsApiMock = new();

  public SubscriptionServiceTests() => 
    _subscriptionService = new SubscriptionService();

  [Fact]
  public async Task CreateHappyPath()
  {
    const string id = "id";
    _subscriptionsApiMock.Setup(api => api.CreateSubscription(It.IsAny<NotificationSubscription>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationSubscription { Id = id });
    var result = await _subscriptionService.Create(_subscriptionsApiMock.Object, new ApiSubscription("subscriptionName", ApiType.Alarms, null));
    Assert.True(result.IsT0);
    Assert.Equal(id, result.AsT0);
  }

  [Fact]
  public async Task CreateError()
  {
    var result = await _subscriptionService.Create(_subscriptionsApiMock.Object, new ApiSubscription("subscriptionName", ApiType.Alarms, null));
    Assert.True(result.IsT1);
    Assert.Equal(Constants.NullResultApiError, result.AsT1);
  }

  [Fact]
  public async Task GetHappyPath()
  {
    const string id = "id";
    _subscriptionsApiMock.Setup(api => api.GetSubscriptions(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationSubscriptionCollection { Subscriptions = [new() { Id = id }] });
    var result = await _subscriptionService.Get(_subscriptionsApiMock.Object, "subscriptionName");
    Assert.True(result.IsT0);
    Assert.Equal(id, result.AsT0);
  }

  [Fact]
  public async Task GetWasNotFound()
  {
    _subscriptionsApiMock.Setup(api => api.GetSubscriptions(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationSubscriptionCollection { Subscriptions = [] });
    var result = await _subscriptionService.Get(_subscriptionsApiMock.Object, "subscriptionName");
    Assert.True(result.IsT0);
    Assert.Null(result.AsT0);
  }

  [Fact]
  public async Task GetError()
  {
    var result = await _subscriptionService.Get(_subscriptionsApiMock.Object, "subscriptionName");
    Assert.True(result.IsT1);
    Assert.Equal(Constants.NullResultApiError, result.AsT1);
  }

  [Fact]
  public async Task DeleteExists()
  {
    _subscriptionsApiMock.Setup(api => api.GetSubscriptions(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationSubscriptionCollection
      {
        Subscriptions = [new() { Id = "id" }]
      });
    var result = await _subscriptionService.Delete(_subscriptionsApiMock.Object, "subscriptionName");

    Assert.True(result.IsT0);
    _subscriptionsApiMock.Verify(api => api.DeleteSubscription(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()));
  }

  [Fact]
  public async Task DeleteDoesNotExist()
  {
    _subscriptionsApiMock.Setup(api => api.GetSubscriptions(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationSubscriptionCollection
      {
        Subscriptions = []
      });
    var result = await _subscriptionService.Delete(_subscriptionsApiMock.Object, "subscriptionName");

    Assert.True(result.IsT1);
    _subscriptionsApiMock.Verify(api => api.DeleteSubscription(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteNullResult()
  {
    var result = await _subscriptionService.Delete(_subscriptionsApiMock.Object, "subscriptionName");

    Assert.Equal(Constants.NullResultApiError, result);
  }
}