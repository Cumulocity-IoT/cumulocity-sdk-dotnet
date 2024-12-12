using C8yServices.Common.Models;
using C8yServices.Extensions.Notifications.Internal;
using C8yServices.Notifications.Models.Internal;

using Client.Com.Cumulocity.Client.Api;

using OneOf;
using OneOf.Types;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class SubscriptionService : ISubscriptionService
{
  public Task<OneOf<string, ApiError>> Create(ISubscriptionsApi subscriptionsApi, Subscription subscription, CancellationToken token = default) =>
    FlowWrappers.HandleOneCallWithHttpRequestException((subscription, subscriptionsApi), static (p, cancellationToken) => CreateInt(p.subscriptionsApi, p.subscription, cancellationToken), token);

  public Task<OneOf<string?, ApiError>> Get(ISubscriptionsApi subscriptionsApi, string name, CancellationToken token = default) =>
    FlowWrappers.HandleOneCallWithHttpRequestException((name, subscriptionsApi), static (p, cancellationToken) => GetInt(p.subscriptionsApi, p.name, cancellationToken), token);

  public Task<OneOf<Success, NotFound, ApiError>> Delete(ISubscriptionsApi subscriptionsApi, string name, CancellationToken token = default) =>
    FlowWrappers.HandleOneCallWithHttpRequestException((name, subscriptionsApi), static (p, cancellationToken) => DeleteInt(p.subscriptionsApi, p.name, cancellationToken), token);

  private static async Task<OneOf<string, ApiError>> CreateInt(ISubscriptionsApi subscriptionsApi, Subscription subscription, CancellationToken token = default)
  {
    var request = subscription.GetNotificationSubscription();
    var result = await subscriptionsApi.CreateSubscription(request, cToken: token).ConfigureAwait(false);
    var id = result?.Id;

    return id is null ? Constants.NullResultApiError : id;
  }

  private static async Task<OneOf<string?, ApiError>> GetInt(ISubscriptionsApi subscriptionsApi, string name, CancellationToken token = default)
  {
    var result = await subscriptionsApi.GetSubscriptions(subscription: name, cToken: token).ConfigureAwait(false);

    return result is null ? Constants.NullResultApiError : result.Subscriptions.FirstOrDefault()?.Id;
  }

  private static async Task<OneOf<Success, NotFound, ApiError>> DeleteInt(ISubscriptionsApi subscriptionsApi, string name, CancellationToken token = default)
  {
    var getResult = await GetInt(subscriptionsApi, name, token).ConfigureAwait(false);
    if (getResult.IsT1)
    {
      return getResult.AsT1;
    }
    var id = getResult.AsT0;
    if (id is null)
    {
      return new NotFound();
    }
    await subscriptionsApi.DeleteSubscription(id, cToken: token).ConfigureAwait(false);

    return new Success();
  }
}