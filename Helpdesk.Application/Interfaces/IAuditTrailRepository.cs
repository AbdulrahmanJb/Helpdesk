using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Interfaces;

public interface IAuditTrailRepository
{
    Task<AuditTrailEntry> CreateAsync(AuditTrailEntry entry);

    Task<List<AuditTrailEntry>> GetByTicketIdAsync(int ticketId);
}
