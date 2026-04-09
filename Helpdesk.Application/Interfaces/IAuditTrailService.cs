using Helpdesk.Application.DTOs;

namespace Helpdesk.Application.Interfaces;

public interface IAuditTrailService
{
    Task RecordAsync(int ticketId, int actorId, string action, string description);

    Task<List<AuditTrailEntryResponseDto>?> GetTicketAuditTrailAsync(int ticketId, int userId, string role);
}
