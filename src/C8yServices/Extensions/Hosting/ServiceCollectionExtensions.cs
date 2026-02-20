using C8yServices.Authentication.Common;
using C8yServices.Configuration;
using C8yServices.Extensions.Http;
using C8yServices.Extensions.Notifications;
using C8yServices.MqttService;
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Services;
using C8yServices.RestApi;
using C8yServices.Subscriptions;

using Client.Com.Cumulocity.Client.Api;

using FluentValidation;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace C8yServices.Extensions.Hosting;

/// <summary>
/// provides extensions to the <see cref="IServiceCollection"/> interface
/// </summary>
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddNotifications(this IServiceCollection collection, IConfiguration configuration)
  {
    collection.AddTransient<IValidator<NotificationServiceConfiguration>, NotificationServiceConfigurationValidator>();
    collection.AddConfigurationWithValidation<NotificationServiceConfiguration>(configuration, NotificationServiceConfiguration.Section);
    collection.AddNotifications();

    return collection;
  }

  /// <summary>
  /// Adds service credentials infrastructure for multi-tenant credential management.
  /// This is called automatically by <see cref="AddPulsarServices"/> and <see cref="AddCumulocityCoreLibraryProvider"/>.
  /// Requires <see cref="AddC8YConfigurationFromCumulocityPlatform"/> or <see cref="AddC8YConfiguration"/> to be called first.
  /// </summary>
  /// <param name="serviceCollection">The service collection to add services to.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddServiceCredentials(this IServiceCollection serviceCollection)
  {
    serviceCollection.AddSingleton<IServiceCredentialsFactoryHelper, ServiceCredentialsFactoryHelper>();
    serviceCollection.AddSingleton<IServiceCredentialsFactory, ServiceCredentialsFactory>();
    serviceCollection.AddSingleton<ServiceCredentialsRefreshJob>();
    serviceCollection.AddCurrentApplicationApi();

    return serviceCollection;
  }

  /// <summary>
  /// Adds Pulsar/MQTT services for multi-tenant message consumption and production.
  /// Automatically includes service credentials infrastructure.
  /// Requires <see cref="AddC8YConfigurationFromCumulocityPlatform"/> or <see cref="AddC8YConfiguration"/> to be called first.
  /// </summary>
  /// <param name="serviceCollection">The service collection to add services to.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddPulsarServices(this IServiceCollection serviceCollection)
  {
    serviceCollection.AddServiceCredentials();
    serviceCollection.AddSingleton<PulsarServiceProvider>();
    serviceCollection.AddSingleton<IPulsarServiceProvider>(sp => sp.GetRequiredService<PulsarServiceProvider>());
    serviceCollection.AddSingleton<ICredentialAwareService>(sp => sp.GetRequiredService<PulsarServiceProvider>());

    return serviceCollection;
  }

  /// <summary>
  /// Adds Cumulocity REST API provider for multi-tenant API access.
  /// Automatically includes service credentials infrastructure.
  /// Requires <see cref="AddC8YConfigurationFromCumulocityPlatform"/> or <see cref="AddC8YConfiguration"/> to be called first.
  /// </summary>
  /// <param name="serviceCollection">The service collection to add services to.</param>
  /// <returns>The service collection for chaining.</returns>
  public static IServiceCollection AddCumulocityCoreLibraryProvider(this IServiceCollection serviceCollection)
  {
    serviceCollection.AddServiceCredentials();
    serviceCollection.AddSingleton<CumulocityCoreLibrayProvider>();
    serviceCollection.AddSingleton<ICumulocityCoreLibraryProvider>(sp => sp.GetRequiredService<CumulocityCoreLibrayProvider>());
    serviceCollection.AddSingleton<ICredentialAwareService>(sp => sp.GetRequiredService<CumulocityCoreLibrayProvider>());

    return serviceCollection;
  }

  public static IServiceCollection AddC8YConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration) =>
    serviceCollection.AddConfigurationWithValidation<C8YConfiguration>(configuration, C8YConfiguration.Section, true);

  public static IServiceCollection AddC8YConfigurationFromCumulocityPlatform(this IServiceCollection serviceCollection) =>
    serviceCollection.AddSingleton<IOptions<C8YConfiguration>>(static _ => new OptionsWrapper<C8YConfiguration>(C8YConfiguration.FromCumulocityPlatform()));

  public static IServiceCollection AddConfigurationWithValidation<T>(this IServiceCollection serviceCollection,
    IConfiguration configuration, string sectionName, bool sectionRequired = false) where T : class
  {
    serviceCollection.AddOptions<T>()
      .Bind(sectionRequired ? configuration.GetRequiredSection(sectionName) : configuration.GetSection(sectionName))
      .ValidateUsingFluentValidateOptions()
      .ValidateOnStart();

    return serviceCollection;
  }

  public static IServiceCollection AddCurrentApplicationApi(this IServiceCollection serviceCollection)
  {
    serviceCollection.AddSingleton<ICurrentApplicationApi>(static provider => provider.GetRequiredService<CurrentApplicationApi>());
    serviceCollection.AddHttpClient<CurrentApplicationApi>(static (provider, client) =>
    {
      var configuration = provider.GetRequiredService<IOptions<C8YConfiguration>>().Value;
      client.BaseAddress = configuration.BaseUrl;
      client.UpdateBasicAuth(configuration.BootstrapTenant, configuration.BootstrapUsername, configuration.BootstrapPassword);
    }).SetupCommonHttpMessageHandler();

    return serviceCollection;
  }

  public static IServiceCollection AddCurrentUserService(this IServiceCollection serviceCollection)
  {
    serviceCollection.TryAddSingleton<ICurrentUserService, CurrentUserService>();
    serviceCollection.AddHttpClient<CurrentUserService>().SetupCommonHttpMessageHandler();

    return serviceCollection;
  }

  public static IServiceCollection AddCurrentTenantService(this IServiceCollection serviceCollection)
  {
    serviceCollection.TryAddSingleton<ICurrentTenantService, CurrentTenantService>();
    serviceCollection.AddHttpClient<CurrentTenantService>().SetupCommonHttpMessageHandler();

    return serviceCollection;
  }

  private static void SetupCommonHttpMessageHandler(this IHttpClientBuilder httpClientBuilder) =>
    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) })
      .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
}