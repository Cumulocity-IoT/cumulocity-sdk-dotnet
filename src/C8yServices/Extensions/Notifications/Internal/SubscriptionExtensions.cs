using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Model;

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
    var apiSubscription = (ApiSubscription)subscription;

    return new NotificationSubscription(NotificationSubscription.Context.TENANT, apiSubscription.Name)
    {
      PSubscriptionFilter = new NotificationSubscription.SubscriptionFilter
      {
        Apis = [apiSubscription.Api.ToString().ToLowerInvariant()],
        TypeFilter = apiSubscription.Type
      },
      FragmentsToCopy = [.. subscription.FragmentsToCopy],
      NonPersistent = subscription.NonPersistent
    };
  }
}