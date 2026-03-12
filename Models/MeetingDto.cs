namespace MeetingApp.Models
{
    public class MeetingDto
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public DocumentUploadDto? Document { get; set; }

    }
}
