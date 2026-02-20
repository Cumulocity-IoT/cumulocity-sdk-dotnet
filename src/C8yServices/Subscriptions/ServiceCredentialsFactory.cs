using C8yServices.Utils;

using Microsoft.Extensions.DependencyInjection;

namespace C8yServices.Subscriptions;

public sealed class ServiceCredentialsFactory : IServiceCredentialsFactory
{
  private readonly Locker _locker = new();
  private readonly IServiceCredentialsFactoryHelper _helper;
  private readonly IServiceProvider _serviceProvider;
  private HashSet<string> _currentlySubscribedTenants = new();
  public event EventHandler<ServiceCredentials>? ApiCredentialsUpdated;
  public event EventHandler<string>? SubscriptionAdded;
  public event EventHandler<string>? SubscriptionRemoved;

  public ServiceCredentialsFactory(IServiceCredentialsFactoryHelper helper, IServiceProvider serviceProvider)
  {
    _helper = helper;
    _serviceProvider = serviceProvider;
  }

  public async Task InitOrRefresh(CancellationToken token = default)
  {
    // Automatically initialize all credential-aware services before firing events
    foreach (var credentialAwareService in _serviceProvider.GetServices<ICredentialAwareService>())
    {
      // Simply resolving the service ensures it's created and subscribed to events
      _ = credentialAwareService;
    }

    var apiCredentials = await _helper.GetApiCredentials(token).ConfigureAwait(false);

    // Track tenant changes for subscription events
    var currentTenants = new HashSet<string>(apiCredentials.Select(c => c.Tenant));
    var addedTenants = currentTenants.Except(_currentlySubscribedTenants).ToList();
    var removedTenants = _currentlySubscribedTenants.Except(currentTenants).ToList();

    foreach (var tenant in addedTenants)
    {
      // Only fire SubscriptionAdded if tenant is not already present
      if (!_currentlySubscribedTenants.Contains(tenant))
      {
        InvokeEventHandlersSafe(SubscriptionAdded, tenant, nameof(SubscriptionAdded));
      }
    }
    foreach (var tenant in removedTenants)
      InvokeEventHandlersSafe(SubscriptionRemoved, tenant, nameof(SubscriptionRemoved));

    _currentlySubscribedTenants = currentTenants;

    // Notify all subscribers about the new credentials
    NotifyApiCredentialsUpdated(apiCredentials);
  }

  /// <summary>
  /// Event triggered when API credentials are updated.
  /// Subscribers are notified asynchronously and exceptions in one handler do not prevent others from being called.
  /// </summary>

  /// <summary>
  /// Notifies all subscribers about updated API credentials.
  /// Each subscriber is called in a fire-and-forget async Task, exceptions are caught and logged.
  /// </summary>
  /// <param name="credentials">The updated API credentials.</param>
  private void NotifyApiCredentialsUpdated(IEnumerable<ServiceCredentials> credentials)
  {
    foreach (var cred in credentials)
    {
      InvokeEventHandlersSafe(ApiCredentialsUpdated, cred, nameof(ApiCredentialsUpdated));
    }
  }

  /// <summary>
  /// Invokes all handlers for the given event in an exception-safe way (generic).
  /// </summary>
  private static void InvokeEventHandlersSafe<T>(EventHandler<T>? eventHandler, T arg, string eventName)
  {
    var handlers = eventHandler?.GetInvocationList();
    if (handlers == null) return;
    foreach (var handler in handlers)
    {
      try
      {
        ((EventHandler<T>)handler).Invoke(null, arg);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"{eventName} exception: {ex}");
      }
    }
  }

  public void Dispose() =>
    _locker.Dispose();
}