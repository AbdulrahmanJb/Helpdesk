using System;
using System.Collections.Generic;
using System.Text;
using Helpdesk.Domain.Entities;


namespace Helpdesk.Application.Interfaces;

public interface ITicketService
{
    IEnumerable<Ticket> GetTickets();
}