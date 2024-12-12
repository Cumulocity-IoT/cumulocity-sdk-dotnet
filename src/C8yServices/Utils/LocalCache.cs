using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Caching.Memory;

using OneOf;

namespace C8yServices.Utils;

public sealed class LocalCache : ILocalCache
{
  private readonly IMemoryCache _memoryCache;
  private readonly ConcurrentDictionary<string, Lazy<Locker>> _valuesForSpecificCacheKeys;

  public LocalCache(IMemoryCache memoryCache)
  {
    _memoryCache = memoryCache;
    _valuesForSpecificCacheKeys = new ConcurrentDictionary<string, Lazy<Locker>>();
  }

  [ExcludeFromCodeCoverage(Justification = Constants.NothingToTest)]
  public async Task<T?> GetOrAdd<T, TParam>(string key, int cacheTimeInSeconds, int lockTimeOutInSeconds, TParam param, Func<TParam, CancellationToken, Task<T?>> getFunc,
    CancellationToken token)
    where T : class
  {
    var result = await GetOrAdd((param, getFunc), key, cacheTimeInSeconds, lockTimeOutInSeconds, static (p, token) => GetTransformedFunc(p.param, p.getFunc, token), token);

    return result.AsT0;
  }

  public Task<OneOf<T?, OneOf.Types.Error<string>>> GetOrAdd<T, TParam>(TParam param, string key, int cacheTimeInSeconds,
    int lockTimeoutInSeconds, Func<TParam, CancellationToken, Task<OneOf<T?, OneOf.Types.Error<string>>>> getFunc, CancellationToken token) where T : class
  {
    if (cacheTimeInSeconds <= 0)
    {
      return getFunc(param, token);
    }
    var lazyLocker = _valuesForSpecificCacheKeys.GetOrAdd(key, static _ => new Lazy<Locker>(static () => new Locker()));
    var locker = lazyLocker.Value;

    return locker.GetValueAsync(static (p, token) => p.This.GetOrAddInt(p.key, p.cacheTimeInSeconds, p.param, p.getFunc, token),
      (param, This: this, key, cacheTimeInSeconds, getFunc), lockTimeoutInSeconds, token);
  }

  private static async Task<OneOf<T?, OneOf.Types.Error<string>>> GetTransformedFunc<T, TParam>(TParam param, Func<TParam, CancellationToken, Task<T?>> getFunc, CancellationToken token)
  {
    var result = await getFunc(param, token).ConfigureAwait(false);

    return OneOf<T?, OneOf.Types.Error<string>>.FromT0(result);
  }

  private async Task<OneOf<T?, OneOf.Types.Error<string>>> GetOrAddInt<T, TParam>(string key, int cacheTimeInSeconds, TParam param, Func<TParam, CancellationToken, Task<OneOf<T?, OneOf.Types.Error<string>>>> getFunc,
    CancellationToken token)
    where T : class
  {
    if (_memoryCache.TryGetValue(key, out var value) && value is T val)
    {
      return val;
    }
    var fetchResult = await getFunc(param, token).ConfigureAwait(false);
    if (fetchResult.IsT1)
    {
      return fetchResult.AsT1;
    }
    var valueFromSource = fetchResult.AsT0;

    return valueFromSource is null
      ? null
      : AddToCache(key, cacheTimeInSeconds, valueFromSource);
  }

  private T AddToCache<T>(string key, int cacheTimeInSeconds, T value) where T : class
    => _memoryCache.Set(key, value, new MemoryCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheTimeInSeconds)
    });
}