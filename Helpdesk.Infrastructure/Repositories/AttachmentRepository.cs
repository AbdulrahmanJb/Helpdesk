using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly HelpdeskDbContext _context;

    public AttachmentRepository(HelpdeskDbContext context)
    {
        _context = context;
    }

    public async Task<Attachment> CreateAsync(Attachment attachment)
    {
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task<List<Attachment>> GetByTicketIdAsync(int ticketId)
    {
        return await _context.Attachments
            .Where(attachment => attachment.TicketId == ticketId)
            .ToListAsync();
    }

    public async Task<Attachment?> GetByIdAsync(int id)
    {
        return await _context.Attachments.FirstOrDefaultAsync(attachment => attachment.Id == id);
    }

    public async Task DeleteAsync(Attachment attachment)
    {
        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }
}
