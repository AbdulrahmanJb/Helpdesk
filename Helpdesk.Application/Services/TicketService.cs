using System;
using System.Collections.Generic;
using System.Text;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;
using Helpdesk.Application.DTOs;


namespace Helpdesk.Application.Services
{
public class TicketService : ITicketService
{
    private static readonly List<Ticket> _tickets = new();

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto)
    {
        var ticket = new Ticket
        {
            Id = _tickets.Count + 1,
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            RequesterId = dto.RequesterId,
            Status = TicketStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _tickets.Add(ticket);

        var response = new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            RequesterId = ticket.RequesterId,
            AgentId = ticket.AgentId,
            CreatedAt = ticket.CreatedAt
        };

        return await Task.FromResult(response);
    }

    public async Task<List<TicketResponseDto>> GetAllTicketsAsync()
    {
        var tickets = _tickets.Select(ticket => new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            RequesterId = ticket.RequesterId,
            AgentId = ticket.AgentId,
            CreatedAt = ticket.CreatedAt
        }).ToList();

        return await Task.FromResult(tickets);
    }

    public async Task<TicketResponseDto?> GetTicketByIdAsync(int id)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return null;

        var result = new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            RequesterId = ticket.RequesterId,
            AgentId = ticket.AgentId,
            CreatedAt = ticket.CreatedAt
        };

        return await Task.FromResult(result);
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto)
    {
        var ticket = _tickets.FirstOrDefault(t => t.Id == id);

        if (ticket == null)
            return false;

        ticket.Status = dto.Status;

        return await Task.FromResult(true);
    }
}
}
