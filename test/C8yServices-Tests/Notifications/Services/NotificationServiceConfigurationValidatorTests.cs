using C8yServices.Notifications.Models;

using FluentValidation.Results;

namespace C8yServices.Notifications.Services;

public class NotificationServiceConfigurationValidatorTests
{
  private const string DEFAULT_URL = "wss://localhost";
  private readonly NotificationServiceConfigurationValidator _validator = [];

  [Fact]
  public void Valid()
  {
    var result = _validator.Validate(new NotificationServiceConfiguration() { BaseUrl = new Uri(DEFAULT_URL)});
    Assert.True(result.IsValid);
  }

  [Fact]
  public void Invalid()
  {
    var result = _validator.Validate(new NotificationServiceConfiguration
    {
      NotificationEndpoint = string.Empty,
      OperationTimeout = TimeSpan.Zero,
      WebSocketClientMonitorInterval = TimeSpan.Zero,
      TokenExpirationOffset = TimeSpan.FromSeconds(-1),
      BaseUrl = new Uri(DEFAULT_URL)
    });
    Assert.False(result.IsValid);
    Assert.Equal(4, result.Errors.Count);
    Assert.Equal("'Notification Endpoint' must not be empty.", FindMessage(result.Errors, nameof(NotificationServiceConfiguration.NotificationEndpoint)));
    Assert.Equal("'Operation Timeout' must be greater than '00:00:00'.", FindMessage(result.Errors, nameof(NotificationServiceConfiguration.OperationTimeout)));
    Assert.Equal("'Web Socket Client Monitor Interval' must be greater than '00:00:00'.", FindMessage(result.Errors, nameof(NotificationServiceConfiguration.WebSocketClientMonitorInterval)));
    Assert.Equal("'Token Expiration Offset' must be greater than or equal to '00:00:00'.", FindMessage(result.Errors, nameof(NotificationServiceConfiguration.TokenExpirationOffset)));
  }

  private static string? FindMessage(IEnumerable<ValidationFailure> failures, string propertyName) =>
    failures.FirstOrDefault(failure => failure.PropertyName == propertyName)?.ErrorMessage;
}