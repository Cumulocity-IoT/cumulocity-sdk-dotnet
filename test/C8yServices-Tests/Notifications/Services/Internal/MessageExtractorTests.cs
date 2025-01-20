namespace C8yServices.Notifications.Services.Internal;

public class MessageExtractorTests
{
  private readonly MessageExtractor _messageExtractor = new();

  [Fact]
  public void HappyPath()
  {
    var message = $"MessageLine1{MessageExtractor.LineSeparator}MessageLine2";
    const string ack = "Ack";
    const string action = "Action";
    const string api = "Api";
    var source = $"{ack}{MessageExtractor.LineSeparator}{api}{MessageExtractor.LineSeparator}{action}{MessageExtractor.LineSeparator}{message}";
    var result = _messageExtractor.GetMessageData(source);
    Assert.Equal(ack, result.Acknowledgement);
    Assert.Equal(message, result.RawMessage);
    Assert.Equal(api, result.ApiUrl);
    Assert.Equal(action, result.Action);
  }

  [Fact]
  public void EmptyString()
  {
    var result = _messageExtractor.GetMessageData(string.Empty);
    Assert.NotNull(result);
    Assert.Equal(string.Empty, result.Acknowledgement);
    Assert.Equal(string.Empty, result.RawMessage);
    Assert.Equal(string.Empty, result.ApiUrl);
    Assert.Equal(string.Empty, result.Action);
  }
}