using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Application.Services;

public class AuditTrailService : IAuditTrailService
{
    private readonly IAuditTrailRepository _auditTrailRepository;
    private readonly ITicketRepository _ticketRepository;

    public AuditTrailService(IAuditTrailRepository auditTrailRepository, ITicketRepository ticketRepository)
    {
        _auditTrailRepository = auditTrailRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task RecordAsync(int ticketId, int actorId, string action, string description)
    {
        await _auditTrailRepository.CreateAsync(new AuditTrailEntry
        {
            TicketId = ticketId,
            ActorId = actorId,
            Action = action,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<List<AuditTrailEntryResponseDto>?> GetTicketAuditTrailAsync(int ticketId, int userId, string role)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket == null || !CanAccessTicket(ticket, userId, role))
            return null;

        var entries = await _auditTrailRepository.GetByTicketIdAsync(ticketId);

        return entries
            .OrderBy(entry => entry.CreatedAt)
            .Select(entry => new AuditTrailEntryResponseDto
            {
                Id = entry.Id,
                TicketId = entry.TicketId,
                ActorId = entry.ActorId,
                Action = entry.Action,
                Description = entry.Description,
                CreatedAt = entry.CreatedAt
            })
            .ToList();
    }

    private static bool CanAccessTicket(Ticket ticket, int userId, string role)
    {
        return role switch
        {
            nameof(UserRole.Admin) => true,
            nameof(UserRole.Agent) => ticket.AgentId == userId,
            nameof(UserRole.Requester) => ticket.RequesterId == userId,
            _ => false
        };
    }
}
