using System.Net;


using C8yServices.Inventory;
using C8yServices.RestApi;

using Client.Com.Cumulocity.Client.Supplementary;

using OneOf;
using OneOf.Types;

using RestControllerExample.Models;

namespace RestControllerExample.Services;
public sealed class ExampleManagedObjectService : IExampleManagedObjectService
{
  private readonly ILogger<ExampleManagedObjectService> _logger;
  private readonly ICumulocityCoreLibraryProvider _cumulocityCoreLibraryProvider;

  public ExampleManagedObjectService(ILogger<ExampleManagedObjectService> logger, ICumulocityCoreLibraryProvider c8yApiProvider)
  {
    _logger = logger;
    _cumulocityCoreLibraryProvider = c8yApiProvider;
  }

  public async Task<OneOf<ExampleQueryableManagedObject?, Error>> CreateExampleManagedObject(string tenant, ExampleQueryableManagedObject exampleQueryableManagedObject, CancellationToken token)
  {
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenant);

    if (cumulocityCoreLibrary is null)
    {
      return new Error();
    }

    try
    {
      return await cumulocityCoreLibrary.Inventory.ManagedObjectsApi.CreateManagedObject(exampleQueryableManagedObject, cToken: token);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception while trying to create managed object");
      return new Error();
    }
  }

  public async Task<OneOf<ExampleQueryableManagedObject?, Error>> GetExampleManagedObject(string tenant, string id, CancellationToken token)
  {
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenant);

    if (cumulocityCoreLibrary is null)
    {
      return new Error();
    }

    try
    {
      var result = await cumulocityCoreLibrary.Inventory.ManagedObjectsApi.GetManagedObject<ExampleQueryableManagedObject>(id, cToken: token);
      return result;
    }
    catch (Exception ex)
    {
      if (ex is HttpRequestException exception && exception.GetStatusCode() == HttpStatusCode.NotFound)
      {
        return null;
      }
      _logger.LogError(ex, "Exception while trying to fetch managed object with id {Id}", id);
      return new Error();
    }
  }

  public async Task<OneOf<IReadOnlyList<ExampleQueryableManagedObject>, Error>> GetExampleManagedObjects(string tenant, CancellationToken token, string? exampleFragmentValue = null)
  {
    var c8yApi = _cumulocityCoreLibraryProvider.GetForTenant(tenant);

    if (c8yApi is null)
    {
      return new Error();
    }

    Task<OneOf<IReadOnlyList<ExampleQueryableManagedObject>, Error<string>>> getTask = exampleFragmentValue == null
      ? InventoryApiHelper.RequestAllAsync<ExampleQueryableManagedObject>(c8yApi, token: token)
      : InventoryApiHelper.RequestAllByQueryAsync<ExampleQueryableManagedObject>(c8yApi, $"{ExampleQueryableManagedObject.ExampleFragmentName} eq '{exampleFragmentValue}'", token: token);

    var result = await getTask;
    return result.IsT0
      ? result.AsT0.ToList()
      : new Error();
  }

  public async Task<bool> DeleteExampleManagedObject(string tenant, string id, CancellationToken token)
  {
    var cumulocityCoreLibrary = _cumulocityCoreLibraryProvider.GetForTenant(tenant);

    if (cumulocityCoreLibrary is null)
    {
      return false;
    }

    try
    {
      return await cumulocityCoreLibrary.Inventory.ManagedObjectsApi.DeleteManagedObject(id, cToken: token) != null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception while trying to delete managed object with id {Id}", id);
      return false;
    }
  }
}
