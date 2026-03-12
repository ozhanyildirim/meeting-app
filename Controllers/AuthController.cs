using MeetingApp.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task Register([FromBody] RegisterDto dto)
    {
        await _authService.RegisterAsync(dto);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}