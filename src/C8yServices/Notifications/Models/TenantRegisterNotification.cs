using System.Numerics;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Models;

public sealed record TenantRegisterNotification : RegisterNotification, IEqualityOperators<TenantRegisterNotification, TenantRegisterNotification, bool>
{
  private TenantRegisterNotification(string subscriptionName, IReadOnlyCollection<ApiType>? apiTypes, string? type, IReadOnlyCollection<string>? fragmentsToCopy, bool? nonPersistent)
    : base(subscriptionName, apiTypes, type, fragmentsToCopy, nonPersistent)
  {
  }

  public static OneOf<TenantRegisterNotification, Error<string>> TryCreate(string serviceName, IReadOnlyCollection<ApiType>? apiTypes, string? type, IReadOnlyCollection<string>? fragmentsToCopy, bool? nonPersistent)
  {
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(serviceName))
    {
      errors.Add("serviceName is required and cannot be empty or whitespace string.");
    }
    if (apiTypes != null && apiTypes.Contains(ApiType.Events) && string.IsNullOrWhiteSpace(type))
    {
      errors.Add("type is required and cannot be empty or whitespace string for Events api.");
    }

    return errors.Count > 0
      ? new Error<string>(string.Join(" ", errors))
      : new TenantRegisterNotification(serviceName, apiTypes, type, fragmentsToCopy, nonPersistent);
  }
}