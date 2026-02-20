namespace NotificationExample.Models;

public sealed class TenantNotificationCreateInput() : NotificationCreateInput
{
  public IReadOnlyList<string>? ApiType { get; set; }
  public string? Type { get; set; }
}