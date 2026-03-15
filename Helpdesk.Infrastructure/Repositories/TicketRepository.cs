using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly HelpdeskDbContext _context;

    public TicketRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket> CreateAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<List<Ticket>> GetAllAsync()
    {
        return await _context.Tickets.ToListAsync();
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
}