using C8yServices.Notifications.Models;
using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Extensions.Notifications.Internal;

internal static class RegisterNotificationExtensions
{
  public static Subscription ToSubscription(this RegisterNotification registerNotification)
  {
    if (registerNotification is ObjectRegisterNotification objectRegisterNotification)
    {
      return new ObjectSubscription(
        objectRegisterNotification.SubscriptionName,
        objectRegisterNotification.Id,
        objectRegisterNotification.ApiTypes,
        objectRegisterNotification.Type,
        objectRegisterNotification.FragmentsToCopy,
        objectRegisterNotification.NonPersistent
      );
    }
    var apiRegisterNotification = (TenantRegisterNotification)registerNotification;

    return new TenantSubscription(
      apiRegisterNotification.SubscriptionName,
      apiRegisterNotification.ApiTypes,
      apiRegisterNotification.Type,
      apiRegisterNotification.FragmentsToCopy,
      apiRegisterNotification.NonPersistent
    );
  }
}