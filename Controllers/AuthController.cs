using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeopleOS.Api.Data;

namespace PeopleOS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(PeopleOsDbContext db) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(x =>
            x.Email.ToLower() == request.Email.ToLower() && x.Password == request.Password);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid demo credentials." });
        }

        var employee = await db.Employees.FindAsync(user.EmployeeId);
        return Ok(new
        {
            token = $"demo-token-{user.Id}",
            user.Email,
            user.Role,
            employee
        });
    }
}

public record LoginRequest(string Email, string Password);
