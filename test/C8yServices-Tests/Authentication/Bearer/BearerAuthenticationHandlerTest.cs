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

namespace C8yServices.Authentication.Bearer;
public class BearerAuthenticationHandlerTest
{
  private const string SchemaName = BearerAuthenticationDefaults.AuthenticationScheme;

  private readonly BearerAuthenticationHandler _bearerAuthenticationHandler;
  private readonly Mock<IAuthenticationVerifier> _authenticationVerifierMock = new();
  private readonly Mock<IOptionsMonitor<BearerAuthenticationOptions>> _optionsMonitorMock = new();
  private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();
  private readonly Mock<UrlEncoder> _encoderMock = new();
  public BearerAuthenticationHandlerTest()
  {
    var logger = new Mock<ILogger<BearerAuthenticationHandler>>();
    _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);
    _optionsMonitorMock.Setup(x => x.Get(BearerAuthenticationDefaults.AuthenticationScheme)).Returns(new BearerAuthenticationOptions());
    _bearerAuthenticationHandler = new BearerAuthenticationHandler(_authenticationVerifierMock.Object, _optionsMonitorMock.Object, _loggerFactoryMock.Object, _encoderMock.Object);
  }

  [Fact]
  public async Task AuthenticateSuccess()
  {
    var context = new DefaultHttpContext();
    var authorizationHeader = new StringValues("Bearer someBase64String");
    context.Request.Headers.Append(HeaderNames.Authorization, authorizationHeader);
    const string username = "username";

    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, authorizationHeader! },
    };

    var claims = new List<Claim> { new(ClaimTypes.Name, username) };
    var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemaName));
    var ticket = new AuthenticationTicket(principal, SchemaName);

    _authenticationVerifierMock.Setup(x => x.AuthenticateAsync(headers, SchemaName)).ReturnsAsync(ticket);

    var authenticationScheme = new AuthenticationScheme(SchemaName, null, typeof(BearerAuthenticationHandler));
    await _bearerAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _bearerAuthenticationHandler.AuthenticateAsync();
    Assert.True(result.Succeeded);
    Assert.False(result.None);
    Assert.Equal(username, result.Principal.FindFirst(ClaimTypes.Name)!.Value);
    Assert.Equal(SchemaName, result.Ticket.AuthenticationScheme);
  }

  [Fact]
  public async Task AuthenticateNoResult()
  {
    var context = new DefaultHttpContext();
    var authorizationHeader = new StringValues("somethingOtherThanStartingWithBearer");
    context.Request.Headers.Append(HeaderNames.Authorization, authorizationHeader);

    var authenticationScheme = new AuthenticationScheme(SchemaName, null, typeof(BearerAuthenticationHandler));
    await _bearerAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _bearerAuthenticationHandler.AuthenticateAsync();
    Assert.False(result.Succeeded);
    Assert.True(result.None);
  }

  [Fact]
  public async Task AuthenticateFailure()
  {
    var context = new DefaultHttpContext();
    var authorizationHeader = new StringValues("Bearer someBase64String");
    context.Request.Headers.Append(HeaderNames.Authorization, authorizationHeader);

    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, authorizationHeader! },
    };

    _authenticationVerifierMock.Setup(x => x.AuthenticateAsync(headers, SchemaName)).ReturnsAsync(value: null);

    var authenticationScheme = new AuthenticationScheme(SchemaName, null, typeof(BearerAuthenticationHandler));
    await _bearerAuthenticationHandler.InitializeAsync(authenticationScheme, context);
    var result = await _bearerAuthenticationHandler.AuthenticateAsync();
    Assert.False(result.Succeeded);
    Assert.False(result.None);
  }
}
