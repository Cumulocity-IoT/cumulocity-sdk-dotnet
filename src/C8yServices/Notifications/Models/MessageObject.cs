using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models;

/// <summary>
/// The message object with the payload and metadata
/// </summary>
/// <param name="Message">The raw message received from notification api without any metadata.</param>
/// <param name="Api">The Cumulocity api, which generate the message.</param>
/// <param name="Action">The action, which generate the message.</param>
[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public sealed record MessageObject(string Message, Api Api, Action Action) : IEqualityOperators<MessageObject, MessageObject, bool>;