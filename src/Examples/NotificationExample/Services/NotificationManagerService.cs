
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Services;

using NotificationExample.Models;

using OneOf;
using OneOf.Types;

namespace NotificationExample.Services;

public class NotificationManagerService : INotificationManagerService
{
  private readonly DataFeedHandler _objectDataFeedHandler;
  private readonly INotificationServiceProvider _notificationServiceProvider;

  public NotificationManagerService(DataFeedHandler objectDataFeedHandler, INotificationServiceProvider notificationServiceProvider)
  {
    _objectDataFeedHandler = objectDataFeedHandler;
    _notificationServiceProvider = notificationServiceProvider;
  }

  public async Task<OneOf<TenantRegisterNotification, Error<string>>> CreateTenantSubscription(string tenantId, TenantNotificationCreateInput input, CancellationToken cancellationToken )
  {
    // this example does not really do different things in the different implementations of IDataFeedHandler, this is just to demonstrate that it could
    IDataFeedHandler? dataFeedHandler = _objectDataFeedHandler;

    IReadOnlyCollection<ApiType>? apiTypes = null;
    if (input.ApiType != null)
    {
      var parsed = new List<ApiType>();
      foreach (var apiTypeStr in input.ApiType)
      {
        if (Enum.TryParse<ApiType>(apiTypeStr, out var parsedApiType))
        {
          parsed.Add(parsedApiType);
        }
      }
      apiTypes = parsed;
    }
    var registerNotification = TenantRegisterNotification.TryCreate(input.SubscriptionName, apiTypes, input.Type, input.FragmentsToCopy, input.NonPersistent);
    if (registerNotification.IsT1) 
      return OneOf<TenantRegisterNotification, Error<string>>.FromT1(registerNotification.AsT1);

    var withHandlerRegisterNotification = new WithHandlerRegisterNotification(registerNotification.AsT0, dataFeedHandler);

    var notificationService = _notificationServiceProvider.GetForTenant(tenantId);
    var registerResult = await notificationService.Register(withHandlerRegisterNotification, cancellationToken);
    return registerResult.IsT0 ? registerNotification.AsT0 : (OneOf<TenantRegisterNotification, Error<string>>)new Error<string>("permission issue");
  }

  public async Task<OneOf<ObjectRegisterNotification, Error<string>>> CreateObjectSubscription(string tenantId, ObjectNotificationCreateInput input, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<ApiType>? objectApiTypes = null;
    if (!string.IsNullOrWhiteSpace(input.ApiType) && Enum.TryParse<ApiType>(input.ApiType, out var parsedObjectApiType))
    {
      objectApiTypes = new[] { parsedObjectApiType };
    }
    var objectNotification = ObjectRegisterNotification.TryCreate(input.SubscriptionName, input.Id, objectApiTypes, input.Type, input.FragmentsToCopy, input.NonPersistent);
    if (objectNotification.IsT1) 
      return OneOf<ObjectRegisterNotification, Error<string>>.FromT1(objectNotification.AsT1);

    var withHandlerRegisterNotification = new WithHandlerRegisterNotification(objectNotification.AsT0, _objectDataFeedHandler);  

    var notificationService = _notificationServiceProvider.GetForTenant(tenantId);
    var registerResult = await notificationService.Register(withHandlerRegisterNotification, cancellationToken);
    return registerResult.IsT0 ? objectNotification.AsT0 : (OneOf<ObjectRegisterNotification, Error<string>>)new Error<string>("permission issue");
  }

  public async Task<OneOf<Success, Error>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken)
  {
    var notificationService = _notificationServiceProvider.GetForTenant(tenantId);
    var result = await notificationService.DeleteSubscription(subscriptionName, cancellationToken);
    return result.IsT0 ? (OneOf<Success, Error>)new Success() : (OneOf<Success, Error>)new Error();
  }
  
  public async Task<OneOf<Success, Error>> UnregisterSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken)
  {
    var notificationService = _notificationServiceProvider.GetForTenant(tenantId);
    var result = await notificationService.Unregister(subscriptionName, cancellationToken);
    return result.IsT0 ? (OneOf<Success, Error>)new Success() : (OneOf<Success, Error>)new Error();
  }

}