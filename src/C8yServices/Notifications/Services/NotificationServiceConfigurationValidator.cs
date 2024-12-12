using C8yServices.Notifications.Models;

using FluentValidation;

namespace C8yServices.Notifications.Services;

public sealed class NotificationServiceConfigurationValidator : AbstractValidator<NotificationServiceConfiguration>
{
  public NotificationServiceConfigurationValidator()
  {
    RuleFor(configuration => configuration.OperationTimeout).GreaterThan(TimeSpan.Zero);
    RuleFor(configuration => configuration.WebSocketClientMonitorInterval).GreaterThan(TimeSpan.Zero);
    RuleFor(configuration => configuration.NotificationEndpoint).NotNull().NotEmpty();
    RuleFor(configuration => configuration.TokenExpirationOffset).GreaterThanOrEqualTo(TimeSpan.Zero);
  }
}