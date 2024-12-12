using System.Diagnostics.CodeAnalysis;

using C8yServices.Notifications.Models;

using Microsoft.Extensions.Options;

namespace C8yServices.Notifications.Services.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed class StaticNotificationServiceConfigurationProvider : INotificationServiceConfigurationProvider
{
  private readonly IOptions<NotificationServiceConfiguration> _options;

  public StaticNotificationServiceConfigurationProvider(IOptions<NotificationServiceConfiguration> options) => 
    _options = options;

  public ValueTask<NotificationServiceConfiguration> Get(CancellationToken token) => 
    new(_options.Value);
}