using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NileGuideApi.Options;

namespace NileGuideApi.Services
{
    // Sends transactional emails through the configured SMTP server.
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettingsOptions _emailSettings;

        public EmailSender(IOptions<EmailSettingsOptions> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string plainTextMessage,
            string? htmlMessage = null)
        {
            var mime = new MimeMessage();
            mime.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail!));
            mime.To.Add(MailboxAddress.Parse(email));
            mime.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = plainTextMessage
            };

            if (!string.IsNullOrWhiteSpace(htmlMessage))
            {
                bodyBuilder.HtmlBody = htmlMessage;
            }

            mime.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer!, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername!, _emailSettings.SmtpPassword!);
            await client.SendAsync(mime);
            await client.DisconnectAsync(true);
        }
    }
}
