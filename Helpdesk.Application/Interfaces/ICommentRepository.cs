using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Interfaces;

public interface ICommentRepository
{
    Task<Comment> CreateAsync(Comment comment);

    Task<List<Comment>> GetByTicketIdAsync(int ticketId);
}
