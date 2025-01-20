using C8yServices.Notifications.Models;

namespace C8yServices.Notifications.Services;

/// <summary>
/// The data feed handler for receiving notifications
/// </summary>
public interface IDataFeedHandler
{
  /// <summary>
  /// Handles the message object.
  /// </summary>
  /// <param name="messageObject">The message object.</param>
  /// <param name="token">The token.</param>
  /// <returns></returns>
  Task Handle(MessageObject messageObject, CancellationToken token = default);
}