using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed record TokenClaim(string Subscriber, string Subscription) : IEqualityOperators<TokenClaim, TokenClaim, bool>;