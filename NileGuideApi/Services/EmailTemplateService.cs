using System.Text.Encodings.Web;

namespace NileGuideApi.Services
{
    // Builds lightweight HTML emails that stay readable across major mail clients.
    public class EmailTemplateService : IEmailTemplateService
    {
        private const string BrandName = "NileGuide";
        private const string OuterBackground = "#100d09";
        private const string CardBackground = "#1b1915";
        private const string SurfaceBackground = "#15120e";
        private const string Gold = "#eab308";
        private const string Border = "#5f4a12";
        private const string Heading = "#ffffff";
        private const string TextPrimary = "#f5efe1";
        private const string TextMuted = "#c7ba9a";
        private const string Divider = "#3b3224";
        private readonly IConfiguration _config;

        public EmailTemplateService(IConfiguration config)
        {
            _config = config;
        }

        public EmailTemplateContent BuildPasswordResetCodeEmail(string code, TimeSpan expiresIn)
        {
            var projectUrl = GetProjectUrl();
            var safeCode = HtmlEncoder.Default.Encode(code);
            var expiryMinutes = Math.Max(1, (int)Math.Round(expiresIn.TotalMinutes));

            var introHtml = """
                <p style="margin:0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:16px;line-height:28px;color:#f5efe1;">
                  Use this code to reset your password.
                </p>
                """;

            var highlightHtml = $$"""
                <tr>
                  <td style="padding:8px 32px 12px 32px;">
                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background-color:{{SurfaceBackground}};border:1px solid {{Border}};border-radius:14px;">
                      <tr>
                        <td align="center" style="padding:24px 20px 12px 20px;">
                          <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:12px;line-height:18px;font-weight:700;letter-spacing:1.2px;text-transform:uppercase;color:{{Gold}};">Verification code</div>
                          <div style="margin-top:10px;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:34px;line-height:40px;font-weight:800;letter-spacing:8px;color:{{Heading}};">{{safeCode}}</div>
                        </td>
                      </tr>
                      <tr>
                        <td align="center" style="padding:0 20px 24px 20px;">
                          <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:14px;line-height:22px;color:{{TextMuted}};">
                            This code expires in {{expiryMinutes}} minutes.
                          </div>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
                """;

            var contentHtml = """
                <p style="margin:0 0 16px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:26px;color:#c7ba9a;">
                  If you did not request this, you can ignore this email.
                </p>
                """;

            var supportEmail = GetSupportEmail();

            var plainTextBody =
                $"NileGuide Password Reset Code{Environment.NewLine}{Environment.NewLine}" +
                $"Use this code to reset your password:{Environment.NewLine}{Environment.NewLine}" +
                $"{code}{Environment.NewLine}{Environment.NewLine}" +
                $"This code expires in {expiryMinutes} minutes.{Environment.NewLine}{Environment.NewLine}" +
                $"If you did not request this, you can ignore this email.{Environment.NewLine}{Environment.NewLine}" +
                $"{supportEmail}{Environment.NewLine}" +
                $"Open NileGuide: {projectUrl}";

            return BuildLayout(
                previewText: $"Your NileGuide verification code is {code}.",
                eyebrow: "Account Security",
                title: "Reset your password",
                introHtml: introHtml,
                highlightHtml: highlightHtml,
                contentHtml: contentHtml,
                ctaText: "Open NileGuide",
                ctaUrl: projectUrl,
                footerNote: "This mailbox is for transactional messages only. Keep your verification code private.",
                plainTextBody: plainTextBody);
        }

        public EmailTemplateContent BuildNewsletterEmail(string subject, string body)
        {
            var projectUrl = GetProjectUrl();
            var introHtml = string.Empty;
            var supportEmail = GetSupportEmail();

            var contentHtml = BuildParagraphs(body);

            var plainTextBody =
                $"{subject}{Environment.NewLine}{Environment.NewLine}" +
                $"{NormalizeLineEndings(body)}{Environment.NewLine}{Environment.NewLine}" +
                $"{supportEmail}{Environment.NewLine}" +
                $"Explore more on NileGuide: {projectUrl}{Environment.NewLine}{Environment.NewLine}" +
                "You are receiving this email because you subscribed to the NileGuide newsletter.";

            return BuildLayout(
                previewText: subject,
                eyebrow: "Newsletter",
                title: subject,
                introHtml: introHtml,
                highlightHtml: string.Empty,
                contentHtml: contentHtml,
                ctaText: "Explore NileGuide",
                ctaUrl: projectUrl,
                footerNote: "You are receiving this email because you subscribed to NileGuide updates.",
                plainTextBody: plainTextBody);
        }

