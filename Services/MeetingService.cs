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
            .Where(m => m.UserId == userId && !m.IsCancelled)
            .Select(m => new MeetingResponse
            {
                Id = m.Id,
                Title = m.Title,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Description = m.Description,
                DocumentPath = m.DocumentPath,
                IsCancelled = m.IsCancelled,
                CancelledAt = m.CancelledAt
            })
            .ToListAsync();
    }

    public async Task<MeetingResponse?> GetByIdAsync(int id, int userId)
    {
        var meeting = await _db.Meetings
            .Where(m => m.Id == id && m.UserId == userId)
            .Select(m => new MeetingResponse
            {
                Id = m.Id,
                Title = m.Title,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Description = m.Description,
                DocumentPath = m.DocumentPath,
                IsCancelled = m.IsCancelled,
                CancelledAt = m.CancelledAt
            })
            .FirstOrDefaultAsync();

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        return meeting;
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
            IsCancelled = meeting.IsCancelled
        };
    }

    public async Task<MeetingResponse> UpdateAsync(int id, MeetingDto dto, int userId)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        if (meeting.IsCancelled)
            throw new Exception("İptal edilmiş toplantı güncellenemez");

        meeting.Title = dto.Title;
        meeting.StartDate = dto.StartDate;
        meeting.EndDate = dto.EndDate;
        meeting.Description = dto.Description;

        await _db.SaveChangesAsync();

        return new MeetingResponse
        {
            Id = meeting.Id,
            Title = meeting.Title,
            StartDate = meeting.StartDate,
            EndDate = meeting.EndDate,
            Description = meeting.Description,
            IsCancelled = meeting.IsCancelled
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