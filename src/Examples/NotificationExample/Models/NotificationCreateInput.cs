namespace NotificationExample.Models;

public abstract class NotificationCreateInput() 
{
  public required string SubscriptionName { get; set; }
  public IReadOnlyCollection<string>? FragmentsToCopy { get; set; }
  public bool? NonPersistent { get; set; }
}