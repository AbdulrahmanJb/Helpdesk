using System.ComponentModel.DataAnnotations;

namespace Helpdesk.Application.DTOs;

public class AssignTicketDto
{
    [Range(1, int.MaxValue)]
    public int AgentId { get; set; }
}
