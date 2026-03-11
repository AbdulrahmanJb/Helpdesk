using Helpdesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Application.DTOs
{
    public class TicketResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; }

        public TicketPriority Priority { get; set; }

        public int RequesterId { get; set; }

        public int? AgentId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
