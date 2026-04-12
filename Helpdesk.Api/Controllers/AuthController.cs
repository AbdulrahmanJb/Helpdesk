using Helpdesk.Api.Extensions;
using Helpdesk.Application.DTOs;
using Helpdesk.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result == null)
            return this.ApiUnauthorized("Invalid email or password.");

        return Ok(result);
    }
}
