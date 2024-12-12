using C8yServices.Notifications.Models;

namespace C8yServices.Notifications.Services;

public interface INotificationServiceConfigurationProvider
{
  ValueTask<NotificationServiceConfiguration> Get(CancellationToken token);
}