using Helpdesk.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.DTOs;

public class UpdateTicketStatusDto
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus Status { get; set; }
}
