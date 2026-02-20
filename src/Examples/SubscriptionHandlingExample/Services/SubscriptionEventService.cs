using C8yServices.Configuration;
using C8yServices.Inventory;
using C8yServices.RestApi;

using Client.Com.Cumulocity.Client.Api;
using Client.Com.Cumulocity.Client.Model;
using Client.Com.Cumulocity.Client.Supplementary;

using Microsoft.Extensions.Options;

using SubscriptionHandlingExample.Models;

namespace SubscriptionHandlingExample.Services;
public sealed class SubscriptionEventService : ISubscriptionEventService
{
  private const string CustomEventSourceTypePrefix = "c8yExample_EventTarget_";
  private readonly ILogger<SubscriptionEventService> _logger;
  private readonly ICumulocityCoreLibraryProvider _cumulocityApiProvider;
  private readonly ICurrentApplicationApi _currentApplicationApi;
  private readonly string _bootstrapTenant;
  private string? _applicationId;

  public SubscriptionEventService(ILogger<SubscriptionEventService> logger, ICumulocityCoreLibraryProvider cumulocityApiProvider, ICurrentApplicationApi currentApplicationApi, IOptions<C8YConfiguration> c8yConfiguration)
  {
    _logger = logger;
    _cumulocityApiProvider = cumulocityApiProvider;
    _currentApplicationApi = currentApplicationApi;
    _bootstrapTenant = c8yConfiguration.Value.BootstrapTenant!;
  }

  public async Task<SubscriptionAddedEvent?> CreateSubscriptionAddedEvent(string tenant, CancellationToken cancellationToken)
  {
    var subscriptionAddedEvent = new SubscriptionAddedEvent(tenant);
    return await CreateEvent(subscriptionAddedEvent, tenant, cancellationToken);
  }

  public async Task<SubscriptionRemovedEvent?> CreateSubscriptionRemovedEvent(string tenant, CancellationToken cancellationToken)
  {
    var subscriptionRemovedEvent = new SubscriptionRemovedEvent(tenant);
    return await CreateEvent(subscriptionRemovedEvent, tenant, cancellationToken);
  }

  public async Task<IReadOnlyList<SubscriptionAddedEvent>?> GetAllSubscriptionAddedEvents(CancellationToken cancellationToken)
  {
    var cumulocityCoreLibrary = _cumulocityApiProvider.GetForTenant(_bootstrapTenant);

    return cumulocityCoreLibrary == null
      ? new List<SubscriptionAddedEvent>()
      : await GetAllEventsByType<SubscriptionAddedEvent>(SubscriptionAddedEvent.TypeName, cumulocityCoreLibrary, cancellationToken);
  }

  public async Task<IReadOnlyList<SubscriptionRemovedEvent>?> GetAllSubscriptionRemovedEvents(CancellationToken cancellationToken)
  {
    var cumulocityCoreLibrary = _cumulocityApiProvider.GetForTenant(_bootstrapTenant);

    return cumulocityCoreLibrary == null
      ? new List<SubscriptionRemovedEvent>()
      : await GetAllEventsByType<SubscriptionRemovedEvent>(SubscriptionRemovedEvent.TypeName, cumulocityCoreLibrary, cancellationToken);
  }

  public async Task<bool> RemoveAllSubscriptionAddedEvents(CancellationToken cancellationToken)
  {
    var cumulocityApi = _cumulocityApiProvider.GetForTenant(_bootstrapTenant);

    return (cumulocityApi != null) && await RemoveAllEventsByType(cumulocityApi, SubscriptionAddedEvent.TypeName, cancellationToken);
  }

  public async Task<bool> RemoveAllSubscriptionRemovedEvents(CancellationToken cancellationToken)
  {
    var cumulocityApi = _cumulocityApiProvider.GetForTenant(_bootstrapTenant);

    return cumulocityApi != null && await RemoveAllEventsByType(cumulocityApi, SubscriptionRemovedEvent.TypeName, cancellationToken);
  }

  private async Task<T?> CreateEvent<T>(T @event, string tenant, CancellationToken cancellationToken) where T : Event
  {
    var cumulocityApi = _cumulocityApiProvider.GetForTenant(_bootstrapTenant);

    if (cumulocityApi == null)
    {
      return null;
    }

    _applicationId ??= await InitializeEventTarget(cumulocityApi, cancellationToken);

    if (_applicationId == null)
    {
      return null;
    }

    try
    {
      @event.Time = DateTime.UtcNow;
      @event.PSource = new Event.Source() { Id = _applicationId };
      var result = await cumulocityApi.Events.EventsApi.CreateEvent(@event, cToken: cancellationToken);
      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception while trying to create event of type {Type} for tenant {Tenant}", @event.Type, tenant);
      return null;
    }
  }

  private async Task<IReadOnlyList<T>?> GetAllEventsByType<T>(string eventType, ICumulocityCoreLibrary cumulocityCoreLibrary, CancellationToken cancellationToken) where T : Event
  {
    int currentPage = 0;
    int pageSize = 100;
    int? count;
    var events = new List<T>();
    do
    {
      try
      {
        var result = await cumulocityCoreLibrary.Events.EventsApi.GetEvents<T>(type: eventType, currentPage: currentPage, pageSize: pageSize, cToken: cancellationToken);
        count = result?.Events.Count;
        if (result is not null) events.AddRange(result.Events);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Exception while trying to fetchs event of type {Type} on page {CurrentPage} with pageSize {PageSize}", eventType, currentPage, pageSize);
        return null;
      }

    } while (count == pageSize);
    return events;
  }

  private async Task<bool> RemoveAllEventsByType(ICumulocityCoreLibrary cumulocityCoreLibrary, string type, CancellationToken cancellationToken)
  {
    try
    {
      await cumulocityCoreLibrary.Events.EventsApi.DeleteEvents(type: type, cToken: cancellationToken);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception while trying to delete event of type {Type}", type);
      return false;
    }
  }

  private async Task<string?> InitializeEventTarget(ICumulocityCoreLibrary cumulocityCoreLibrary, CancellationToken cancellationToken)
  {
    var applicationId = (await _currentApplicationApi.GetCurrentApplication(cancellationToken))?.Id;
    if (applicationId is not null)
    {
      var result = await InventoryApiHelper.RequestFirstByQueryAsync<ManagedObject>(cumulocityCoreLibrary, $"type eq 'c8y_Application_{applicationId}'", token: cancellationToken);
      if (result.IsT0)
      {
        return result.AsT0.Id;
      }
      // if the microservice is not deployed and only running locally/externally, there is no managed object for the application
      // in this case we should create a dedicated one
      return await CreateEventTarget(cumulocityCoreLibrary, applicationId, cancellationToken);
    }
    return null;
  }

  private async Task<string?> CreateEventTarget(ICumulocityCoreLibrary cumulocityCoreLibrary, string applicationId, CancellationToken cancellationToken)
  {
    var type = CustomEventSourceTypePrefix + applicationId;
    try 
    {
      var managedObject = new ManagedObject() 
      {
        Type = type
      };
      return (await cumulocityCoreLibrary.Inventory.ManagedObjectsApi.CreateManagedObject(managedObject, cToken: cancellationToken))?.Id;
    }
    catch (Exception ex) 
    {
      _logger.LogError(ex, "Exception while trying to create event target source of type {Type}", type);
      return null;
    }
  }
}
