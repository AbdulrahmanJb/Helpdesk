using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.DTOs;

public class CreateCommentDto
{
    [Required]
    [StringLength(1000, MinimumLength = 2)]
    public string Content { get; set; } = string.Empty;

    public bool IsInternal { get; set; }
}
