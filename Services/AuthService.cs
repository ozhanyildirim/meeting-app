using MeetingApp.Context;
using MeetingApp.Models;
using MeetingApp.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext db, TokenService tokenService, IEmailService emailService)
    {
        _db = db;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task RegisterAsync(RegisterDto dto)
    {
        if (_db.Users.Any(u => u.Email == dto.Email))
            throw new Exception("Bu email zaten kayıtlı");

        byte[]? profileImage = null;
        if (!string.IsNullOrEmpty(dto.ProfileImage))
        {
            var allowedTypes = new[] { "data:image/jpg", "data:image/jpeg", "data:image/png" };

            if (!allowedTypes.Any(t => dto.ProfileImage.StartsWith(t)))
                throw new Exception("Profil resmi sadece .jpg, .jpeg, .png olabilir");

            var base64 = dto.ProfileImage.Contains(",")
                ? dto.ProfileImage.Split(',')[1]
                : dto.ProfileImage;

            byte[] imageBytes = Convert.FromBase64String(base64);

            if (imageBytes.Length > 2 * 1024 * 1024)
                throw new Exception("Profil resmi 2MB'dan büyük olamaz");

            profileImage = imageBytes;
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            ProfileImage = profileImage,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);
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