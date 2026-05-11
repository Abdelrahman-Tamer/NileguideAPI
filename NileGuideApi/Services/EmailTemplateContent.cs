namespace NileGuideApi.Services
{
    // Holds the plain-text and HTML versions of the same email.
    public sealed record EmailTemplateContent(string PlainTextBody, string HtmlBody);
}
