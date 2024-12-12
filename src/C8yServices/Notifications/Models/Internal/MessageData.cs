using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed record MessageData(string Acknowledgement, string Action, string ApiUrl, string RawMessage) : IEqualityOperators<MessageData, MessageData, bool>;