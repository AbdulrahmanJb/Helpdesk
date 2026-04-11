using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Application.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditTrailService _auditTrailService;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        ITicketRepository ticketRepository,
        IFileStorageService fileStorageService,
        IAuditTrailService auditTrailService)
    {
        _attachmentRepository = attachmentRepository;
        _ticketRepository = ticketRepository;
        _fileStorageService = fileStorageService;
        _auditTrailService = auditTrailService;
    }

    public async Task<AttachmentResponseDto?> UploadAsync(int ticketId, int userId, string role, string originalFileName, string contentType, Stream fileStream, long fileSize)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket == null || !CanAccessTicket(ticket, userId, role))
            return null;

        var extension = Path.GetExtension(originalFileName);
        var storedFileName = await _fileStorageService.SaveAsync(fileStream, extension);

        var attachment = await _attachmentRepository.CreateAsync(new Attachment
        {
            TicketId = ticketId,
            UploadedByUserId = userId,
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileSize = fileSize,
            CreatedAt = DateTime.UtcNow
        });

        await _auditTrailService.RecordAsync(ticketId, userId, "AttachmentAdded", $"Attachment '{originalFileName}' uploaded.");

        return MapAttachment(attachment);
    }

    public async Task<List<AttachmentResponseDto>?> GetTicketAttachmentsAsync(int ticketId, int userId, string role)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket == null || !CanAccessTicket(ticket, userId, role))
            return null;

        var attachments = await _attachmentRepository.GetByTicketIdAsync(ticketId);
        return attachments.Select(MapAttachment).ToList();
    }

    public async Task<AttachmentDownloadDto?> DownloadAsync(int attachmentId, int userId, string role)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);

        if (attachment == null)
            return null;

        var ticket = await _ticketRepository.GetByIdAsync(attachment.TicketId);

        if (ticket == null || !CanAccessTicket(ticket, userId, role))
            return null;

        var content = await _fileStorageService.ReadAsync(attachment.StoredFileName);

        if (content == null)
            return null;

        return new AttachmentDownloadDto
        {
            FileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            Content = content
        };
    }

    public async Task<bool> DeleteAsync(int attachmentId, int userId, string role)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);

        if (attachment == null)
            return false;

        var ticket = await _ticketRepository.GetByIdAsync(attachment.TicketId);

        if (ticket == null || !CanAccessTicket(ticket, userId, role))
            return false;

        await _attachmentRepository.DeleteAsync(attachment);
        await _fileStorageService.DeleteAsync(attachment.StoredFileName);
        await _auditTrailService.RecordAsync(ticket.Id, userId, "AttachmentDeleted", $"Attachment '{attachment.OriginalFileName}' deleted.");

        return true;
    }

    private static bool CanAccessTicket(Ticket ticket, int userId, string role)
    {
        return role switch
        {
            nameof(UserRole.Admin) => true,
            nameof(UserRole.Agent) => ticket.AgentId == userId,
            nameof(UserRole.Requester) => ticket.RequesterId == userId,
            _ => false
        };
    }

    private static AttachmentResponseDto MapAttachment(Attachment attachment)
    {
        return new AttachmentResponseDto
        {
            Id = attachment.Id,
            TicketId = attachment.TicketId,
            UploadedByUserId = attachment.UploadedByUserId,
            OriginalFileName = attachment.OriginalFileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            CreatedAt = attachment.CreatedAt
        };
    }
}
