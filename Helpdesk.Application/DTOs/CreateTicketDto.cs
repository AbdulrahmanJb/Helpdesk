using Helpdesk.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.DTOs;

public class CreateTicketDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; }
}
