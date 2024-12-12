using System.Diagnostics.CodeAnalysis;

using C8yServices.Extensions.Hosting;

using C8yServices.Notifications.Services;
using C8yServices.Notifications.Services.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace C8yServices.Extensions.Notifications;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddNotifications(this IServiceCollection collection)
  {
    collection
      .TryAddTimeProvider()
      .AddSingleton<ITokenValidator, TokenValidator>()
      .AddSingleton<ITokenProvider, TokenProvider>()
      .AddSingleton<IClientWebSocketWrapperFactory, ClientWebSocketWrapperFactory>()
      .AddSingleton<INotificationServiceHelper, NotificationServiceHelper>()
      .AddSingleton<ITokenService, TokenService>()
      .AddSingleton<ISubscriptionService, SubscriptionService>()
      .AddSingleton<IMessageExtractor, MessageExtractor>()
      .AddSingleton<INotificationService>(static provider => new NotificationService(
        provider.GetRequiredService<IRealTimeWebSocketClientFactory>(), provider.GetRequiredService<INotificationServiceHelper>(), []))
      .AddCumulocityCoreLibraryProvider()
      .AddSingleton<IRealTimeWebSocketClientFactory>(static provider => new RealTimeWebSocketClientFactory(provider.GetRequiredService<ILogger<RealTimeWebSocketClientFactory>>(),
        provider.GetRequiredService<IClientWebSocketWrapperFactory>(),
        provider.GetRequiredService<INotificationServiceConfigurationProvider>(),
        provider.GetRequiredService<IMessageExtractor>(),
        provider.GetRequiredService<TimeProvider>()))      
      .AddSingleton<INotificationServiceConfigurationProvider, NotificationServiceConfigurationProvider>();

    return collection;
  }

  private static IServiceCollection TryAddTimeProvider(this IServiceCollection serviceCollection)
  {
    serviceCollection.TryAddSingleton(TimeProvider.System);

    return serviceCollection;
  }
}