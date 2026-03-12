using MeetingApp.Context;
using MeetingApp.Models;
using MeetingApp.Services;
using Microsoft.EntityFrameworkCore;

public class MeetingService : IMeetingService
{

    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public MeetingService(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task<List<MeetingResponse>> GetAllAsync(int userId)
    {
        return await _db.Meetings
            .Where(m => m.UserId == userId)
            .Select(m => new MeetingResponse
            {
                Id = m.Id,
                Title = m.Title,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Description = m.Description,
                Document = null,
                IsCancelled = m.IsCancelled,
                CancelledAt = m.CancelledAt
            })
            .ToListAsync();
    }

    public async Task<MeetingResponse> GetByIdAsync(int id, int userId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Document)
            .Where(m => m.Id == id && m.UserId == userId)
            .FirstOrDefaultAsync();

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        return new MeetingResponse
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartDate = meeting.StartDate,
            EndDate = meeting.EndDate,
            Description = meeting.Description,
            IsCancelled = meeting.IsCancelled,
            CancelledAt = meeting.CancelledAt,
            Document = meeting.Document == null ? null : new DocumentResponseDto
            {
                Id = meeting.Document.Id,
                FileName = meeting.Document.FileName,
                FileType = meeting.Document.FileType,
                Base64Content = Convert.ToBase64String(meeting.Document.FileContent),
                FileSize = meeting.Document.FileSize,
                UploadedAt = meeting.Document.UploadedAt
            }
        };
    }

    public async Task<MeetingResponse> CreateAsync(MeetingDto dto, int userId)
    {
        if (dto.EndDate <= dto.StartDate)
            throw new Exception("Bitiş tarihi başlangıç tarihinden önce olamaz");

        var meeting = new Meeting
        {
            Title = dto.Title,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description,
            UserId = userId
        };

        _db.Meetings.Add(meeting);
        await _db.SaveChangesAsync();

        // Document kaydet
        if (dto.Document != null)
        {
            var ext = Path.GetExtension(dto.Document.FileName).ToLower();
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

            if (!allowedTypes.Contains(ext))
                throw new Exception($"Desteklenmeyen dosya tipi: {ext}");

            var base64 = dto.Document.Base64Content.Contains(",")
                ? dto.Document.Base64Content.Split(',')[1]
                : dto.Document.Base64Content;

            byte[] fileBytes = Convert.FromBase64String(base64);

            if (fileBytes.Length > 10 * 1024 * 1024)
                throw new Exception("Dosya boyutu 10MB'dan büyük olamaz");

            var document = new Document
            {
                FileName = dto.Document.FileName,
                FileType = ext,
                FileContent = fileBytes,
                FileSize = fileBytes.Length,
                MeetingId = meeting.Id,
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();
            meeting.Document = document;
        }

        var user = await _db.Users.FindAsync(userId);
        if (user != null)
            await _emailService.SendMeetingNotificationAsync(user.Email, user.FirstName, meeting);

        return new MeetingResponse
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartDate = meeting.StartDate,
            EndDate = meeting.EndDate,
            Description = meeting.Description,
            IsCancelled = meeting.IsCancelled,
            Document = meeting.Document == null ? null : new DocumentResponseDto
            {
                Id = meeting.Document.Id,
                FileName = meeting.Document.FileName,
                FileType = meeting.Document.FileType,
                Base64Content = Convert.ToBase64String(meeting.Document.FileContent),
                FileSize = meeting.Document.FileSize,
                UploadedAt = meeting.Document.UploadedAt
            }
        };
    }
    public async Task<MeetingResponse> UpdateAsync(int id, MeetingDto dto, int userId)
    {
        var meeting = await _db.Meetings
            .Include(m => m.Document)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        if (meeting.IsCancelled)
            throw new Exception("İptal edilmiş toplantı güncellenemez");

        meeting.Title = dto.Title;
        meeting.StartDate = dto.StartDate;
        meeting.EndDate = dto.EndDate;
        meeting.Description = dto.Description;

        if (dto.Document == null)
        {
            // Document gönderilmedi, mevcut varsa sil
            if (meeting.Document != null)
            {
                _db.Documents.Remove(meeting.Document);
                meeting.Document = null;
            }
        }
        else
        {
            var ext = Path.GetExtension(dto.Document.FileName).ToLower();
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

            if (!allowedTypes.Contains(ext))
                throw new Exception($"Desteklenmeyen dosya tipi: {ext}");

            var base64 = dto.Document.Base64Content.Contains(",")
                ? dto.Document.Base64Content.Split(',')[1]
                : dto.Document.Base64Content;

            byte[] fileBytes = Convert.FromBase64String(base64);

            if (fileBytes.Length > 10 * 1024 * 1024)
                throw new Exception("Dosya boyutu 10MB'dan büyük olamaz");

            if (meeting.Document != null)
            {
                meeting.Document.FileName = dto.Document.FileName;
                meeting.Document.FileType = ext;
                meeting.Document.FileContent = fileBytes;
                meeting.Document.FileSize = fileBytes.Length;
                meeting.Document.UploadedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Documents.Add(new Document
                {
                    FileName = dto.Document.FileName,
                    FileType = ext,
                    FileContent = fileBytes,
                    FileSize = fileBytes.Length,
                    MeetingId = meeting.Id,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();

        return new MeetingResponse
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartDate = meeting.StartDate,
            EndDate = meeting.EndDate,
            Description = meeting.Description,
            IsCancelled = meeting.IsCancelled,
            Document = meeting.Document == null ? null : new DocumentResponseDto
            {
                Id = meeting.Document.Id,
                FileName = meeting.Document.FileName,
                FileType = meeting.Document.FileType,
                Base64Content = Convert.ToBase64String(meeting.Document.FileContent),
                FileSize = meeting.Document.FileSize,
                UploadedAt = meeting.Document.UploadedAt
            }
        };
    }
    public async Task DeleteAsync(int id, int userId)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        _db.Meetings.Remove(meeting);
        await _db.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, int userId)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        if (meeting.IsCancelled)
            throw new Exception("Toplantı zaten iptal edilmiş");

        meeting.IsCancelled = true;
        meeting.CancelledAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}