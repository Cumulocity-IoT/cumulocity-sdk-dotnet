using C8yServices.Notifications.Models;

using NotificationExample.Models;

using OneOf;
using OneOf.Types;

namespace NotificationExample.Services;

public interface INotificationManagerService
{
  Task<OneOf<TenantRegisterNotification, Error<string>>> CreateTenantSubscription(string tenantId, TenantNotificationCreateInput input, CancellationToken cancellationToken);
  Task<OneOf<ObjectRegisterNotification, Error<string>>> CreateObjectSubscription(string tenantId, ObjectNotificationCreateInput input, CancellationToken cancellationToken);
  Task<OneOf<Success, Error>> DeleteSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken);
  Task<OneOf<Success, Error>> UnregisterSubscription(string tenantId, string subscriptionName, CancellationToken cancellationToken);
}