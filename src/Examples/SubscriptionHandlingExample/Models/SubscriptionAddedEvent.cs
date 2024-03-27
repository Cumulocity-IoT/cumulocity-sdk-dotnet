using Client.Com.Cumulocity.Client.Model;

namespace SubscriptionHandlingExample.Models;
public class SubscriptionAddedEvent : Event
{
  public const string TypeName = "example_subscription_added_event";

  public SubscriptionAddedEvent(string tenant)
  {
    Type = TypeName;
    Text = $"Microservice subscribed on tenant {tenant}";
  }

  // empty constructor for deserialization
  public SubscriptionAddedEvent() {

  }

}
