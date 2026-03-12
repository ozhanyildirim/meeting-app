using MeetingApp.Models;

namespace MeetingApp.Services
{
    public interface IMeetingService
    {
        Task<List<MeetingResponse>> GetAllAsync(int userId);
        Task<MeetingResponse?> GetByIdAsync(int id, int userId);
        Task<MeetingResponse> CreateAsync(MeetingDto dto, int userId);
        Task<MeetingResponse> UpdateAsync(int id, MeetingDto dto, int userId);
        Task DeleteAsync(int id, int userId);
        Task CancelAsync(int id, int userId);
    }
}
