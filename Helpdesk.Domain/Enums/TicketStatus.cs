using System;
using System.Collections.Generic;
using System.Text;

namespace Helpdesk.Domain.Enums
{
    public enum TicketStatus
    {
        New = 0,
        Assigned = 1,
        InProgress = 2,
        Resolved = 3,
        Closed = 4
    }
}
