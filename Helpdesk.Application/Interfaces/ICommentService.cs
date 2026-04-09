using Helpdesk.Application.DTOs;

namespace Helpdesk.Application.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponseDto>?> GetTicketCommentsAsync(int ticketId, int userId, string role);

    Task<CreateCommentResponseDto> CreateCommentAsync(int ticketId, CreateCommentDto dto, int userId, string role);
}
