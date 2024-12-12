using System.Numerics;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Models;

public sealed record ApiRegisterNotification : RegisterNotification, IEqualityOperators<ApiRegisterNotification, ApiRegisterNotification, bool>
{
  private ApiRegisterNotification(string subscriptionName, ApiType api, string? type, IReadOnlyCollection<string>? fragmentsToCopy, bool? nonPersistent)
    : base(subscriptionName, fragmentsToCopy, nonPersistent)
  {
    Api = api;
    Type = type;
  }

  public ApiType Api { get; }
  public string? Type { get; }

  public static OneOf<ApiRegisterNotification, Error<string>> TryCreate(string serviceName, ApiType api, string? type, IReadOnlyCollection<string>? fragmentsToCopy, bool? nonPersistent)
  {
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(serviceName))
    {
      errors.Add("serviceName is required and cannot be empty or whitespace string.");
    }
    if (api == ApiType.Events && string.IsNullOrWhiteSpace(type))
    {
      errors.Add("type is required and cannot be empty or whitespace string for Events api.");
    }

    return errors.Count > 0
    ? new Error<string>(string.Join(" ", errors))
      : new ApiRegisterNotification(serviceName, api, type, fragmentsToCopy, nonPersistent);
  }
}