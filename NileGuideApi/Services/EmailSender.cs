using MailKit.Security;
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
            var fromName = _config["EmailSettings:FromName"] ?? "NileGuide";
            var fromEmail = _config["EmailSettings:FromEmail"] ?? throw new InvalidOperationException("EmailSettings:FromEmail missing");
            var server = _config["EmailSettings:SmtpServer"] ?? throw new InvalidOperationException("EmailSettings:SmtpServer missing");
            var username = _config["EmailSettings:SmtpUsername"] ?? throw new InvalidOperationException("EmailSettings:SmtpUsername missing");
            var password = _config["EmailSettings:SmtpPassword"] ?? throw new InvalidOperationException("EmailSettings:SmtpPassword missing");

            var portRaw = _config["EmailSettings:SmtpPort"] ?? "587";
            if (!int.TryParse(portRaw, out var port)) port = 587;

            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(fromName, fromEmail));
            mime.To.Add(MailboxAddress.Parse(email));
            mime.Subject = subject;
            mime.Body = new TextPart("plain") { Text = message };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(server, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(mime);
            await client.DisconnectAsync(true);
        }
    }
}
