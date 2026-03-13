namespace NileGuideApi.Services
{
    // Small abstraction so email delivery can be replaced or mocked in tests.
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string plainTextMessage, string? htmlMessage = null);
    }
}
