using MeetingApp.Models;

namespace MeetingApp.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string firstName);
        Task SendMeetingNotificationAsync(string toEmail, string firstName, Meeting meeting);
    }
}
