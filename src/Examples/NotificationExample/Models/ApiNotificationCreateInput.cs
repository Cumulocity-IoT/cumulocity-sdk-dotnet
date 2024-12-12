namespace NotificationExample.Models;

public sealed class ApiNotificationCreateInput() : NotificationCreateInput
{
  public required string ApiType { get; set; }
  public string? Type { get; set; }
}