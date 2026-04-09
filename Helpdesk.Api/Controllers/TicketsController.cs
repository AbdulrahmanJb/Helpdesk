using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTickets()
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        var tickets = await _ticketService.GetAllTicketsAsync(userId, role);
        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        var ticket = await _ticketService.GetTicketByIdAsync(id, userId, role);

        if (ticket == null)
            return NotFound();

        return Ok(ticket);
    }

    [Authorize(Roles = "Requester,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTicket(CreateTicketDto dto)
    {
        var requesterIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(requesterIdClaim, out var requesterId))
            return Unauthorized();

        var ticket = await _ticketService.CreateTicketAsync(dto, requesterId);

        return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticket);
    }

    private bool TryGetCurrentUser(out int userId, out string role)
    {
        userId = 0;
        role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdClaim, out userId) && !string.IsNullOrWhiteSpace(role);
    }

    [Authorize(Roles = "Admin,Agent")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateTicketStatusDto dto)
    {
        if (!TryGetCurrentUser(out var userId, out var role))
            return Unauthorized();

        var result = await _ticketService.UpdateTicketStatusAsync(id, dto, userId, role);

        if (result == UpdateTicketStatusResult.NotFound)
            return NotFound();

        if (result == UpdateTicketStatusResult.Forbidden)
            return Forbid();

        if (result == UpdateTicketStatusResult.InvalidTransition)
            return BadRequest("Invalid ticket status transition.");

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/assign")]
    public async Task<IActionResult> AssignTicket(int id, AssignTicketDto dto)
    {
        var actorIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(actorIdClaim, out var actorId))
            return Unauthorized();

        var assigned = await _ticketService.AssignTicketAsync(id, dto, actorId);

        if (!assigned)
            return BadRequest("Ticket not found, agent not found, or selected user is not an agent.");

        return NoContent();
    }
}
