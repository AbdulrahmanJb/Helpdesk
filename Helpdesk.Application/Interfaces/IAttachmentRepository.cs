using Helpdesk.Domain.Entities;

namespace Helpdesk.Application.Interfaces;

public interface IAttachmentRepository
{
    Task<Attachment> CreateAsync(Attachment attachment);

    Task<List<Attachment>> GetByTicketIdAsync(int ticketId);

    Task<Attachment?> GetByIdAsync(int id);

    Task DeleteAsync(Attachment attachment);
}
