namespace Helpdesk.Application.DTOs;

public enum UpdateTicketStatusResult
{
    Success = 0,
    NotFound = 1,
    Forbidden = 2,
    InvalidTransition = 3
}
