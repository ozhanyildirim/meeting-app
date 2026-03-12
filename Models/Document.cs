namespace MeetingApp.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileContent { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; }
    }

    public class DocumentUploadDto
    {
        public string FileName { get; set; }
        public string Base64Content { get; set; }
    }

    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Base64Content { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}