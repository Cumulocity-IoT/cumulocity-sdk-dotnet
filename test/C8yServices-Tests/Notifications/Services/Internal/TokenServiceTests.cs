using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;

using Moq;

namespace C8yServices.Notifications.Services.Internal;

public class TokenServiceTests
{
  private readonly TokenService _tokenService;
  private readonly Mock<ITokensApi> _tokensApiMock = new();

  public TokenServiceTests() => 
    _tokenService = new TokenService();

  [Fact]
  public async Task CreateTokenHappyPath()
  {
    const string token = "token";
    _tokensApiMock.Setup(api => api.CreateToken(It.IsAny<NotificationTokenClaims>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationToken { Token = token });
    var result = await _tokenService.CreateToken(_tokensApiMock.Object, new TokenClaim(string.Empty, "subscriptionName"));
    Assert.True(result.IsT0);
    Assert.Equal(token, result.AsT0);
  }

  [Fact]
  public async Task CreateTokenError()
  {
    _tokensApiMock.Setup(api => api.CreateToken(It.IsAny<NotificationTokenClaims>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationToken());
    var result = await _tokenService.CreateToken(_tokensApiMock.Object, new TokenClaim(string.Empty, "subscriptionName"));
    Assert.True(result.IsT1);
    Assert.Equal(Constants.NullResultApiError, result.AsT1);
  }

  [Fact]
  public async Task UnsubscribeHappyPath()
  {
    _tokensApiMock.Setup(api => api.UnsubscribeSubscriber(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new NotificationSubscriptionResult { PResult = NotificationSubscriptionResult.Result.DONE });
    var result = await _tokenService.Unsubscribe(_tokensApiMock.Object, "token");

    Assert.True(result.IsT0);
  }

  [Fact]
  public async Task UnsubscribeError()
  {
    var result = await _tokenService.Unsubscribe(_tokensApiMock.Object, "token");

    Assert.Equal(Constants.NullResultApiError, result);
  }
}