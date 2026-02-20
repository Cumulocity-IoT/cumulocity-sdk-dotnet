using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Model;
using System.Globalization;

namespace C8yServices.Extensions.Notifications.Internal;

internal static class SubscriptionExtensions
{
  public static NotificationSubscription GetNotificationSubscription(this Subscription subscription)
  {
    if (subscription is ObjectSubscription objectSubscription)
    {
      return new NotificationSubscription(NotificationSubscription.Context.MO, objectSubscription.Name)
      {
        PSource = new NotificationSubscription.Source { Id = objectSubscription.Id },
        FragmentsToCopy = [.. subscription.FragmentsToCopy],
        NonPersistent = subscription.NonPersistent
      };
    }
    var tenantSubscription = (TenantSubscription)subscription;

    return new NotificationSubscription(NotificationSubscription.Context.TENANT, tenantSubscription.Name)
    {
      PSubscriptionFilter = new NotificationSubscription.SubscriptionFilter
      {
        Apis = [.. tenantSubscription.ApiTypes.Select(apiType => apiType.ToString().ToLower(CultureInfo.InvariantCulture))],
        TypeFilter = tenantSubscription.Type
      },
      FragmentsToCopy = [.. subscription.FragmentsToCopy],
      NonPersistent = subscription.NonPersistent
    };
  }
}