using System.Numerics;

namespace C8yServices.Notifications.Models;

/// <summary>
/// The api metadata
/// </summary>
public sealed record Api : IEqualityOperators<Api, Api, bool>
{
  public const char Separator = '/';
  /// <summary>
  /// Initializes a new instance of the <see cref="Api"/> class.
  /// </summary>
  /// <param name="text">The text.</param>
  public Api(string text)
  {
    Text = text;
    var items = text.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
    Tenant = items.Length > 0 ? items[0] : null;
    ApiType = items.Length > 1 && Enum.TryParse<ApiType>(items[1], true, out var value) ? value : null;
    Id = items.Length > 2 ? items[2] : null;
  }

  /// <summary>
  /// If the text match api type enum value then value will be created otherwise null.
  /// </summary>
  public ApiType? ApiType { get; }

  public string? Tenant { get; }

  public string? Id { get; }

  /// <summary>
  /// The raw text received from notification api.
  /// </summary>
  public string Text { get; }
}