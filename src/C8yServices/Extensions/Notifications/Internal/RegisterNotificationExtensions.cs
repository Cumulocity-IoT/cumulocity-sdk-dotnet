using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Extensions.Notifications.Internal;

internal static class RegisterNotificationExtensions
{
  public static Subscription ToSubscription(this RegisterNotification registerNotification)
  {
    if (registerNotification is ObjectRegisterNotification objectRegisterNotification)
    {
      return new ObjectSubscription(objectRegisterNotification.SubscriptionName, objectRegisterNotification.Id, objectRegisterNotification.FragmentsToCopy, objectRegisterNotification.NonPersistent);
    }
    var apiRegisterNotification = (ApiRegisterNotification)registerNotification;

    return new ApiSubscription(apiRegisterNotification.SubscriptionName, apiRegisterNotification.Api, apiRegisterNotification.Type, apiRegisterNotification.FragmentsToCopy, apiRegisterNotification.NonPersistent);
  }
}