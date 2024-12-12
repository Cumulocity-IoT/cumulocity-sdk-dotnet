using C8yServices.Bootstrapping;
using C8yServices.Common.Models;
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Supplementary;

using Moq;

namespace C8yServices.Notifications.Services.Internal;

public class TokenProviderTests
{
  private readonly TokenProvider _tokenProvider;
  private readonly Mock<ICumulocityCoreLibraryProvider> _c8yCoreLibraryProviderMock = new();
  private readonly Mock<ICumulocityCoreLibrary> _c8yCoreLibraryMock = new();
  private readonly Mock<ITokensApi> _tokensApiMock = new();
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly Mock<ITokenValidator> _tokenValidatorMock = new();
  private readonly string _tenantId = "100";

  public TokenProviderTests()
  {
    _tokenProvider = new TokenProvider(_c8yCoreLibraryProviderMock.Object, _tokenServiceMock.Object, _tokenValidatorMock.Object);
    _c8yCoreLibraryProviderMock.Setup(provider => provider.GetForTenant(_tenantId)).Returns(_c8yCoreLibraryMock.Object);
    _c8yCoreLibraryMock.Setup(c8yCoreLibrary => c8yCoreLibrary.Notifications20.TokensApi).Returns(_tokensApiMock.Object);
  }


  [Fact]
  public async Task NoNeedToRefreshToken()
  {
    var result = await _tokenProvider.GetTokenIfExpired(_tenantId, new TokenClaim("s1", "subscriptionName"), "token", CancellationToken.None);

    Assert.True(result.IsT0);
    Assert.Null(result.AsT0);
  }

  [Fact]
  public async Task NeedToRefreshToken()
  {
    _tokenValidatorMock.Setup(validator => validator.IsExpired(It.IsAny<string>())).Returns(true);
    _tokenServiceMock.Setup(service => service.CreateToken(_tokensApiMock.Object, It.IsAny<TokenClaim>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync("token");
    var result = await _tokenProvider.GetTokenIfExpired(_tenantId, new TokenClaim("s1", "subscriptionName"), "token", CancellationToken.None);

    Assert.True(result.IsT0);
    Assert.Equal("token", result.AsT0);
  }

  [Fact]
  public async Task NeedToRefreshTokenError()
  {
    _tokenValidatorMock.Setup(validator => validator.IsExpired(It.IsAny<string>())).Returns(true);
    _tokenServiceMock.Setup(service => service.CreateToken(_tokensApiMock.Object, It.IsAny<TokenClaim>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApiError("message", null));
    var result = await _tokenProvider.GetTokenIfExpired(_tenantId, new TokenClaim("s1", "subscriptionName"), "token", CancellationToken.None);

    Assert.True(result.IsT2);
  }
}