using OneOf;

namespace C8yServices.Utils;

public interface ILocalCache
{
  Task<T?> GetOrAdd<T, TParam>(string key, int cacheTimeInSeconds, int lockTimeOutInSeconds, TParam param, Func<TParam, CancellationToken, Task<T?>> getFunc,
    CancellationToken token)
    where T : class;

  Task<OneOf<T?, OneOf.Types.Error<string>>> GetOrAdd<T, TParam>(TParam param, string key, int cacheTimeInSeconds, int lockTimeoutInSeconds,
    Func<TParam, CancellationToken, Task<OneOf<T?, OneOf.Types.Error<string>>>> getFunc, CancellationToken token) where T : class;
}