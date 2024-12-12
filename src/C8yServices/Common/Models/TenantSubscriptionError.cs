using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace C8yServices.Common.Models;

/// <summary>
/// The api error. 
/// </summary>
/// <param name="Message">Get or set detailed error message.</param>
[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public sealed record TenantSubscriptionError(string TenantId) : IEqualityOperators<TenantSubscriptionError, TenantSubscriptionError, bool>;