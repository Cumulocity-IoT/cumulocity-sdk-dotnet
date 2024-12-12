using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Extensions.Notifications.Internal;

internal static class StringExtensions
{
  public static Error GetError(this string? message, bool transient) =>
      new(message is not null && transient, message ?? "Generic error.");
}