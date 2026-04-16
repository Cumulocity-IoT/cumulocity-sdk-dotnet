namespace C8yServices.Extensions.Collections;

public static class IEnumerableExtensions
{
  public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> values) where T : class
  {
    ArgumentNullException.ThrowIfNull(values);
    return from value in values
           where value != null
           select value;
  }

}