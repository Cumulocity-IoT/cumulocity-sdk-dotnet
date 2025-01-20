using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Numerics;

namespace C8yServices.Common.Models;

/// <summary>
/// The api error. 
/// </summary>
/// <param name="Message">Get or set detailed error message.</param>
/// <param name="StatusCode">Get or set the http status code, if available.</param>
[ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
public sealed record ApiError(string Message, HttpStatusCode? StatusCode) : IEqualityOperators<ApiError, ApiError, bool>;