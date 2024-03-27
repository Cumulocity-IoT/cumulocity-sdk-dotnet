using System.Security.Claims;
using System.Text.Encodings.Web;

using C8yServices.Authentication.Common;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using Moq;

namespace C8yServices.Authentication.Basic;
public class BasicAuthenticationHandlerTest
{
  private const string SchemaName = BasicAuthenticationDefaults.AuthenticationScheme;

  private readonly BasicAuthenticationHandler _basicAuthenticationHandler;
  private readonly Mock<IAuthenticationVerifier> _authorizationHeaderCredentialVerifierMock = new();
  private readonly Mock<IOptionsMonitor<BasicAuthenticationOptions>> _optionsMonitorMock = new();
  private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();
  private readonly Mock<UrlEncoder> _encoderMock = new();
  public BasicAuthenticationHandlerTest()
  {
    var logger = new Mock<ILogger<BasicAuthenticationHandler>>();
    _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);
    _optionsMonitorMock.Setup(x => x.Get(BasicAuthenticationDefaults.AuthenticationScheme)).Returns(new BasicAuthenticationOptions());
    _basicAuthenticationHandler = new BasicAuthenticationHandler(_authorizationHeaderCredentialVerifierMock.Object, _optionsMonitorMock.Object, _loggerFactoryMock.Object, _encoderMock.Object);
  }

  [Fact]
  public async Task AuthenticateSuccess()
  {
    var context = new DefaultHttpContext();
    var authorizationHeader = new StringValues("Basic someBase64String");
    context.Request.Headers.Append(HeaderNames.Authorization, authorizationHeader);

    const string username = "username";

    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, authorizationHeader! },
    };

    var claims = new List<Claim> { new(ClaimTypes.Name, username) };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemaName));
    var ticket = new AuthenticationTicket(principal, SchemaName);

    _authorizationHeaderCredentialVerifierMock.Setup(x => x.AuthenticateAsync(headers, SchemaName)).ReturnsAsync(ticket);

    var authenticationScheme = new AuthenticationScheme(SchemaName, null, typeof(BasicAuthenticationHandler));
    await _basicAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _basicAuthenticationHandler.AuthenticateAsync();
    Assert.True(result.Succeeded);
    Assert.False(result.None);
    Assert.Equal(username, result.Principal.FindFirst(ClaimTypes.Name)!.Value);
    Assert.Equal(SchemaName, result.Ticket.AuthenticationScheme);
  }

  [Fact]
  public async Task AuthenticateNoResult()
  {
    var context = new DefaultHttpContext();
    var authorizationHeader = new StringValues("somethingOtherThanStartingWithBasic");
    context.Request.Headers.Append(HeaderNames.Authorization, authorizationHeader);

    var authenticationScheme = new AuthenticationScheme(BasicAuthenticationDefaults.AuthenticationScheme, null, typeof(BasicAuthenticationHandler));
    await _basicAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _basicAuthenticationHandler.AuthenticateAsync();
    Assert.False(result.Succeeded);
    Assert.True(result.None);
  }

  [Fact]
  public async Task AuthenticateFailure()
  {
    var context = new DefaultHttpContext();
    var authorizationHeader = new StringValues("Basic someBase64String");
    context.Request.Headers.Append(HeaderNames.Authorization, authorizationHeader);

    const string schemeName = "scheme";
    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, authorizationHeader! },
    };

    _authorizationHeaderCredentialVerifierMock.Setup(x => x.AuthenticateAsync(headers, schemeName)).ReturnsAsync(value: null);

    var authenticationScheme = new AuthenticationScheme(BasicAuthenticationDefaults.AuthenticationScheme, null, typeof(BasicAuthenticationHandler));
    await _basicAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _basicAuthenticationHandler.AuthenticateAsync();
    Assert.False(result.Succeeded);
    Assert.False(result.None);
  }
}
