using Client.Com.Cumulocity.Client.Model;

using Moq;

namespace C8yServices.Authentication.Common;
public class AuthenticationVerifierTest
{
  private readonly AuthenticationVerifier _credentialVerifier;
  private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
  private readonly Mock<ICurrentTenantService> _currentTenantServiceMock = new();

  public AuthenticationVerifierTest()
  {
    _credentialVerifier = new AuthenticationVerifier(_currentUserServiceMock.Object, _currentTenantServiceMock.Object);
  }

  [Fact]
  public async Task AuthenticateAsyncSuccess()
  {
    const string headerKey = "someHeaderKey";
    const string headerValue = "someHeaderValue";
    const string username = "username";
    const string tenantName = "tenantName";
    const string schemeName = "someSchemeName";

    var headers = new Dictionary<string, string>
    {
      { headerKey, headerValue },
    };

    var currentUser = new CurrentUser
    {
      UserName = username,
    };
    _currentUserServiceMock.Setup(x => x.GetCurrentUser(headers)).ReturnsAsync(currentUser);

    var currentTenant = new CurrentTenant<CustomProperties>
    {
      Name = tenantName,
    };
    _currentTenantServiceMock.Setup(x => x.GetCurrentTenant(headers)).ReturnsAsync(currentTenant);


    var result = await _credentialVerifier.AuthenticateAsync(headers, schemeName);
    Assert.NotNull(result);
    Assert.Equal(schemeName, result.AuthenticationScheme);
    Assert.Equal(username, result.Principal.Identity!.Name);
  }

  [Fact]
  public async Task AuthenticateAsyncFailure()
  {
    const string authorizationHeader = "someAuthorizationHeader";

    var headers = new Dictionary<string, string>
    {
      { CommonAuthenticationDefaults.AuthorizationHeader, authorizationHeader },
    };

    _currentUserServiceMock.Setup(x => x.GetCurrentUser(headers)).ReturnsAsync(value: null);

    var result = await _credentialVerifier.AuthenticateAsync(headers, "someSchemeName");
    Assert.Null(result);
  }
}
