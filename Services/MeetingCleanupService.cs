using MeetingApp.Context;
using Microsoft.EntityFrameworkCore;

public class MeetingCleanupService
{
    private readonly AppDbContext _db;

    public MeetingCleanupService(AppDbContext db)
    {
        _db = db;
    }

    public async Task DeleteCancelledMeetingsAsync()
    {
        // İptal edilmiş ve üzerinden 24 saat geçmiş toplantıları sil
        var cutoff = DateTime.UtcNow.AddMinutes(-1); // 1 dakika önce iptal edilenleri sil

       // var cutoff = DateTime.UtcNow.AddHours(-24);

        var toDelete = await _db.Meetings
            .Where(m => m.IsCancelled && m.CancelledAt < cutoff)
            .ToListAsync();

        if (toDelete.Any())
        {
            _db.Meetings.RemoveRange(toDelete);
            await _db.SaveChangesAsync();
            // Trigger otomatik devreye girer → MeetingDeleteLogs'a yazar ✅
        }
    }
}