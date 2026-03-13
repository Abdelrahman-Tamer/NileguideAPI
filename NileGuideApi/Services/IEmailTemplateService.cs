namespace NileGuideApi.Services
{
    // Produces branded email bodies for the app's outbound messages.
    public interface IEmailTemplateService
    {
        EmailTemplateContent BuildPasswordResetCodeEmail(string code, TimeSpan expiresIn);
        EmailTemplateContent BuildNewsletterEmail(string subject, string body);
    }
}
