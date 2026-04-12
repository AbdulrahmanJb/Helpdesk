using Helpdesk.Api.Extensions;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets/{ticketId:int}/audit-trail")]
public class AuditTrailController : ControllerBase
{
    private readonly IAuditTrailService _auditTrailService;

    public AuditTrailController(IAuditTrailService auditTrailService)
    {
        _auditTrailService = auditTrailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditTrail(int ticketId)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return this.ApiUnauthorized("Authentication is required to view audit trail.");

        var entries = await _auditTrailService.GetTicketAuditTrailAsync(ticketId, userId, role);

        if (entries == null)
            return this.ApiNotFound("Audit trail not found for the ticket.");

        return Ok(entries);
    }

    private bool TryGetCurrentUser(out int userId, out string role)
    {
        userId = 0;
        role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && !string.IsNullOrWhiteSpace(role);
    }
}
