using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Helpdesk.Domain.Entities;
using Helpdesk.Domain.Enums;

namespace Helpdesk.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IAuditTrailService _auditTrailService;
    private readonly INotificationService _notificationService;

    public CommentService(ICommentRepository commentRepository, ITicketRepository ticketRepository, IAuditTrailService auditTrailService, INotificationService notificationService)
    {
        _commentRepository = commentRepository;
        _ticketRepository = ticketRepository;
        _auditTrailService = auditTrailService;
        _notificationService = notificationService;
    }

    public async Task<List<CommentResponseDto>?> GetTicketCommentsAsync(int ticketId, int userId, string role)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket == null || !CanAccessTicket(ticket, userId, role))
            return null;

        var comments = await _commentRepository.GetByTicketIdAsync(ticketId);

        if (role == nameof(UserRole.Requester))
            comments = comments.Where(comment => !comment.IsInternal).ToList();

        return comments
            .OrderBy(comment => comment.CreatedAt)
            .Select(MapComment)
            .ToList();
    }

    public async Task<CreateCommentResponseDto> CreateCommentAsync(int ticketId, CreateCommentDto dto, int userId, string role)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);

        if (ticket == null)
            return new CreateCommentResponseDto { Result = CreateCommentResult.NotFound };

        if (!CanAccessTicket(ticket, userId, role))
            return new CreateCommentResponseDto { Result = CreateCommentResult.Forbidden };

        if (dto.IsInternal && role == nameof(UserRole.Requester))
            return new CreateCommentResponseDto { Result = CreateCommentResult.Forbidden };

        var comment = await _commentRepository.CreateAsync(new Comment
        {
            TicketId = ticketId,
            AuthorId = userId,
            Content = dto.Content,
            IsInternal = dto.IsInternal,
            CreatedAt = DateTime.UtcNow
        });
        await _auditTrailService.RecordAsync(
            ticketId,
            userId,
            "CommentAdded",
            dto.IsInternal ? "Internal comment added." : "Public comment added.");
        await NotifyCommentRecipientsAsync(ticket, userId, dto.IsInternal);

        return new CreateCommentResponseDto
        {
            Result = CreateCommentResult.Success,
            Comment = MapComment(comment)
        };
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

    private async Task NotifyCommentRecipientsAsync(Ticket ticket, int actorId, bool isInternal)
    {
        if (isInternal)
        {
            if (ticket.AgentId.HasValue && ticket.AgentId.Value != actorId)
            {
                await _notificationService.NotifyAsync(
                    ticket.AgentId.Value,
                    ticket.Id,
                    "Internal comment added",
                    $"A new internal comment was added to ticket #{ticket.Id}.");
            }

            return;
        }

        if (ticket.RequesterId != actorId)
        {
            await _notificationService.NotifyAsync(
                ticket.RequesterId,
                ticket.Id,
                "New comment on your ticket",
                $"A new comment was added to ticket #{ticket.Id}.");
        }

        if (ticket.AgentId.HasValue && ticket.AgentId.Value != actorId)
        {
            await _notificationService.NotifyAsync(
                ticket.AgentId.Value,
                ticket.Id,
                "New comment on assigned ticket",
                $"A new comment was added to ticket #{ticket.Id}.");
        }
    }

    private static CommentResponseDto MapComment(Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            TicketId = comment.TicketId,
            AuthorId = comment.AuthorId,
            Content = comment.Content,
            IsInternal = comment.IsInternal,
            CreatedAt = comment.CreatedAt
        };
    }
}
