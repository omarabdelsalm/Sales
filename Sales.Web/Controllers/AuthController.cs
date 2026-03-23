using Microsoft.AspNetCore.Mvc;
using Sales.Shared.Models;
using Sales.Shared.Services;

namespace Sales.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IDataService _data;

    public AuthController(IDataService data)
    {
        _data = data;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AppUser>> Login(LoginRequest request)
    {
        var user = await _data.LoginAsync(request.Email, request.Password);
        if (user == null) return Unauthorized();
        return user;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterRequest request)
    {
        var (success, message) = await _data.RegisterAsync(request.FullName, request.Email, request.Password, request.Role, request.StoreName);
        return Ok(new { Success = success, Message = message });
    }

    [HttpDelete("account/{userId}")]
    public async Task<ActionResult> DeleteAccount(int userId)
    {
        var (success, message) = await _data.DeleteAccountAsync(userId);
        return Ok(new { Success = success, Message = message });
    }

    [HttpPost("subscribe/{userId}")]
    public async Task<ActionResult> Subscribe(int userId, [FromBody] SubscribeRequest request)
    {
        var success = await _data.UpdateSubscriptionAsync(userId, request.Months);
        return success ? Ok() : BadRequest();
    }

    public class LoginRequest { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class RegisterRequest { public string FullName { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; public UserRole Role { get; set; } public string? StoreName { get; set; } }
    public class SubscribeRequest { public int Months { get; set; } }
}
