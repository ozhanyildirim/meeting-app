using MeetingApp.Context;
using MeetingApp.Models;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthService(AppDbContext db, TokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<string> RegisterAsync(RegisterDto dto)
    {
        if (_db.Users.Any(u => u.Email == dto.Email))
            throw new Exception("Bu email zaten kayıtlı");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return "Kayıt başarılı";
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            throw new Exception("Kullanıcı bulunamadı");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new Exception("Şifre hatalı");

        return _tokenService.GenerateToken(user);
    }
}