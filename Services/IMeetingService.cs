using MeetingApp.Models;

namespace MeetingApp.Services
{
    public interface IMeetingService
    {
        Task<List<Meeting>> GetAllAsync(int userId);
        Task<Meeting?> GetByIdAsync(int id, int userId);
        Task<Meeting> CreateAsync(MeetingDto dto, int userId);
        Task<Meeting> UpdateAsync(int id, MeetingDto dto, int userId);
        Task DeleteAsync(int id, int userId);
        Task CancelAsync(int id, int userId);

    }
}
