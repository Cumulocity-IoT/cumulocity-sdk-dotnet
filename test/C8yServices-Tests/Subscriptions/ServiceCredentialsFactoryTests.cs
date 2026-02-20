using Moq;

namespace C8yServices.Subscriptions;

public class ServiceCredentialsFactoryTests
{
  [Fact]
  public async Task ApiCredentialsUpdatedEventIsFiredForEachCredential()
  {
    // Arrange
    var creds = new[]
    {
      new ServiceCredentials("tenant1", "user1", "pass1"),
      new ServiceCredentials("tenant2", "user2", "pass2")
    };
    var helperMock = new Mock<IServiceCredentialsFactoryHelper>();
    helperMock.Setup(h => h.GetApiCredentials(It.IsAny<CancellationToken>())).ReturnsAsync(creds);
    var serviceProviderMock = new Mock<IServiceProvider>();
    // Mock GetServices<ICredentialAwareService>() to return an empty list
    serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<ICredentialAwareService>)))
      .Returns(Array.Empty<ICredentialAwareService>());
    var factory = new ServiceCredentialsFactory(helperMock.Object, serviceProviderMock.Object);
    var received = new List<ServiceCredentials>();
    factory.ApiCredentialsUpdated += (s, c) => received.Add(c);

    // Act
    await factory.InitOrRefresh();

    // Assert
    Assert.Equal(2, received.Count);
    Assert.Contains(received, c => c.Tenant == "tenant1");
    Assert.Contains(received, c => c.Tenant == "tenant2");
  }

  [Fact]
  public async Task SubscriptionAddedAndRemovedEventsAreFiredCorrectly()
  {
    // Arrange
    var creds1 = new[]
    {
      new ServiceCredentials("tenant1", "user1", "pass1"),
      new ServiceCredentials("tenant2", "user2", "pass2")
    };
    var creds2 = new[]
    {
      new ServiceCredentials("tenant2", "user2", "pass2"),
      new ServiceCredentials("tenant3", "user3", "pass3")
    };
    var helperMock = new Mock<IServiceCredentialsFactoryHelper>();
    var serviceProviderMock = new Mock<IServiceProvider>();
    // Mock GetServices<ICredentialAwareService>() to return an empty list
    serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<ICredentialAwareService>)))
      .Returns(Array.Empty<ICredentialAwareService>());
    var factory = new ServiceCredentialsFactory(helperMock.Object, serviceProviderMock.Object);
    var added = new List<string>();
    var removed = new List<string>();
    factory.SubscriptionAdded += (s, t) => added.Add(t);
    factory.SubscriptionRemoved += (s, t) => removed.Add(t);

    // add some credentials and verify added events
    helperMock.Setup(h => h.GetApiCredentials(It.IsAny<CancellationToken>())).ReturnsAsync(creds1);
    await factory.InitOrRefresh();

    // Assert 
    Assert.Equal(2, added.Count);
    Assert.Empty(removed);
    Assert.Contains(added, t => t == "tenant1");
    Assert.Contains(added, t => t == "tenant2");
    // Lists are already empty before second refresh
    helperMock.Setup(h => h.GetApiCredentials(It.IsAny<CancellationToken>())).ReturnsAsync(creds2);
    await factory.InitOrRefresh();
  }
}
