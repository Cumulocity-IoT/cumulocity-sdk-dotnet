namespace C8yServices.Notifications.Services.Internal;

internal interface ITokenValidator
{
  bool IsExpired(string tokenString);
}