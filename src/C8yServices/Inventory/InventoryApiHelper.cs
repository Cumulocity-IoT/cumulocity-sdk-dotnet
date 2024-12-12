using Client.Com.Cumulocity.Client.Model;
using Client.Com.Cumulocity.Client.Supplementary;

using OneOf;
using OneOf.Types;

namespace C8yServices.Inventory;

public static class InventoryApiHelper
{
  public const int DefaultPageSize = 5;
  /// <inheritdoc/>
  public static async Task<OneOf<T, Error<string>>> RequestManagedObject<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string id, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : ManagedObject
  {
    if (string.IsNullOrWhiteSpace(id))
    {
      return new Error<string>("no valid ID given to request managed object");
    }

    try
    {
      var managedObject = await cumulocityCoreLibrary.Inventory.ManagedObjectsApi.GetManagedObject<T>(id: id, withChildren: withChildren, withParents: withParents, cToken: token);

      return managedObject is not null
        ? managedObject
        : new Error<string>($"found no managed object of type '{typeof(T).Name}' with ID '{id}'");
    }
    catch (Exception e)
    {
      return new Error<string>($"can't fetch managed object of type '{typeof(T).Name}' by ID '{id}': {e.Message}");
    }
  }

  /// <inheritdoc/>
  public static Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestPageAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, int currentPage, int pageSize = DefaultPageSize, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : QueryableManagedObject
  {
    var requestParams = QueryableManagedObject.GetRequestParameters<T>();
    if (requestParams.Count == 0)
    {
      return Task.FromResult((OneOf<IReadOnlyList<T>, Error<string>>)new Error<string>($"no request parameters of queryable managed object type '{typeof(T).Name}' given"));
    }


    return RequestPage<T>(cumulocityCoreLibrary, requestParams!, currentPage, pageSize, withChildren, withParents, token);
  }

  /// <inheritdoc/>
  public static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestAllAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, int pageSize = DefaultPageSize, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : QueryableManagedObject
  {
    return await RequestAllPages((ps, currentPage) => RequestPageAsync<T>(cumulocityCoreLibrary, currentPage, ps, withChildren, withParents, token), pageSize);
  }

  /// <inheritdoc/>
  public static async Task<OneOf<T, Error<string>>> RequestFirstAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : QueryableManagedObject
  {
    var result = await RequestPageAsync<T>(cumulocityCoreLibrary, 1, 1, withChildren, withParents, token);
    if (result.IsT1)
      return result.AsT1;

    var managedObjects = result.AsT0;
    return managedObjects.Count > 0
      ? managedObjects[0]
      : new Error<string>($"found no managed object of type '{typeof(T).Name}'");
  }


  /// <inheritdoc/>
  public static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestPageByQueryAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string query, int currentPage, int pageSize = DefaultPageSize, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : ManagedObject
  {
    if (string.IsNullOrWhiteSpace(query))
    {
      return new Error<string>($"no query given to request managed objects of type '{typeof(T).Name}' by query");
    }

    var requestParams = new Dictionary<QueryableManagedObject.RequestParameterType, string?>
    {
      { QueryableManagedObject.RequestParameterType.Query, query }
    };

    return await RequestPage<T>(cumulocityCoreLibrary, requestParams, currentPage, pageSize, withChildren, withParents, token);
  }

  /// <inheritdoc/>
  public static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestAllByQueryAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string query, int pageSize = DefaultPageSize, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : ManagedObject
  {
    if (string.IsNullOrWhiteSpace(query))
    {
      return new Error<string>($"no query given to request managed objects of type '{typeof(T).Name}' by query");
    }

    var requestParams = new Dictionary<QueryableManagedObject.RequestParameterType, string?>
    {
      { QueryableManagedObject.RequestParameterType.Query, query }
    };

    return await RequestAllPages((ps, currentPage) => RequestPage<T>(cumulocityCoreLibrary, requestParams, currentPage, ps, withChildren, withParents, token), pageSize);
  }

  /// <inheritdoc/>
  public static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestAllByFragmentTypeAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string fragmentType, int pageSize = DefaultPageSize, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : ManagedObject
  {
    if (string.IsNullOrWhiteSpace(fragmentType))
    {
      return new Error<string>($"no fragment type given to request managed objects of type '{typeof(T).Name}'");
    }

    var requestParams = new Dictionary<QueryableManagedObject.RequestParameterType, string?>
    {
      { QueryableManagedObject.RequestParameterType.FragmentType, fragmentType }
    };

    return await RequestAllPages((ps, currentPage) => RequestPage<T>(cumulocityCoreLibrary, requestParams, currentPage, ps, withChildren, withParents, token), pageSize);
  }

