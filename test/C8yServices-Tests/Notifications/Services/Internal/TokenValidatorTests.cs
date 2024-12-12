using C8yServices.Notifications.Models;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

using Moq;

namespace C8yServices.Notifications.Services.Internal;

public class TokenValidatorTests
{
  private readonly TokenValidator _tokenValidator;
  private readonly FakeTimeProvider _timeProvider = new();
  private const string Token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjlBNkIyOTEwRDYyQ0UwMzc5RjNCREI1MjBBNTc1RTIyIiwidHlwIjoiYXQrand0In0.eyJpc3MiOiJodHRwczovL2RlbW8uZHVlbmRlc29mdHdhcmUuY29tIiwibmJmIjoxNzAwNjU0MjQ0LCJpYXQiOjE3MDA2NTQyNDQsImV4cCI6MTcwMDY1Nzg0NCwiYXVkIjoiYXBpIiwic2NvcGUiOlsiYXBpIl0sImNsaWVudF9pZCI6Im0ybSIsImp0aSI6IkUwRjYyQTA1MjM1RDY4MzdFNjcwNzExRTY1NDFDMjlEIn0.pFpzXr3jMKBYpB1JC6bq04xMyJ5gsWCq45TjO2SH44Ai6Fnic-5hn_IAYGaRPTu8dqwZjH0aBZd2UEmpvQs3WgWUANHbqG7fOAmaQQ7z3T8RRBWj5kpD6tlXmsACJAVZZl4Yra8cvrf0gacC9UHQtX9WRF51y7NeG2ZWIPq5OB4jzKvObDmcoujQgjaRRX-j7pMcpAWGxG_VMjVR9kqeOlhsfOeg3K8STIsZ46XvPhTD5CtK9j5HyYWCpadFGlpmskT8E_lBwrCGdJQae8EEO7-NllRLLaTTqz52KsT-Mhyolu7MMZy1lm58NXhv4g5_rzLbKelTWnbsBJzu-phdkg";

  public TokenValidatorTests()
  {
    var mock = new Mock<IOptions<NotificationServiceConfiguration>>();
    mock.Setup(options => options.Value).Returns(new NotificationServiceConfiguration
    {
      TokenExpirationOffset = TimeSpan.FromSeconds(30),
      BaseUrl = new Uri("wss://localhost")
    });
    _tokenValidator = new TokenValidator(_timeProvider, mock.Object);
  }

  [Theory]
  [InlineData(0, true)]
  [InlineData(30, true)]
  [InlineData(29, true)]
  [InlineData(31, true)]
  [InlineData(-30, false)]
  [InlineData(-29, true)]
  [InlineData(-31, false)]
  public void IsExpired(int secondOffset, bool expectedResult)
  {
    var baseDate = new DateTime(2023, 11, 22, 12, 57, 24, DateTimeKind.Utc);
    var dateWithOffset = baseDate.Add(TimeSpan.FromSeconds(secondOffset));
    _timeProvider.SetUtcNow(dateWithOffset);
    var result = _tokenValidator.IsExpired(Token);
    Assert.Equal(expectedResult, result);
  }
}