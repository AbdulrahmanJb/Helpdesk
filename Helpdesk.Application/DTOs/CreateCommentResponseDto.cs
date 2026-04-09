namespace Helpdesk.Application.DTOs;

public class CreateCommentResponseDto
{
    public CreateCommentResult Result { get; set; }

    public CommentResponseDto? Comment { get; set; }
}
