using Client.Com.Cumulocity.Client.Model;

namespace SubscriptionHandlingExample.Models;
public class SubscriptionRemovedEvent : Event
{
  public const string TypeName = "example_subscription_removed_event";

  public SubscriptionRemovedEvent(string tenant)
  {
    Type = TypeName;
    Text = $"Microservice unsubscribed on tenant {tenant}";
  }

  // empty constructor for deserialization
  public SubscriptionRemovedEvent() {

  }
}
