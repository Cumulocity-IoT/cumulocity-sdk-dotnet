using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Notifications.Services.Internal;

internal sealed class MessageExtractor : IMessageExtractor
{
  public const char LineSeparator = '\n';
  public MessageData GetMessageData(string source)
  {
    var items = source.Split(LineSeparator);

    return new MessageData(GetItem(items, 0), GetItem(items, 2), GetItem(items, 1), GetMessage(items));
  }

  private static string GetItem(IReadOnlyList<string> items, int index) => 
    items.Count > index ? items[index] : string.Empty;

  private static string GetMessage(IEnumerable<string> items) =>
    string.Join(LineSeparator, items.Skip(3));
}