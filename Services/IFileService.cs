using MeetingApp.Models;

namespace MeetingApp.Services
{
    public interface IFileService
    {
        Task<Document> UploadDocumentAsync(DocumentUploadDto dto, int meetingId);
        Task<string> UploadProfileImageAsync(string base64, string fileName);
        void Delete(string? filePath);
    }
}
