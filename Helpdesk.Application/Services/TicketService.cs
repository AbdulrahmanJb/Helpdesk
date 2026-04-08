using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;

    public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto)
    {
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            RequesterId = dto.RequesterId,
            Status = TicketStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        var createdTicket = await _ticketRepository.CreateAsync(ticket);

        return new TicketResponseDto
        {
            Id = createdTicket.Id,
            Title = createdTicket.Title,
            Description = createdTicket.Description,
            Priority = createdTicket.Priority,
            Status = createdTicket.Status,
            RequesterId = createdTicket.RequesterId,
            AgentId = createdTicket.AgentId,
            CreatedAt = createdTicket.CreatedAt
        };
    }

    public async Task<List<TicketResponseDto>> GetAllTicketsAsync()
    {
        var tickets = await _ticketRepository.GetAllAsync();

        return tickets.Select(ticket => new TicketResponseDto
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
    }

    public async Task<TicketResponseDto?> GetTicketByIdAsync(int id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);

        if (ticket == null)
            return null;

        return new TicketResponseDto
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
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);

        if (ticket == null)
            return false;

        ticket.Status = dto.Status;

        await _ticketRepository.UpdateAsync(ticket);

        return true;
    }

    public async Task<bool> AssignTicketAsync(int ticketId, AssignTicketDto dto)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket == null)
            return false;

        var agent = await _userRepository.GetByIdAsync(dto.AgentId);

        if (agent == null)
            return false;

        if (agent.Role != UserRole.Agent)
            return false;

        ticket.AgentId = dto.AgentId;
        ticket.Status = TicketStatus.Assigned;

        await _ticketRepository.UpdateAsync(ticket);

        return true;
    }
}