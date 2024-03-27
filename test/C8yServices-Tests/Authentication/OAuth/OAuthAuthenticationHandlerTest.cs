using System.Security.Claims;
using System.Text.Encodings.Web;

using C8yServices.Authentication.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace C8yServices.Authentication.OAuth;

public class OAuthAuthenticationHandlerTest
{
  private const string SchemaName = OAuthAuthenticationDefaults.AuthenticationScheme;

  private readonly OAuthAuthenticationHandler _oauthAuthenticationHandler;
  private readonly Mock<IAuthenticationVerifier> _authenticationVerifier = new();
  private readonly Mock<IOptionsMonitor<OAuthAuthenticationOptions>> _optionsMonitorMock = new();
  private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();
  private readonly Mock<UrlEncoder> _encoderMock = new();
  public OAuthAuthenticationHandlerTest()
  {
    var logger = new Mock<ILogger<OAuthAuthenticationHandler>>();
    _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);
    _optionsMonitorMock.Setup(x => x.Get(OAuthAuthenticationDefaults.AuthenticationScheme)).Returns(new OAuthAuthenticationOptions());
    _oauthAuthenticationHandler = new OAuthAuthenticationHandler(_authenticationVerifier.Object, _optionsMonitorMock.Object, _loggerFactoryMock.Object, _encoderMock.Object);
  }

  [Fact]
  public async Task AuthenticateSuccess()
  {
    var context = new DefaultHttpContext();
    const string cookie = "some cookie";
    const string xxsrfToken = "some xxsrfToken";
    const string username = "username";
    context.Request.Headers.Append(OAuthAuthenticationDefaults.Cookie, cookie);
    context.Request.Headers.Append(OAuthAuthenticationDefaults.XXsrfToken, xxsrfToken);

    var headers = new Dictionary<string, string>
    {
      { OAuthAuthenticationDefaults.XXsrfToken, xxsrfToken },
      { OAuthAuthenticationDefaults.Cookie, cookie },
    };

    var claims = new List<Claim> { new(ClaimTypes.Name, username) };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemaName));
    var ticket = new AuthenticationTicket(principal, SchemaName);

    _authenticationVerifier.Setup(x => x.AuthenticateAsync(headers, OAuthAuthenticationDefaults.AuthenticationScheme)).ReturnsAsync(ticket);

    var authenticationScheme = new AuthenticationScheme(OAuthAuthenticationDefaults.AuthenticationScheme, null, typeof(OAuthAuthenticationHandler));
    await _oauthAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _oauthAuthenticationHandler.AuthenticateAsync();
    Assert.True(result.Succeeded);
    Assert.False(result.None);
    Assert.Equal(username, result.Principal.FindFirst(ClaimTypes.Name)!.Value);
    Assert.Equal(SchemaName, result.Ticket.AuthenticationScheme);
  }

  [Fact]
  public async Task AuthenticateNoResult()
  {
    var context = new DefaultHttpContext();

    var authenticationScheme = new AuthenticationScheme(OAuthAuthenticationDefaults.AuthenticationScheme, null, typeof(OAuthAuthenticationHandler));
    await _oauthAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _oauthAuthenticationHandler.AuthenticateAsync();
    Assert.False(result.Succeeded);
    Assert.True(result.None);
  }

  [Fact]
  public async Task AuthenticateFailure()
  {
    var context = new DefaultHttpContext();
    const string cookie = "some cookie";
    const string xxsrfToken = "some xxsrfToken";
    context.Request.Headers.Append(OAuthAuthenticationDefaults.Cookie, cookie);
    context.Request.Headers.Append(OAuthAuthenticationDefaults.XXsrfToken, xxsrfToken);

    var headers = new Dictionary<string, string>
    {
      { OAuthAuthenticationDefaults.XXsrfToken, xxsrfToken },
      { OAuthAuthenticationDefaults.Cookie, cookie },
    };

    _authenticationVerifier.Setup(x => x.AuthenticateAsync(headers, SchemaName)).ReturnsAsync(value: null);

    var authenticationScheme = new AuthenticationScheme(OAuthAuthenticationDefaults.AuthenticationScheme, null, typeof(OAuthAuthenticationHandler));
    await _oauthAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _oauthAuthenticationHandler.AuthenticateAsync();
    Assert.False(result.Succeeded);
    Assert.False(result.None);
  }
}
