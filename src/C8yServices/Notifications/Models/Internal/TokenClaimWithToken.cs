using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Notifications.Models.Internal;

[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
internal sealed record TokenClaimWithToken(TokenClaim TokenClaim, string Token) : IEqualityOperators<TokenClaimWithToken, TokenClaimWithToken, bool>;