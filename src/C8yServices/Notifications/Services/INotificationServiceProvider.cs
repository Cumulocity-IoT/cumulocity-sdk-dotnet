namespace C8yServices.Notifications.Services;

/// <summary>
/// Provides tenant-specific NotificationService instances.
/// </summary>
public interface INotificationServiceProvider : IDisposable
{
  /// <summary>
  /// Gets a NotificationService instance for the specified tenant.
  /// </summary>
  INotificationService GetForTenant(string tenantId);
}
