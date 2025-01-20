
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Services;

namespace NotificationExample.Services.DataFeedHandlers;

public class ObjectDataFeedHandler : IDataFeedHandler
{
  private readonly ILogger<ObjectDataFeedHandler> _logger;

  public ObjectDataFeedHandler(ILogger<ObjectDataFeedHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(MessageObject messageObject, CancellationToken token = default)
  {
    _logger.LogInformation("Received message: {Message}, api: {Api}, action: {Action}", messageObject.Message, messageObject.Api, messageObject.Action);
    return Task.CompletedTask;    
  }
}