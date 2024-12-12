namespace C8yServices.Extensions.Collections;

public static class CollectionExtensions
{
  public static object? GetValueOrDefault(this IDictionary<string, object?> dictionary, string key)
    => dictionary.TryGetValue(key, out var value) ? value : null;

  public static bool IsEquivalentTo<T>(this IEnumerable<T> first, IEnumerable<T> second)
  {
    if (ReferenceEquals(first, second))
    {
      return true;
    }
    var firstRes = first.TryGetNonEnumeratedCount(out var firstCount);
    var secondRes = second.TryGetNonEnumeratedCount(out var secondCount);

    return !firstRes || !secondRes
      ? first.OrderBy(arg => arg).SequenceEqual(second.OrderBy(arg => arg))
      : firstCount == secondCount && first.OrderBy(arg => arg).SequenceEqual(second.OrderBy(arg => arg));
  }

  public static IReadOnlyCollection<T> AsReadOnlyCollectionOrToArrayIfItIsEnumerable<T>(this IEnumerable<T> items) => 
    items as IReadOnlyCollection<T> ?? items.ToArray();

  public static IReadOnlyList<T> AsReadOnlyListOrToArrayIfItIsEnumerable<T>(this IEnumerable<T> items) => 
    items as IReadOnlyList<T> ?? items.ToArray();
}