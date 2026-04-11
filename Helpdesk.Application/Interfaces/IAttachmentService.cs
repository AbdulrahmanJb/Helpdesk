using Helpdesk.Application.DTOs;

namespace Helpdesk.Application.Interfaces;

public interface IAttachmentService
{
    Task<AttachmentResponseDto?> UploadAsync(int ticketId, int userId, string role, string originalFileName, string contentType, Stream fileStream, long fileSize);

    Task<List<AttachmentResponseDto>?> GetTicketAttachmentsAsync(int ticketId, int userId, string role);

    Task<AttachmentDownloadDto?> DownloadAsync(int attachmentId, int userId, string role);

    Task<bool> DeleteAsync(int attachmentId, int userId, string role);
}
