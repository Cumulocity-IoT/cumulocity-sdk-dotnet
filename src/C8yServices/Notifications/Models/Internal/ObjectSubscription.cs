using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed record ObjectSubscription : Subscription, IEqualityOperators<ObjectSubscription, ObjectSubscription, bool>
{
  public ObjectSubscription(string subscriptionName, string id, IReadOnlyCollection<string>? fragmentsToCopy = null, bool? nonPersistent = null) 
    : base(subscriptionName, fragmentsToCopy, nonPersistent) =>
    Id = id;

  public string Id { get; }
}