using Helpdesk.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Application.DTOs
{
    public class UpdateTicketStatusDto
    {
        public TicketStatus Status { get; set; }
    }
}
