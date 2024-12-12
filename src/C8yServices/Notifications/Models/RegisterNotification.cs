using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public abstract record RegisterNotification : IEqualityOperators<RegisterNotification, RegisterNotification, bool>
{
  protected RegisterNotification(string subscriptionName, IReadOnlyCollection<string>? fragmentsToCopy = null, bool? nonPersistent = null)
  {
    SubscriptionName = subscriptionName;
    FragmentsToCopy = fragmentsToCopy ?? new List<string>();
    NonPersistent = nonPersistent; 
  }

  public string SubscriptionName { get; }
  public IReadOnlyCollection<string> FragmentsToCopy { get; }
  public bool? NonPersistent { get; }
}