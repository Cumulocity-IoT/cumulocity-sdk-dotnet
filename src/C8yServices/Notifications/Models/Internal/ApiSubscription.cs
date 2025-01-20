using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed record ApiSubscription : Subscription, IEqualityOperators<ApiSubscription, ApiSubscription, bool>
{
  public ApiSubscription(string subscriptionName, ApiType api, string? type, IReadOnlyCollection<string>? fragmentsToCopy = null, bool? nonPersistent = null)
    : base(subscriptionName, fragmentsToCopy, nonPersistent)
  {
    Api = api;
    Type = type;
  }
  public ApiType Api { get; }
  public string? Type { get; }
}