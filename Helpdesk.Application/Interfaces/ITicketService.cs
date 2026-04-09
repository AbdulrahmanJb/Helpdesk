using Helpdesk.Application.DTOs;
namespace Helpdesk.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto dto, int requesterId);

    Task<List<TicketResponseDto>> GetAllTicketsAsync(int userId, string role);

    Task<TicketResponseDto?> GetTicketByIdAsync(int id, int userId, string role);

    Task<UpdateTicketStatusResult> UpdateTicketStatusAsync(int id, UpdateTicketStatusDto dto, int userId, string role);

    Task<bool> AssignTicketAsync(int ticketId, AssignTicketDto dto);
}
