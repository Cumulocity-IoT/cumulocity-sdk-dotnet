using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed record TenantSubscription : Subscription, IEqualityOperators<TenantSubscription, TenantSubscription, bool>
{
  public TenantSubscription(string subscriptionName, IReadOnlyCollection<ApiType>? apiTypes, string? type, IReadOnlyCollection<string>? fragmentsToCopy = null, bool? nonPersistent = null)
    : base(subscriptionName, apiTypes, type, fragmentsToCopy, nonPersistent)
  {
  }
}