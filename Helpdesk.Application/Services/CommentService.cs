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

    public CommentService(ICommentRepository commentRepository, ITicketRepository ticketRepository, IAuditTrailService auditTrailService)
    {
        _commentRepository = commentRepository;
        _ticketRepository = ticketRepository;
        _auditTrailService = auditTrailService;
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
