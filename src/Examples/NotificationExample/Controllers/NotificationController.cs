using C8yServices.Authentication.Basic;
using C8yServices.Authentication.OAuth;
using C8yServices.Extensions.Security;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NotificationExample.Models;
using NotificationExample.Services;

namespace NotificationExample.Controllers;

[ApiController]
[Route("example")] // base path, may also be empty
[Authorize(AuthenticationSchemes = $"{BasicAuthenticationDefaults.AuthenticationScheme},{OAuthAuthenticationDefaults.AuthenticationScheme}")] // possible authentication schemes (can only use schemes that have been added to IServiceCollection)
public class NotificationController : ControllerBase
{
  private readonly INotificationManagerService _notificationManagerService;

  public NotificationController(INotificationManagerService notificationManagerService)
  {
    _notificationManagerService = notificationManagerService;
  }

  [HttpPost("notifications/tenant")] // endpoint for creating new notification subscriptions
  public async Task<ActionResult> CreateTenantSubscription([FromBody] TenantNotificationCreateInput input, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    var result = await _notificationManagerService.CreateTenantSubscription(tenant, input, token);
    return result.IsT0 ? Ok() : BadRequest();
  }

  [HttpPost("notifications/object")] // endpoint for creating new notification subscriptions
  public async Task<ActionResult> CreateObjectSubscription([FromBody] ObjectNotificationCreateInput input, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    var result = await _notificationManagerService.CreateObjectSubscription(tenant, input, token);
    return result.IsT0 ? Ok() : BadRequest();
  }  

  [HttpDelete("notifications/{subscriptionName}/unregister")] // endpoint for unregistering from a subscription
  public async Task<ActionResult> UnregisterFromSubscription([FromRoute] string subscriptionName, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    var result = await _notificationManagerService.UnregisterSubscription(tenant, subscriptionName, token);
    return result.IsT0 ? NoContent() : BadRequest();
  } 

  [HttpDelete("notifications/{subscriptionName}")] // endpoint for deleting subscriptions
  public async Task<ActionResult> DeleteSubscription([FromRoute] string subscriptionName, CancellationToken token)
  {
    var tenant = User.GetC8yTenant();

    if (tenant is null)
    {
      return Unauthorized();
    }

    var result = await _notificationManagerService.DeleteSubscription(tenant, subscriptionName, token);
    return result.IsT0 ? NoContent() : BadRequest();
  }
}