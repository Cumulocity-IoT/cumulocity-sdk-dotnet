
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Services;

using NotificationExample.Models;
using NotificationExample.Services.DataFeedHandlers;

using OneOf;
using OneOf.Types;

namespace NotificationExample.Services;

public class NotificationManagerService : INotificationManagerService
{
  private readonly ManagedObjectDataFeedHandler _managedObjectDataFeedHandler;
  private readonly AlarmDataFeedHandler _alarmDataFeedHandler;
  private readonly EventDataFeedHandler _eventDataFeedHandler;
  private readonly ObjectDataFeedHandler _objectDataFeedHandler;
  private readonly INotificationService _notificationService;

  public NotificationManagerService(ManagedObjectDataFeedHandler managedObjectDataFeedHandler, AlarmDataFeedHandler alarmDataFeedHandler, EventDataFeedHandler eventDataFeedHandler, ObjectDataFeedHandler objectDataFeedHandler, INotificationService notificationService)
  {
    _managedObjectDataFeedHandler = managedObjectDataFeedHandler;
    _alarmDataFeedHandler = alarmDataFeedHandler;
    _eventDataFeedHandler = eventDataFeedHandler;
    _objectDataFeedHandler = objectDataFeedHandler;
    _notificationService = notificationService;
  }

  public async Task<OneOf<ApiRegisterNotification, Error<string>>> CreateApiSubscription(string tenantId, ApiNotificationCreateInput input, CancellationToken cancellationToken )
  {
    var apiType = (ApiType)Enum.Parse(typeof(ApiType), input.ApiType);

    // this example does not really do different things in the different implementations of IDataFeedHandler, this is just to demonstrate that it could
    IDataFeedHandler? dataFeedHandler = null;
    switch(apiType) {
      case ApiType.ManagedObjects: 
        dataFeedHandler = _managedObjectDataFeedHandler;
        break;
      case ApiType.Alarms:
        dataFeedHandler = _alarmDataFeedHandler;
        break;
      case ApiType.Events:
        dataFeedHandler = _eventDataFeedHandler;
        break;
    }
    if (dataFeedHandler == null) 
    {
      return new Error<string>("no datafeed handler found");
    }

    var registerNotification = ApiRegisterNotification.TryCreate(input.SubscriptionName, apiType, input.Type, input.FragmentsToCopy, input.NonPersistent);
    if (registerNotification.IsT1) 
      return OneOf<ApiRegisterNotification, Error<string>>.FromT1(registerNotification.AsT1);

    var withHandlerRegisterNotification = new WithHandlerRegisterNotification(registerNotification.AsT0, dataFeedHandler);

    var registerResult = await _notificationService.Register(tenantId, withHandlerRegisterNotification, cancellationToken);
    return registerResult.IsT0 ? registerNotification.AsT0 : (OneOf<ApiRegisterNotification, Error<string>>)new Error<string>("permission issue");
  }

  public async Task<OneOf<ObjectRegisterNotification, Error<string>>> CreateObjectSubscription(string tenantId, ObjectNotificationCreateInput input, CancellationToken cancellationToken)
  {
    var objectNotification = ObjectRegisterNotification.TryCreate(input.SubscriptionName, input.Id, input.FragmentsToCopy, input.NonPersistent);
    if (objectNotification.IsT1) 
      return OneOf<ObjectRegisterNotification, Error<string>>.FromT1(objectNotification.AsT1);

    var withHandlerRegisterNotification = new WithHandlerRegisterNotification(objectNotification.AsT0, _objectDataFeedHandler);  

    var registerResult = await _notificationService.Register(tenantId, withHandlerRegisterNotification, cancellationToken);
    return registerResult.IsT0 ? objectNotification.AsT0 : (OneOf<ObjectRegisterNotification, Error<string>>)new Error<string>("permission issue");
  }


  public async Task<OneOf<Success, Error>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken)
  {
    var result = await _notificationService.DeleteSubscription(tenantId, subscriptionName, cancellationToken);
    return result.IsT0 ? (OneOf<Success, Error>)new Success() : (OneOf<Success, Error>)new Error();
  }
  public async Task<OneOf<Success, Error>> UnregisterSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken)
  {
    var result = await _notificationService.Unregister(tenantId, subscriptionName, cancellationToken);
    return result.IsT0 ? (OneOf<Success, Error>)new Success() : (OneOf<Success, Error>)new Error();
  }

}