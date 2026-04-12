using Helpdesk.Api.Extensions;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        if (!TryGetCurrentUserId(out var userId))
            return this.ApiUnauthorized("Authentication is required to view notifications.");

        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (!TryGetCurrentUserId(out var userId))
            return this.ApiUnauthorized("Authentication is required to update notifications.");

        var updated = await _notificationService.MarkAsReadAsync(id, userId);

        if (!updated)
            return this.ApiNotFound("Notification not found.");

        return NoContent();
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId);
    }
}
