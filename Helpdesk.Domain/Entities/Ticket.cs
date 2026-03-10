using Helpdesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public int RequesterId { get; set; }

    public int? AgentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}