using MailKit.Net.Smtp;
using MeetingApp.Models;
using MeetingApp.Services;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly MailSettings _mailSettings;

    public EmailService(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string firstName)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
        message.To.Add(new MailboxAddress(firstName, toEmail));
        message.Subject = "Hoş Geldiniz! 🎉";
        message.Body = new TextPart("html")
        {
            Text = $@"
                <h2>Merhaba {firstName},</h2>
                <p>MeetingApp'e hoş geldiniz!</p>
                <p>Artık toplantılarınızı kolayca yönetebilirsiniz.</p>
                <br/>
                <p>İyi çalışmalar,</p>
                <p><strong>MeetingApp Ekibi</strong></p>
            "
        };

        await SendAsync(message);
    }

    public async Task SendMeetingNotificationAsync(string toEmail, string firstName, Meeting meeting)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Email));
        message.To.Add(new MailboxAddress(firstName, toEmail));
        message.Subject = $"Toplantı Oluşturuldu: {meeting.Title}";
        message.Body = new TextPart("html")
        {
            Text = $@"
                <h2>Merhaba {firstName},</h2>
                <p>Yeni bir toplantı oluşturuldu.</p>
                <table>
                    <tr><td><strong>Toplantı:</strong></td><td>{meeting.Title}</td></tr>
                    <tr><td><strong>Başlangıç:</strong></td><td>{meeting.StartDate:dd.MM.yyyy HH:mm}</td></tr>
                    <tr><td><strong>Bitiş:</strong></td><td>{meeting.EndDate:dd.MM.yyyy HH:mm}</td></tr>
                    <tr><td><strong>Açıklama:</strong></td><td>{meeting.Description}</td></tr>
                </table>
                <br/>
                <p>İyi çalışmalar,</p>
                <p><strong>MeetingApp Ekibi</strong></p>
            "
        };

        await SendAsync(message);
    }

    private async Task SendAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_mailSettings.Host, _mailSettings.Port, false);
        await client.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}