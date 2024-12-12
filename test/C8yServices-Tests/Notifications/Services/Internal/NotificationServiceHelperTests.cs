using C8yServices.Bootstrapping;
using C8yServices.Common.Models;
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Supplementary;

using Moq;

namespace C8yServices.Notifications.Services.Internal;

public class NotificationServiceHelperTests
{
  private readonly NotificationServiceHelper _helper;
  private readonly Mock<ICumulocityCoreLibraryProvider> _c8yCoreLibraryProviderMock = new();
  private readonly Mock<ICumulocityCoreLibrary> _c8yCoreLibraryMock = new();
  private readonly Mock<ISubscriptionsApi> _subscriptionsApiMock = new();
  private readonly Mock<ITokensApi> _tokensApiMock = new();
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly Mock<ISubscriptionService> _subscriptionServiceMock = new();
  private readonly string _tenantId = "100";


  public NotificationServiceHelperTests()
  {
    _helper = new NotificationServiceHelper(_c8yCoreLibraryProviderMock.Object, _tokenServiceMock.Object, _subscriptionServiceMock.Object);
    _c8yCoreLibraryProviderMock.Setup(provider => provider.GetForTenant(_tenantId)).Returns(_c8yCoreLibraryMock.Object);
    _c8yCoreLibraryMock.Setup(c8yCoreLibrary => c8yCoreLibrary.Notifications20.TokensApi).Returns(_tokensApiMock.Object);
    _c8yCoreLibraryMock.Setup(c8yCoreLibrary => c8yCoreLibrary.Notifications20.SubscriptionsApi).Returns(_subscriptionsApiMock.Object);

  }

  [Fact]
  public async Task GetTokenDataIsNullGetSubscriptionFailure()
  {
    var tenantId = "100";
    _subscriptionServiceMock.Setup(service => service.Get(It.IsAny<ISubscriptionsApi>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error", null));
    var result = await _helper.GetToken(tenantId, new ApiSubscription("subscriptionName", ApiType.Alarms, null));

    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task GetTokenDataIsNullSubscriptionDoesNotExistCreateSubscriptionFailure()
  {
    _subscriptionServiceMock.Setup(service => service.Get(_subscriptionsApiMock.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((string?)null);
    _subscriptionServiceMock.Setup(service => service.Create(_subscriptionsApiMock.Object, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error", null));
    var result = await _helper.GetToken(_tenantId, new ApiSubscription("subscriptionName", ApiType.Alarms, null));

    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task GetTokenDataIsNullSubscriptionDoesNotExistCreateTokenFailure()
  {
    _subscriptionServiceMock.Setup(service => service.Get(_subscriptionsApiMock.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((string?)null);
    _subscriptionServiceMock.Setup(service => service.Create(_subscriptionsApiMock.Object, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("id");
    _tokenServiceMock.Setup(service => service.CreateToken(_tokensApiMock.Object, It.IsAny<TokenClaim>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error", null));
    var result = await _helper.GetToken(_tenantId, new ApiSubscription("subscriptionName", ApiType.Alarms, null));

    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task GetTokenDataIsNullSubscriptionDoesNotExistHappyPath()
  {
    _subscriptionServiceMock.Setup(service => service.Get(_subscriptionsApiMock.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((string?)null);
    _subscriptionServiceMock.Setup(service => service.Create(_subscriptionsApiMock.Object, It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("id");
    _tokenServiceMock.Setup(service => service.CreateToken(_tokensApiMock.Object, It.IsAny<TokenClaim>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("token");
    var result = await _helper.GetToken(_tenantId, new ApiSubscription("subscriptionName", ApiType.Alarms, null));

    Assert.Equal("token", result.AsT0.Token);
  }

  [Fact]
  public async Task GetTokenDataIsNullSubscriptionExistsCreateTokenFailure()
  {
    _subscriptionServiceMock.Setup(service => service.Get(_subscriptionsApiMock.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("id");
    _tokenServiceMock.Setup(service => service.CreateToken(_tokensApiMock.Object, It.IsAny<TokenClaim>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("error", null));
    var result = await _helper.GetToken(_tenantId, new ApiSubscription("subscriptionName", ApiType.Alarms, null));

    Assert.Equal("error", result.AsT2.Message);
  }

  [Fact]
  public async Task GetTokenDataIsNullSubscriptionExistsHappyPath()
  {
    _subscriptionServiceMock.Setup(service => service.Get(_subscriptionsApiMock.Object, It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("id");
    _tokenServiceMock.Setup(service => service.CreateToken(_tokensApiMock.Object, It.IsAny<TokenClaim>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("token");
    var result = await _helper.GetToken(_tenantId, new ApiSubscription("subscriptionName", ApiType.Alarms, null));

    Assert.Equal("token", result.AsT0.Token);
  }
}