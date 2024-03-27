using Moq;

namespace C8yServices.TestUtils;

/// <summary>
/// provides test extensions for the <see cref="IServiceProvider"/> interface
/// </summary>
public static class ServiceProviderMockExtensions
{
  /// <summary>
  /// creates and returns a new mock instance of type <typeparamref name="T"/> and sets up the given <see cref="IServiceProvider"/> to return the new mock when <see cref="IServiceProvider.GetService(Type)"/> is called
  /// </summary>
  public static Mock<IServiceProvider> SetupGetService<T>(this Mock<IServiceProvider>? serviceProviderMock, out Mock<T> serviceMock) where T : class
  {
    if (serviceProviderMock is null)
      throw new ArgumentNullException(nameof(serviceProviderMock));

    serviceMock = new Mock<T>();
    serviceProviderMock.Setup(sp => sp.GetService(typeof(T))).Returns(serviceMock.Object);
    return serviceProviderMock;
  }

  /// <summary>
  /// sets up the given <see cref="IServiceProvider"/> to return the given enumeration when <see cref="IServiceProvider.GetService(Type)"/> is called for an enumeration of type <typeparamref name="T"/>
  /// </summary>
  public static Mock<IServiceProvider> SetupGetServiceEnumerable<T>(this Mock<IServiceProvider>? serviceProviderMock, IEnumerable<T>? serviceEnumerable)
  {
    if (serviceProviderMock is null)
      throw new ArgumentNullException(nameof(serviceProviderMock));

    if (serviceEnumerable is null)
      throw new ArgumentNullException(nameof(serviceEnumerable));

    serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<T>))).Returns(serviceEnumerable);
    return serviceProviderMock;
  }
}
