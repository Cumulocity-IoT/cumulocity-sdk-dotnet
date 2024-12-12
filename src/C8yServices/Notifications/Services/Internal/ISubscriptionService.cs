using C8yServices.Common.Models;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal interface ISubscriptionService
{
  Task<OneOf<string, ApiError>> Create(ISubscriptionsApi subscriptionsApi, Subscription subscription, CancellationToken token = default);
  Task<OneOf<string?, ApiError>> Get(ISubscriptionsApi subscriptionsApi, string name, CancellationToken token = default);
  Task<OneOf<Success, NotFound, ApiError>> Delete(ISubscriptionsApi subscriptionsApi, string name, CancellationToken token = default);
}