using Microsoft.AspNetCore.Mvc;
using PeopleOS.Api.Models.Dtos;
using PeopleOS.Api.Services;

namespace PeopleOS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginRequest request)
    {
        var session = await authService.LoginAsync(request);
        return session is null ? Unauthorized(new { message = "Invalid demo credentials." }) : Ok(session);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var changed = await authService.ChangePasswordAsync(request);
        return changed
            ? Ok(new { message = "Password changed successfully." })
            : BadRequest(new { message = "Current password is not correct." });
    }
}
