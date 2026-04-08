using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var tickets = await _ticketService.GetAllTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);

        if (ticket == null)
            return NotFound();

        return Ok(ticket);
    }

    [Authorize(Roles = "Requester,Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTicket(CreateTicketDto dto)
    {
        var ticket = await _ticketService.CreateTicketAsync(dto);

        return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, ticket);
    }

    [Authorize(Roles = "Admin,Agent")]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateTicketStatusDto dto)
    {
        var updated = await _ticketService.UpdateTicketStatusAsync(id, dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/assign")]
    public async Task<IActionResult> AssignTicket(int id, AssignTicketDto dto)
    {
        var assigned = await _ticketService.AssignTicketAsync(id, dto);

        if (!assigned)
            return BadRequest("Ticket not found, agent not found, or selected user is not an agent.");

        return NoContent();
    }
}