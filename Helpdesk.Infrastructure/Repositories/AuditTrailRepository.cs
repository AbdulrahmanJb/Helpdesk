using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Repositories;

public class AuditTrailRepository : IAuditTrailRepository
{
    private readonly HelpdeskDbContext _context;

    public AuditTrailRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<AuditTrailEntry> CreateAsync(AuditTrailEntry entry)
    {
        _context.AuditTrailEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<List<AuditTrailEntry>> GetByTicketIdAsync(int ticketId)
    {
        return await _context.AuditTrailEntries
            .Where(entry => entry.TicketId == ticketId)
            .ToListAsync();
    }
}
