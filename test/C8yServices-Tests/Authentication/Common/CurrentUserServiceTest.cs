using System.Net;
using System.Net.Http.Json;

using C8yServices.Configuration;

using Client.Com.Cumulocity.Client.Model;

using Microsoft.Extensions.Options;

using RichardSzalay.MockHttp;

namespace C8yServices.Authentication.Common;

public sealed class CurrentUserServiceTest : IDisposable
{
  private readonly CurrentUserService _currentUserService;
  private readonly MockHttpMessageHandler _handler = new();
  private const string BaseUrl = "http://someBaseUrl";

  public CurrentUserServiceTest()
  {
    var configuration = Options.Create(new C8YConfiguration
    {
      BaseUrl = new Uri(BaseUrl)
    });
    _currentUserService = new CurrentUserService(new HttpClient(_handler), configuration);
  }

  [Fact]
  public async Task GetCurrentUserSuccess()
  {
    const string username = "username";
    _handler
      .When(HttpMethod.Get, $"{BaseUrl}/{CurrentUserService.CurrentUserPath}")
      .Respond(HttpStatusCode.OK, JsonContent.Create(new CurrentUser
      {
        UserName = username
      }));
    var headerDictionary = new Dictionary<string, string>
    {
      { "someKey", "someValue" }
    };
    var result = await _currentUserService.GetCurrentUser(headerDictionary);
    Assert.NotNull(result);
  }

  [Fact]
  public async Task GetCurrentUserFailure()
  {
    _handler
      .When(HttpMethod.Get, $"{BaseUrl}/{CurrentUserService.CurrentUserPath}")
      .Respond(HttpStatusCode.NotFound);
    var result = await _currentUserService.GetCurrentUser(new Dictionary<string, string>());
    Assert.Null(result);
  }

  public void Dispose() =>
    _handler.Dispose();
}