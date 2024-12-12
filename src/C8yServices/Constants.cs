using System.Diagnostics.CodeAnalysis;

using C8yServices.Common.Models;

namespace C8yServices;

[ExcludeFromCodeCoverage(Justification = NothingToTest)]
internal static class Constants
{
  public const string NothingToTest = "Nothing to test";

  public static readonly ApiError NullResultApiError = new("Result is null.", null);
}