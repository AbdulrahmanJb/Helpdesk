using Helpdesk.Application.DTOs;

namespace Helpdesk.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
}