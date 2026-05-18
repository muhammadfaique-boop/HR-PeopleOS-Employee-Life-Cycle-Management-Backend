using Microsoft.AspNetCore.Mvc;
using PeopleOS.Api.Application.DTOs;
using PeopleOS.Api.Application.Interfaces;

namespace PeopleOS.Api.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var session = await authService.LoginAsync(request);
        return session is null ? Unauthorized(new { message = "Invalid demo credentials." }) : Ok(session);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
    {
        var changed = await authService.ChangePasswordAsync(request);
        return changed
            ? Ok(new { message = "Password changed successfully." })
            : BadRequest(new { message = "Current password is not correct." });
    }
}
