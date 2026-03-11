using MeetingApp.Context;
using MeetingApp.Models;
using MeetingApp.Services;
using Microsoft.EntityFrameworkCore;

public class MeetingService : IMeetingService
{

    private readonly AppDbContext _db;

    public MeetingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Meeting>> GetAllAsync(int userId)
    {
        return await _db.Meetings
            .Where(m => m.UserId == userId && !m.IsCancelled)
            .ToListAsync();
    }

    public async Task<Meeting?> GetByIdAsync(int id, int userId)
    {
        var meeting = await _db.Meetings
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (meeting == null)
            throw new Exception("Toplantı bulunamadı");

        return meeting;
    }

    public async Task<Meeting> CreateAsync(MeetingDto dto, int userId)
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

        return meeting;
    }

    public async Task<Meeting> UpdateAsync(int id, MeetingDto dto, int userId)
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

        return meeting;
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