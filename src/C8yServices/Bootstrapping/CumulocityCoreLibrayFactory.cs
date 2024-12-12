using C8yServices.Utils;

using Client.Com.Cumulocity.Client.Api;

namespace C8yServices.Bootstrapping;

public sealed class CumulocityCoreLibrayFactory : ICumulocityCoreLibrayFactory
{
  private readonly ICumulocityCoreLibraryProvider _cumulocityApiProvider;
  private readonly Locker _locker = new();
  private const int InitOrRefreshTimeoutInSeconds = 30;
  private readonly CumulocityCoreLibrayFactoryHelper _helper;

  public CumulocityCoreLibrayFactory(ICurrentApplicationApi currentApplicationApi, ICumulocityCoreLibraryProvider cumulocityApiProvider)
  {
    _cumulocityApiProvider = cumulocityApiProvider;
    _helper = new CumulocityCoreLibrayFactoryHelper(currentApplicationApi);
  }

  public async Task InitOrRefresh(CancellationToken token = default)
  {
    var apiCredentials = await _helper.GetApiCredentials(token).ConfigureAwait(false);
    InitOrRefresh(apiCredentials);
  }

  private void InitOrRefresh(IEnumerable<Credentials> apiCredentials) =>
    _locker.Execute(static param => param._cumulocityApiProvider.UpdateCumulocityApiCredentials(param.apiCredentials),
      (apiCredentials, _cumulocityApiProvider), InitOrRefreshTimeoutInSeconds);

  public void Dispose() =>
    _locker.Dispose();
}