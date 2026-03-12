namespace MeetingApp.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? DocumentPath { get; set; }
        public bool IsCancelled { get; set; } = false;
        public DateTime? CancelledAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }

    public class MeetingResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? DocumentPath { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }
    }

}
