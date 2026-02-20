
using C8yServices.Notifications.Models;
using C8yServices.Notifications.Services;


namespace NotificationExample.Services;

public class DataFeedHandler : IDataFeedHandler
{
  private readonly ILogger<DataFeedHandler> _logger;

  public DataFeedHandler(ILogger<DataFeedHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(MessageObject messageObject, CancellationToken token = default)
  {
    _logger.LogInformation("Received message: {Message}, api: {Api}, action: {Action}", messageObject.Message, messageObject.Api, messageObject.Action);
    return Task.CompletedTask;    
  }
}