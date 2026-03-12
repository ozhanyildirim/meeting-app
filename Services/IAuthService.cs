using MeetingApp.Models;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
}