using Helpdesk.Application.DTOs;
using Helpdesk.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;


namespace Helpdesk.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto);

    Task<List<TicketResponseDto>> GetAllTicketsAsync();

    Task<TicketResponseDto?> GetTicketByIdAsync(int id);

    Task<bool> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto);

    Task<bool> AssignTicketAsync(int ticketId, AssignTicketDto dto);
}
