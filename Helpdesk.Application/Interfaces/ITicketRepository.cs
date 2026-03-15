using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Interfaces;

public interface ITicketRepository
{
    Task<Ticket> CreateAsync(Ticket ticket);

    Task<List<Ticket>> GetAllAsync();

    Task<Ticket?> GetByIdAsync(int id);

    Task UpdateAsync(Ticket ticket);
}