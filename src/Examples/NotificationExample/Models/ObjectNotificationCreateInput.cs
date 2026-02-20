namespace NotificationExample.Models;

public sealed class ObjectNotificationCreateInput() : NotificationCreateInput
{
  public required string Id { get; set; }
  public string? ApiType { get; set; }
  public string? Type { get; set; }
}