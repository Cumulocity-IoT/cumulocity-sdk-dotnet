using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public abstract record RegisterNotification : IEqualityOperators<RegisterNotification, RegisterNotification, bool>
{
  protected RegisterNotification(string subscriptionName, IReadOnlyCollection<ApiType>? apiTypes = null, string? type = null, IReadOnlyCollection<string>? fragmentsToCopy = null, bool? nonPersistent = null)
    {
      SubscriptionName = subscriptionName;
      ApiTypes = apiTypes ?? Array.Empty<ApiType>();
      Type = type;
      FragmentsToCopy = fragmentsToCopy ?? new List<string>();
      NonPersistent = nonPersistent;
    }

    public string SubscriptionName { get; }
    public IReadOnlyCollection<ApiType> ApiTypes { get; }
    public string? Type { get; }
    public IReadOnlyCollection<string> FragmentsToCopy { get; }
    public bool? NonPersistent { get; }
}