        private EmailTemplateContent BuildLayout(
            string previewText,
            string eyebrow,
            string title,
            string introHtml,
            string highlightHtml,
            string contentHtml,
            string ctaText,
            string ctaUrl,
            string footerNote,
            string plainTextBody)
        {
            var safePreviewText = HtmlEncoder.Default.Encode(previewText);
            var safeEyebrow = HtmlEncoder.Default.Encode(eyebrow);
            var safeTitle = HtmlEncoder.Default.Encode(title);
            var safeCtaText = HtmlEncoder.Default.Encode(ctaText);
            var safeCtaUrl = HtmlEncoder.Default.Encode(ctaUrl);
            var safeFooterNote = HtmlEncoder.Default.Encode(footerNote);
            var supportEmail = HtmlEncoder.Default.Encode(GetSupportEmail());
            var year = DateTime.UtcNow.Year;

            var htmlBody = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                  <meta http-equiv="x-ua-compatible" content="ie=edge" />
                  <title>{{safeTitle}}</title>
                </head>
                <body style="margin:0;padding:0;background-color:{{OuterBackground}};">
                  <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">
                    {{safePreviewText}}
                  </div>
                  <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0;padding:28px 12px;background-color:{{OuterBackground}};">
                    <tr>
                      <td align="center">
                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="max-width:640px;background-color:{{CardBackground}};border:1px solid {{Border}};border-radius:22px;overflow:hidden;">
                          <tr>
                            <td style="padding:16px 32px;background-color:#000000;border-bottom:1px solid {{Border}};">
                              <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:20px;line-height:26px;font-weight:800;letter-spacing:0.3em;color:{{Gold}};">
                                NILEGUIDE
                              </div>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:32px 32px 12px 32px;">
                              <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:12px;line-height:18px;font-weight:700;letter-spacing:1.4px;text-transform:uppercase;color:{{Gold}};">
                                {{safeEyebrow}}
                              </div>
                              <h1 style="margin:12px 0 16px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:32px;line-height:40px;font-weight:800;color:{{Heading}};">{{safeTitle}}</h1>
                              {{introHtml}}
                            </td>
                          </tr>
                          {{highlightHtml}}
                          <tr>
                            <td style="padding:8px 32px 8px 32px;">
                              {{contentHtml}}
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:12px 32px 18px 32px;">
                              <a href="{{safeCtaUrl}}" style="display:inline-block;padding:14px 24px;background-color:{{Gold}};border-radius:12px;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:22px;font-weight:800;color:#111111;text-decoration:none;">
                                {{safeCtaText}}
                              </a>
                            </td>
                          </tr>
                          <tr>
                            <td style="padding:0 32px 32px 32px;">
                              <div style="padding-top:18px;border-top:1px solid {{Divider}};font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:13px;line-height:22px;color:{{TextMuted}};">
                                {{safeFooterNote}}<br />
                                {{supportEmail}}<br />
                                (c) {{year}} NileGuide. All rights reserved.
                              </div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;

            return new EmailTemplateContent(plainTextBody, htmlBody);
        }

        private static string BuildParagraphs(string body)
        {
            var normalizedBody = NormalizeLineEndings(body);
            var paragraphs = normalizedBody
                .Split($"{Environment.NewLine}{Environment.NewLine}", StringSplitOptions.None)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            if (paragraphs.Count == 0)
            {
                paragraphs.Add(normalizedBody.Trim());
            }

            return string.Join(
                Environment.NewLine,
                paragraphs.Select(p =>
                {
                    var encodedParagraph = HtmlEncoder.Default.Encode(p)
                        .Replace(Environment.NewLine, "<br />");

                    return $"""
                        <p style="margin:0 0 16px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:28px;color:#f5efe1;">
                          {encodedParagraph}
                        </p>
                        """;
                }));
        }

        private string GetProjectUrl()
        {
            return _config["Frontend:BaseUrl"]
                ?? _config["App:PublicUrl"]
                ?? "https://nileguide.online";
        }

        private string GetSupportEmail()
        {
            return _config["EmailSettings:FromEmail"] ?? "nileguide.noreply@gmail.com";
        }

        private static string NormalizeLineEndings(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r\n", Environment.NewLine)
                .Replace("\n", Environment.NewLine);
        }
    }
}
