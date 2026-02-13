using MailKit.Net.Smtp;
using MimeKit;

namespace NileGuideApi.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_config["EmailSettings:FromName"], _config["EmailSettings:FromEmail"]));
            mimeMessage.To.Add(new MailboxAddress("", email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("plain") { Text = message };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:SmtpPort"]!), false);
            await client.AuthenticateAsync(_config["EmailSettings:SmtpUsername"], _config["EmailSettings:SmtpPassword"]);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }
    }
}