  /// <inheritdoc/>
  public static async Task<OneOf<T, Error<string>>> RequestFirstByQueryAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string query, bool withChildren = false, bool withParents = false, CancellationToken token = default) where T : ManagedObject
  {
    var result = await RequestPageByQueryAsync<T>(cumulocityCoreLibrary, query, 1, 1, withChildren, withParents, token);
    if (result.IsT1)
      return result.AsT1;

    var managedObjects = result.AsT0;
    return managedObjects.Count > 0
      ? managedObjects[0]
      : new Error<string>($"found no managed object of type '{typeof(T).Name}' by query = {query}");
  }



  /// <inheritdoc/>
  public static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestChildrenPageAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string parentId, ManagedObjectChildType childType, int currentPage, int pageSize = DefaultPageSize, bool withChildren = false, CancellationToken token = default) where T : ManagedObject
  {
    if (string.IsNullOrWhiteSpace(parentId))
    {
      return new Error<string>("no valid parent ID given to request managed object children");
    }

    try
    {
      var query = GetQuery<T>();
      var collection = await RequestChildReferencesPageAsync<T>(cumulocityCoreLibrary, parentId, childType, currentPage, pageSize, query, withChildren, token);
      var children = collection ?? new();

      return children.PReferences.Where(references => references.PManagedObject is not null).Select(references => references.PManagedObject!).ToList();
    }
    catch (Exception e)
    {
      return new Error<string>($"can't request child managed objects (parent ID = {parentId}) of type '{typeof(T).Name}' at page {currentPage} (page size = {pageSize}): {e.Message}'");
    }
  }

  /// <inheritdoc/>
  public static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestAllChildrenAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string parentId, ManagedObjectChildType childType, int pageSize = DefaultPageSize, bool withChildren = false, CancellationToken token = default) where T : ManagedObject
  {
    return await RequestAllPages((ps, currentPage) => RequestChildrenPageAsync<T>(cumulocityCoreLibrary, parentId, childType, currentPage, ps, withChildren, token), pageSize);
  }

  /// <summary>
  /// checks if given type is a <see cref="QueryableManagedObject"/> and returns a query string or <c>null</c>
  /// </summary>
  private static string? GetQuery<T>() where T : ManagedObject
  {
    var requestParameters = QueryableManagedObject.GetRequestParameters<T>();
    if (requestParameters.Count == 0)
      return null;

    var query = requestParameters.GetValueOrDefault(QueryableManagedObject.RequestParameterType.Query);
    if (!string.IsNullOrWhiteSpace(query))
      return query;

    query = "";
    var type = requestParameters!.GetValueOrDefault(QueryableManagedObject.RequestParameterType.Type, null);
    if (!string.IsNullOrWhiteSpace(type))
      query += $"(type eq '{type}')";

    var fragmentType = requestParameters!.GetValueOrDefault(QueryableManagedObject.RequestParameterType.FragmentType, null);
    if (!string.IsNullOrWhiteSpace(fragmentType))
      query += $"{(query.Length > 0 ? " and " : "")} has('{fragmentType}')";

    return query;
  }

  /// <summary>
  /// requests a page of children of given child type
  /// </summary>
  private static async Task<ManagedObjectReferenceCollection<T>?> RequestChildReferencesPageAsync<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, string parentId, ManagedObjectChildType childType, int currentPage, int pageSize, string? query, bool withChildren, CancellationToken token = default)
    where T : ManagedObject
  {
    return childType switch
    {
      ManagedObjectChildType.DeviceChild => await cumulocityCoreLibrary.Inventory.ChildOperationsApi.GetChildDevices<T>(id: parentId, currentPage: currentPage, pageSize: pageSize, query: query, withChildren: withChildren, cToken: token),
      ManagedObjectChildType.AssetChild => await cumulocityCoreLibrary.Inventory.ChildOperationsApi.GetChildAssets<T>(id: parentId, currentPage: currentPage, pageSize: pageSize, query: query, withChildren: withChildren, cToken: token),
      ManagedObjectChildType.AdditionChild => await cumulocityCoreLibrary.Inventory.ChildOperationsApi.GetChildAdditions<T>(id: parentId, currentPage: currentPage, pageSize: pageSize, query: query, withChildren: withChildren, cToken: token),
      _ => null
    };
  }

  /// <summary>
  /// requests a page of managed objects of type 'T' with given page size using given request parameters
  /// </summary>
  private static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestPage<T>(this ICumulocityCoreLibrary cumulocityCoreLibrary, Dictionary<QueryableManagedObject.RequestParameterType, string?> requestParams, int currentPage, int pageSize, bool withChildren, bool withParents, CancellationToken token = default) where T : ManagedObject
  {
    if (requestParams.Count == 0)
    {
      return new Error<string>($"no request parameters given to request page of managed object type '{typeof(T).Name}'");
    }

    try
    {
      var managedObjectCollection = await cumulocityCoreLibrary.Inventory.ManagedObjectsApi.GetManagedObjects<T>(
        type: requestParams.GetValueOrDefault(QueryableManagedObject.RequestParameterType.Type, null) ?? null,
        fragmentType: requestParams.GetValueOrDefault(QueryableManagedObject.RequestParameterType.FragmentType, null) ?? null,
        text: requestParams.GetValueOrDefault(QueryableManagedObject.RequestParameterType.Text, null) ?? null,
        query: requestParams.GetValueOrDefault(QueryableManagedObject.RequestParameterType.Query, null) ?? null,
        currentPage: currentPage > 0 ? currentPage : 1,
        pageSize: pageSize > 0 ? pageSize : DefaultPageSize,
        withChildren: withChildren,
        withParents: withParents,
        cToken: token);
      return managedObjectCollection is null
        ? []
        : managedObjectCollection.ManagedObjects;
    }
    catch (Exception e)
    {
      return new Error<string>($"can't request page {currentPage} of managed objects of type '{typeof(T).Name}': {e.Message}");
    }
  }

  /// <summary>
  /// requests all pages of managed objects of type 'T' with given page size using given request function
  /// </summary>
  private static async Task<OneOf<IReadOnlyList<T>, Error<string>>> RequestAllPages<T>(Func<int, int, Task<OneOf<IReadOnlyList<T>, Error<string>>>> requestFunction, int pageSize) where T : ManagedObject
  {
    var managedObjects = new List<T>();
    var currentPage = 0;
    if (pageSize <= 0)
    {
      pageSize = DefaultPageSize;
    }

    int count;
    do
    {
      var result = await requestFunction(pageSize, ++currentPage);
      if (result.IsT1)
      {
        return result.AsT1;
      }

      var managedObjectsPage = result.AsT0;
      managedObjects.AddRange(managedObjectsPage);
      count = managedObjectsPage.Count;
    } while (count > 0);

    return managedObjects;
  }
}