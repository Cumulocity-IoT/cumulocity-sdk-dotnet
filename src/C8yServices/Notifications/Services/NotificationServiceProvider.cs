using System;
using System.Collections.Concurrent;
using C8yServices.Notifications.Services.Internal;

namespace C8yServices.Notifications.Services;

/// <summary>
/// Default implementation of INotificationServiceProvider.
/// </summary>
public class NotificationServiceProvider : INotificationServiceProvider
{
  // Factory to create a NotificationService for a given tenantId
  private readonly Func<string, INotificationService> _serviceFactory;
  // Holds tenant-specific INotificationService instances
  private readonly ConcurrentDictionary<string, INotificationService> _services = new();
  private bool _disposed;

  public NotificationServiceProvider(Func<string, INotificationService> serviceFactory)
  {
    _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
  }

  /// <summary>
  /// Gets or creates a NotificationService for the specified tenant.
  /// </summary>
  public INotificationService? GetForTenant(string tenantId)
  {
    if (_disposed) return null;
    if (string.IsNullOrWhiteSpace(tenantId)) return null;
    return _services.GetOrAdd(tenantId, _serviceFactory);
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (_disposed) return;
    if (disposing)
    {
      foreach (var service in _services.Values)
      {
        service.DisposeAsync().AsTask().GetAwaiter().GetResult();
      }
      _services.Clear();
    }
    _disposed = true;
  }
}
