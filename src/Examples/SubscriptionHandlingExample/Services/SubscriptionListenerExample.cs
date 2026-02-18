using C8yServices.RestApi;
using C8yServices.Subscriptions;

namespace SubscriptionHandlingExample.Services;
public sealed class SubscriptionListenerExample : BackgroundService
{
  private readonly ILogger<SubscriptionListenerExample> _logger;
  private readonly IServiceCredentialsFactory _serviceCredentialsFactory;
  private readonly ISubscriptionEventService _subscriptionEventService;

  public SubscriptionListenerExample(ILogger<SubscriptionListenerExample> logger, IServiceCredentialsFactory serviceCredentialsFactory, ISubscriptionEventService subscriptionEventService)
  {
    _logger = logger;
    _serviceCredentialsFactory = serviceCredentialsFactory;
    _subscriptionEventService = subscriptionEventService;
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _serviceCredentialsFactory.SubscriptionAdded += OnSubscriptionAdded;
    _serviceCredentialsFactory.SubscriptionRemoved += OnSubscriptionRemoved;
    _logger.LogInformation("Subscription listener initialized");
    return Task.CompletedTask;
  }

  private void OnSubscriptionAdded(object? sender, string tenant)
  {
    _logger.LogInformation("Subscription added on {Tenant}", tenant);
    Task.Run(() => _subscriptionEventService.CreateSubscriptionAddedEvent(tenant, CancellationToken.None));
  }

  private void OnSubscriptionRemoved(object? sender, string tenant)
  {
    _logger.LogInformation("Subscription removed on {Tenant}", tenant);
    Task.Run(() => _subscriptionEventService.CreateSubscriptionRemovedEvent(tenant, CancellationToken.None));
  }
}
