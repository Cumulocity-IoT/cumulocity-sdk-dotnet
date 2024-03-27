using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OneOf;

using RestControllerExample.Models;

namespace RestControllerExample.Services;

public interface IExampleManagedObjectService
{
  public Task<OneOf<ExampleQueryableManagedObject?, OneOf.Types.Error>> CreateExampleManagedObject(string tenant, ExampleQueryableManagedObject exampleQueryableManagedObject, CancellationToken token);
  public Task<OneOf<ExampleQueryableManagedObject?, OneOf.Types.Error>> GetExampleManagedObject(string tenant, string id, CancellationToken token);
  public Task<OneOf<IReadOnlyList<ExampleQueryableManagedObject>, OneOf.Types.Error>> GetExampleManagedObjects(string tenant, CancellationToken token, string? exampleFragmentValue = null);
  public Task<bool> DeleteExampleManagedObject(string tenant, string id, CancellationToken token);
}