using C8yServices.Notifications.Models.Internal;

namespace C8yServices.Notifications.Services.Internal;

internal interface IMessageExtractor
{
  MessageData GetMessageData(string source);
}