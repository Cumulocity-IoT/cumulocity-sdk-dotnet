using C8yServices.Authentication.Basic;
using C8yServices.Authentication.OAuth;
using C8yServices.Extensions.Security;

using Client.Com.Cumulocity.Client.Model;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OneOf;

using RestControllerExample.Models;
using RestControllerExample.Services;

namespace RestControllerExample.Controllers;

[ApiController]
[Route("example")] // base path, may also be empty
[Authorize(AuthenticationSchemes = $"{BasicAuthenticationDefaults.AuthenticationScheme},{OAuthAuthenticationDefaults.AuthenticationScheme}")] // possible authentication schemes (can only use schemes that have been added to IServiceCollection)
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable S6960 // Controllers should not have mixed responsibilities, ok for this example
public sealed class ExampleController : ControllerBase
#pragma warning restore S6960 // Controllers should not have mixed responsibilities, ok for this example
#pragma warning restore IDE0079 // Remove unnecessary suppression
{
  private readonly IExampleManagedObjectService _exampleManagedObjectService;
  private readonly IExampleUserService _exampleUserService;

  public ExampleController(IExampleManagedObjectService exampleService, IExampleUserService exampleUserService)
  {
    _exampleManagedObjectService = exampleService;
    _exampleUserService = exampleUserService;
  }


  [HttpGet("user")] // simple get with user context
  public async Task<ActionResult<User<CustomProperties>?>> GetSimpleResultWithUserContext(CancellationToken token)
  {
    var userName = User.GetC8yUsername();
    var tenant = User.GetC8yTenant();

    if (userName is null || tenant is null)
    {
      return Unauthorized();
    }
    else
    {
      var result = await _exampleUserService.GetOwnUserRepresentation(tenant, userName, token);
      return result.IsT0 ? result.AsT0 : BadRequest();
    }
  }

  [HttpPost("managedObjects")] // endpoint for creating new objects
  public async Task<ActionResult<ExampleQueryableManagedObject?>> CreateExampleManagedObject([FromBody] ExampleQueryableManagedObject exampleQueryableManagedObject, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    var result = await _exampleManagedObjectService.CreateExampleManagedObject(tenant, exampleQueryableManagedObject, token);
    return result.IsT0 ? CreatedAtAction("create", result.AsT0) : BadRequest();
  }

  [HttpGet("managedObjects")] // endpoint for querying objects
  public async Task<ActionResult<IReadOnlyList<ExampleQueryableManagedObject>>> GetExampleManagedObjects(CancellationToken token, [FromQuery] string? exampleFragmentValue = null)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    OneOf<IReadOnlyList<ExampleQueryableManagedObject>, OneOf.Types.Error> result = await _exampleManagedObjectService.GetExampleManagedObjects(tenant, token, exampleFragmentValue);
    return result.IsT0 switch
    {
      true => result.AsT0.ToList(),
      _ => StatusCode(422)
    };
  }

  [HttpGet("managedObjects/{id}")] // endpoint for querying a specific object
  public async Task<ActionResult<ExampleQueryableManagedObject?>> GetExampleManagedObject([FromRoute] string id, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    OneOf<ExampleQueryableManagedObject?, OneOf.Types.Error> result = await _exampleManagedObjectService.GetExampleManagedObject(tenant, id, token);
    return result.IsT0 switch
    {
      true => result.AsT0 == null
                  ? NotFound()
                  : result.AsT0,
      _ => StatusCode(422)
    };
  }

  [HttpDelete("managedObjects/{id}")] // endpoint for deleting a specific object
  public async Task<ActionResult> DeleteExampleManagedObject([FromRoute] string id, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    var result = await _exampleManagedObjectService.DeleteExampleManagedObject(tenant, id, token);
    return result ? NoContent() : StatusCode(422);
  }
}
