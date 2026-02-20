namespace C8yServices.Subscriptions;

using System;

public interface IServiceCredentialsFactory : IDisposable
{
  Task InitOrRefresh(CancellationToken token = default);
  event EventHandler<ServiceCredentials>? ApiCredentialsUpdated;
  event EventHandler<string>? SubscriptionAdded;
  event EventHandler<string>? SubscriptionRemoved;
}