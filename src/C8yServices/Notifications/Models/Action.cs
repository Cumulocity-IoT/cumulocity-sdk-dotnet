using System.Numerics;

namespace C8yServices.Notifications.Models;

/// <summary>
/// The action metadata
/// </summary>
public sealed record Action : IEqualityOperators<Action, Action, bool>
{
  /// <summary>
  /// Initializes a new instance of the <see cref="Action"/> class.
  /// </summary>
  /// <param name="text">The text.</param>
  public Action(string text)
  {
    Text = text;
    ActionType = Enum.TryParse<ActionType>(text, true, out var value) ? value : null;
  }

  /// <summary>
  /// The raw text received from notification api.
  /// </summary>
  public string Text { get; }

  /// <summary>
  /// If the text match api type enum value then value will be created otherwise null.
  /// </summary>
  public ActionType? ActionType { get; }
}