using System.Numerics;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Models;

public sealed record ObjectRegisterNotification : RegisterNotification, IEqualityOperators<ObjectRegisterNotification, ObjectRegisterNotification, bool>
{
  private ObjectRegisterNotification(string subscriptionName, string id, IReadOnlyCollection<ApiType>? apiTypes, string? type, IReadOnlyCollection<string>? fragmentsToCopy, bool? nonPersistent)
    : base(subscriptionName, apiTypes, type, fragmentsToCopy, nonPersistent)
  {
    Id = id;
  }

  public string Id { get; }

  public static OneOf<ObjectRegisterNotification, Error<string>> TryCreate(string serviceName, string id, IReadOnlyCollection<ApiType>? apiTypes, string? type, IReadOnlyCollection<string>? fragmentsToCopy, bool? nonPersistent)
  {
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(serviceName))
    {
      errors.Add("serviceName is required and cannot be empty or whitespace string.");
    }
    if (string.IsNullOrWhiteSpace(id))
    {
      errors.Add("id is required and cannot be empty or whitespace string.");
    }

    return errors.Count > 0
      ? new Error<string>(string.Join(" ", errors))
      : new ObjectRegisterNotification(serviceName, id, apiTypes, type, fragmentsToCopy, nonPersistent);
  }
}