using C8yServices.Authentication.Basic;
using C8yServices.Authentication.OAuth;
using C8yServices.Configuration;
using C8yServices.Extensions.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SubscriptionHandlingExample.Models;
using SubscriptionHandlingExample.Services;

namespace SubscriptionHandlingExample.Controllers;

[ApiController]
[Route("subscriptionEvents")] // base path, may also be empty
[Authorize(AuthenticationSchemes = $"{BasicAuthenticationDefaults.AuthenticationScheme},{OAuthAuthenticationDefaults.AuthenticationScheme}")] // possible authentication schemes (can only use schemes that have been added to IServiceCollection)
public sealed class SubscriptionEventController : ControllerBase
{
  private readonly ISubscriptionEventService _subscriptionEventService;
  private readonly string _bootstrapTenant;

  public SubscriptionEventController(ISubscriptionEventService subscriptionEventService, IOptions<C8YConfiguration> c8YConfiguration)
  {
    _subscriptionEventService = subscriptionEventService;
    _bootstrapTenant = c8YConfiguration.Value.BootstrapTenant;
  }

  [HttpGet("added")]
  public async Task<ActionResult<IReadOnlyList<SubscriptionAddedEvent>>> GetAllSubscribtionAddedEvents(CancellationToken cancellationToken)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null || tenant != _bootstrapTenant)
    {
      return Unauthorized();
    }
    var result = await _subscriptionEventService.GetAllSubscriptionAddedEvents(cancellationToken);
    return result switch
    {
      null => StatusCode(422),
      _ => result.ToList()
    };
  }

  [HttpGet("removed")]
  public async Task<ActionResult<IReadOnlyList<SubscriptionRemovedEvent>>> GetAllSubscribtionRemovedEvents(CancellationToken cancellationToken)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null || tenant != _bootstrapTenant)
    {
      return Unauthorized();
    }
    var result = await _subscriptionEventService.GetAllSubscriptionRemovedEvents(cancellationToken);
    return result switch
    {
      null => StatusCode(422),
      _ => result.ToList()
    };
  }

  [HttpDelete("added")]
  public async Task<ActionResult<bool>> DeleteAllSubscribtionAddedEvents(CancellationToken cancellationToken)
  {
    var tenant = User.GetC8yTenant();

    return tenant is null || tenant != _bootstrapTenant
      ? (ActionResult<bool>)Unauthorized()
      : (ActionResult<bool>)await _subscriptionEventService.RemoveAllSubscriptionAddedEvents(cancellationToken);
  }

  [HttpDelete("removed")]
  public async Task<ActionResult<bool>> DeleteAllSubscribtionRemovedEvents(CancellationToken cancellationToken)
  {
    var tenant = User.GetC8yTenant();

    return tenant is null || tenant != _bootstrapTenant
      ? (ActionResult<bool>)Unauthorized()
      : (ActionResult<bool>)await _subscriptionEventService.RemoveAllSubscriptionRemovedEvents(cancellationToken);
  }
}
