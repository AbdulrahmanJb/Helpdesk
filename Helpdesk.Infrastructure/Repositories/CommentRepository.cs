using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly HelpdeskDbContext _context;

    public CommentRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<List<Comment>> GetByTicketIdAsync(int ticketId)
    {
        return await _context.Comments
            .Where(comment => comment.TicketId == ticketId)
            .ToListAsync();
    }
}
