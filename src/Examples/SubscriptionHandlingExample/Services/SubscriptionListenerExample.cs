using System.Threading;
using System.Threading.Tasks;

using C8yServices.Bootstrapping;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace SubscriptionHandlingExample.Services;
public sealed class SubscriptionListenerExample : BackgroundService
{
  private readonly ILogger<SubscriptionListenerExample> _logger;
  private readonly ICumulocityCoreLibraryProvider _cumulocityApiProvider;
  private readonly ISubscriptionEventService _subscriptionEventService;

  public SubscriptionListenerExample(ILogger<SubscriptionListenerExample> logger, ICumulocityCoreLibraryProvider cumulocityApiProvider, ISubscriptionEventService subscriptionEventService)
  {
    _logger = logger;
    _cumulocityApiProvider = cumulocityApiProvider;
    _subscriptionEventService = subscriptionEventService;
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _cumulocityApiProvider.SubscriptionAddedEventHandler += OnSubscriptionAdded;
    _cumulocityApiProvider.SubscriptionRemovedEventHandler += OnSubscriptionRemoved;
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
