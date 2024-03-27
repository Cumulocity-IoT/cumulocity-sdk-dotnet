using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;

using Moq;

namespace C8yServices.Bootstrapping;

public class CumulocityCoreLibraryFactoryHelperTests
{
  [Fact]
  public async Task GetApiCredentials()
  {
    const string tenant = "tenant";
    var apiMock = new Mock<ICurrentApplicationApi>();
    apiMock.Setup(api => api.GetSubscribedUsers(It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ApplicationUserCollection
      {
        PUsers = new List<ApplicationUserCollection.Users> { new() { Tenant = tenant, Name = string.Empty, Password = string.Empty }, new() { Tenant = "other tenant" } }
      });
    var helper = new CumulocityCoreLibrayFactoryHelper(apiMock.Object);
    var result = await helper.GetApiCredentials();

    Assert.Equal(2, result.Count());
  }

}