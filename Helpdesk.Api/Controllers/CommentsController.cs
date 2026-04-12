using Helpdesk.Api.Extensions;
using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tickets/{ticketId:int}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(int ticketId)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return this.ApiUnauthorized("Authentication is required to view comments.");

        var comments = await _commentService.GetTicketCommentsAsync(ticketId, userId, role);

        if (comments == null)
            return this.ApiNotFound("Ticket or comments not found.");

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment(int ticketId, CreateCommentDto dto)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return this.ApiUnauthorized("Authentication is required to add a comment.");

        var result = await _commentService.CreateCommentAsync(ticketId, dto, userId, role);

        if (result.Result == CreateCommentResult.NotFound)
            return this.ApiNotFound("Ticket not found.");

        if (result.Result == CreateCommentResult.Forbidden)
            return this.ApiForbidden("You are not allowed to add this comment.");

        return CreatedAtAction(nameof(GetComments), new { ticketId }, result.Comment);
    }

    private bool TryGetCurrentUser(out int userId, out string role)
    {
        userId = 0;
        role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && !string.IsNullOrWhiteSpace(role);
    }
}
