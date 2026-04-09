using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditTrailService _auditTrailService;

    public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository, IAuditTrailService auditTrailService)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _auditTrailService = auditTrailService;
    }

    public async Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, int requesterId)
    {
        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            RequesterId = requesterId,
            Status = TicketStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        var createdTicket = await _ticketRepository.CreateAsync(ticket);
        await _auditTrailService.RecordAsync(
            createdTicket.Id,
            requesterId,
            "TicketCreated",
            $"Ticket created with priority {createdTicket.Priority}.");

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

    public async Task<List<TicketResponseDto>> GetAllTicketsAsync(int userId, string role)
    {
        var tickets = await _ticketRepository.GetAllAsync();

        var filteredTickets = role switch
        {
            nameof(UserRole.Admin) => tickets,
            nameof(UserRole.Agent) => tickets.Where(ticket => ticket.AgentId == userId).ToList(),
            nameof(UserRole.Requester) => tickets.Where(ticket => ticket.RequesterId == userId).ToList(),
            _ => []
        };

        return filteredTickets.Select(ticket => new TicketResponseDto
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

    public async Task<TicketResponseDto?> GetTicketByIdAsync(int id, int userId, string role)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);

        if (ticket == null)
            return null;

        var canAccessTicket = role switch
        {
            nameof(UserRole.Admin) => true,
            nameof(UserRole.Agent) => ticket.AgentId == userId,
            nameof(UserRole.Requester) => ticket.RequesterId == userId,
            _ => false
        };

        if (!canAccessTicket)
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

    public async Task<UpdateTicketStatusResult> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto, int userId, string role)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);

        if (ticket == null)
            return UpdateTicketStatusResult.NotFound;

        var canUpdateTicket = role switch
        {
            nameof(UserRole.Admin) => true,
            nameof(UserRole.Agent) => ticket.AgentId == userId,
            _ => false
        };

        if (!canUpdateTicket)
            return UpdateTicketStatusResult.Forbidden;

        if (!IsValidStatusTransition(ticket.Status, dto.Status))
            return UpdateTicketStatusResult.InvalidTransition;

        var previousStatus = ticket.Status;
        ticket.Status = dto.Status;

        await _ticketRepository.UpdateAsync(ticket);
        await _auditTrailService.RecordAsync(
            ticket.Id,
            userId,
            "StatusChanged",
            $"Status changed from {previousStatus} to {ticket.Status}.");

        return UpdateTicketStatusResult.Success;
    }

    public async Task<bool> AssignTicketAsync(int ticketId, AssignTicketDto dto, int actorId)
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
        await _auditTrailService.RecordAsync(
            ticket.Id,
            actorId,
            "TicketAssigned",
            $"Ticket assigned to agent {dto.AgentId}.");

        return true;
    }

    private static bool IsValidStatusTransition(TicketStatus currentStatus, TicketStatus newStatus)
    {
        if (currentStatus == newStatus)
            return true;

        return currentStatus switch
        {
            TicketStatus.New => newStatus == TicketStatus.Assigned,
            TicketStatus.Assigned => newStatus == TicketStatus.InProgress,
            TicketStatus.InProgress => newStatus == TicketStatus.Resolved,
            TicketStatus.Resolved => newStatus == TicketStatus.Closed,
            TicketStatus.Closed => false,
            _ => false
        };
    }
}
