namespace MeetingApp.Models
{
    public class RegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string? ProfileImage { get; set; } 

    }

    public class LoginDto
    {
        [System.ComponentModel.DefaultValue("ozhan@test.com")]
        public string Email { get; set; }

        [System.ComponentModel.DefaultValue("Test123!")]
        public string Password { get; set; }
    }
}
