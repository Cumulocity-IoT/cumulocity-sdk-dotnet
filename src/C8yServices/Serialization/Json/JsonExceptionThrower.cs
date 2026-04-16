using System.Text.Json;


namespace C8yServices.Serialization.Json;

public static class JsonExceptionThrower
{
  public static void ThrowIfNull<T>(T? value, string? message = null)
  {
    if (value is null)
    {
      throw new JsonException(GetExceptionMessage(message, "unexpected null value"));
    }
  }

  public static void ThrowIfNullOrWhitespace(string? value, string? message = null)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new JsonException(GetExceptionMessage(message, "string value is null or whitespace"));
    }
  }

  public static void ThrowIfFalse(bool condition, string? message = null)
  {
    if (!condition)
    {
      throw new JsonException(GetExceptionMessage(message, "condition is not fulfilled"));
    }
  }

  public static void ThrowIfNotAssignableTo<T>(Type instanceType)
  {
    if (!instanceType.IsAssignableTo(typeof(T)))
    {
      throw new JsonException("Instance type " + instanceType.Name + " is not assignable to type " + typeof(T).Name);
    }
  }

  private static string GetExceptionMessage(string? message, string defaultMessage)
  {
    if (string.IsNullOrWhiteSpace(message))
    {
      return defaultMessage;
    }

    return message;
  }
}