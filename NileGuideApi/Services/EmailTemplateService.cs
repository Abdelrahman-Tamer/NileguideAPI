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
            var expiryMinutes = Math.Max(1, (int)Math.Round(expiresIn.TotalMinutes));
            var safeCode = HtmlEncoder.Default.Encode(code);
            var safeProjectUrl = HtmlEncoder.Default.Encode(projectUrl);
            var safePreviewText = HtmlEncoder.Default.Encode($"Your NileGuide verification code is {code}.");

            var plainTextBody =
                $"NileGuide Password Reset Code{Environment.NewLine}{Environment.NewLine}" +
                $"Use this code to reset your password:{Environment.NewLine}{Environment.NewLine}" +
                $"{code}{Environment.NewLine}{Environment.NewLine}" +
                $"This code expires in {expiryMinutes} minutes.{Environment.NewLine}{Environment.NewLine}" +
                $"If you did not request this, ignore this email.{Environment.NewLine}{Environment.NewLine}" +
                $"Open NileGuide: {projectUrl}";
            
            var htmlBody = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                  <meta http-equiv="x-ua-compatible" content="ie=edge" />
                  <title>Reset your password</title>
                  <style>
                    @media only screen and (max-width: 600px) {
                      .email-shell {
                        border-radius: 20px !important;
                      }

                      .email-content {
                        padding: 28px 18px 36px 18px !important;
                      }

                      .email-column {
                        max-width: 100% !important;
                      }

                      .email-title {
                        font-size: 24px !important;
                        line-height: 30px !important;
                      }

                      .email-copy {
                        font-size: 14px !important;
                        line-height: 24px !important;
                        padding-bottom: 22px !important;
                      }

                      .code-panel {
                        padding: 18px 16px 18px 16px !important;
                      }

                      .code-value {
                        font-size: 32px !important;
                        line-height: 38px !important;
                        letter-spacing: 4px !important;
                      }

                      .cta-button {
                        display: inline-block !important;
                        width: auto !important;
                      }
                    }
                  </style>
                </head>
                <body style="margin:0;padding:0;background-color:#0f0d0a;">
                  <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">
                    {{safePreviewText}}
                  </div>
                  <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0;padding:32px 12px;background-color:#0f0d0a;">
                    <tr>
                      <td align="center">
                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" class="email-shell" style="max-width:820px;background-color:#12100c;border-radius:28px;overflow:hidden;">
                          <tr>
                            <td align="center" style="padding:22px 24px 0 24px;">
                              <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:14px;line-height:18px;font-weight:800;letter-spacing:0.18em;text-transform:uppercase;color:{{Gold}};">
                                {{BrandName}}
                              </div>
                            </td>
                          </tr>
                          <tr>
                            <td class="email-content" style="padding:42px 24px 56px 24px;">
                              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" class="email-column" style="max-width:346px;margin:0 auto;">
                                <tr>
                                  <td class="email-title" style="padding:0 0 12px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:30px;line-height:38px;font-weight:800;color:{{Heading}};">
                                    Reset your password
                                  </td>
                                </tr>
                                <tr>
                                  <td class="email-copy" style="padding:0 0 28px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:30px;color:{{TextPrimary}};">
                                    Use this code to reset your password.
                                  </td>
                                </tr>
                                <tr>
                                  <td>
                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" class="code-panel" style="background-color:#2b2821;border-radius:6px;">
                                      <tr>
                                        <td style="padding:22px 24px 20px 24px;">
                                          <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%">
                                            <tr>
                                              <td style="padding:0 0 16px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:11px;line-height:16px;font-weight:800;letter-spacing:0.16em;text-transform:uppercase;color:{{Gold}};">
                                                Verification code
                                              </td>
                                            </tr>
                                            <tr>
                                              <td class="code-value" style="font-family:'Courier New',Consolas,monospace;font-size:36px;line-height:42px;font-weight:800;letter-spacing:6px;white-space:nowrap;color:#ffffff;user-select:all;-webkit-user-select:all;">
                                                {{safeCode}}
                                              </td>
                                            </tr>
                                            <tr>
                                              <td colspan="2" style="padding:18px 0 0 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:10px;line-height:16px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;color:#d8c79d;">
                                                &#9716;&nbsp; Expires in {{expiryMinutes}} minutes
                                              </td>
                                            </tr>
                                          </table>
                                        </td>
                                      </tr>
                                    </table>
                                  </td>
                                </tr>
                                <tr>
                                  <td style="padding:30px 0 20px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:28px;color:{{TextPrimary}};">
                                    If this wasn't you, ignore this email.
                                  </td>
                                </tr>
                                <tr>
                                  <td>
                                    <a href="{{safeProjectUrl}}" class="cta-button" style="display:inline-block;padding:16px 22px;background-color:#f6bf32;border-radius:12px;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:22px;font-weight:800;color:#111111;text-decoration:none;">
                                      Explore NileGuide&nbsp;&nbsp;&rarr;
                                    </a>
                                  </td>
                                </tr>
                              </table>
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

        public EmailTemplateContent BuildNewsletterEmail(string subject, string body)
        {
            var projectUrl = GetProjectUrl();
            var safeProjectUrl = HtmlEncoder.Default.Encode(projectUrl);
            var safePreviewText = HtmlEncoder.Default.Encode(subject);
            var safeTitle = HtmlEncoder.Default.Encode(subject);
            var contentHtml = BuildNewsletterBody(body);

            var plainTextBody =
                $"{subject}{Environment.NewLine}{Environment.NewLine}" +
                $"{NormalizeLineEndings(body)}{Environment.NewLine}{Environment.NewLine}" +
                $"Explore NileGuide: {projectUrl}";

            var htmlBody = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                  <meta http-equiv="x-ua-compatible" content="ie=edge" />
                  <title>{{safeTitle}}</title>
                  <style>
                    @media only screen and (max-width: 600px) {
                      .email-shell {
                        border-radius: 20px !important;
                      }

                      .email-content {
                        padding: 28px 18px 36px 18px !important;
                      }

                      .email-column {
                        max-width: 100% !important;
                      }

                      .email-title {
                        font-size: 24px !important;
                        line-height: 30px !important;
                      }

                      .email-copy {
                        font-size: 14px !important;
                        line-height: 24px !important;
                        padding-bottom: 22px !important;
                      }

                      .cta-button {
                        display: inline-block !important;
                        width: auto !important;
                      }
                    }
                  </style>
                </head>
                <body style="margin:0;padding:0;background-color:#0f0d0a;">
                  <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">
                    {{safePreviewText}}
                  </div>
                  <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="margin:0;padding:32px 12px;background-color:#0f0d0a;">
                    <tr>
                      <td align="center">
                        <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" class="email-shell" style="max-width:820px;background-color:#12100c;border-radius:28px;overflow:hidden;">
                          <tr>
                            <td align="center" style="padding:22px 24px 0 24px;">
                              <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:14px;line-height:18px;font-weight:800;letter-spacing:0.18em;text-transform:uppercase;color:{{Gold}};">
                                {{BrandName}}
                              </div>
                            </td>
                          </tr>
                          <tr>
                            <td class="email-content" style="padding:42px 24px 56px 24px;">
                              <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" class="email-column" style="max-width:346px;margin:0 auto;">
                                <tr>
                                  <td class="email-title" style="padding:0 0 12px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:30px;line-height:38px;font-weight:800;color:{{Heading}};">
                                    {{safeTitle}}
                                  </td>
                                </tr>
                                <tr>
                                  <td class="email-copy" style="padding:0 0 28px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:30px;color:{{TextPrimary}};">
                                    Here's what's new on NileGuide.
                                  </td>
                                </tr>
                                <tr>
                                  <td style="padding:0 0 10px 0;">
                                    <table role="presentation" cellpadding="0" cellspacing="0" border="0" width="100%" style="background-color:#2b2821;border-radius:6px;">
                                      <tr>
                                        <td style="padding:22px 24px 18px 24px;">
                                          <div style="font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:11px;line-height:16px;font-weight:800;letter-spacing:0.16em;text-transform:uppercase;color:{{Gold}};padding:0 0 16px 0;">
                                            Newsletter update
                                          </div>
                                          {{contentHtml}}
                                        </td>
                                      </tr>
                                    </table>
                                  </td>
                                </tr>
                                <tr>
                                  <td style="padding:18px 0 20px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:28px;color:{{TextPrimary}};">
                                    Open NileGuide to see more.
                                  </td>
                                </tr>
                                <tr>
                                  <td>
                                    <a href="{{safeProjectUrl}}" class="cta-button" style="display:inline-block;padding:16px 22px;background-color:#f6bf32;border-radius:12px;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:22px;font-weight:800;color:#111111;text-decoration:none;">
                                      Explore NileGuide&nbsp;&nbsp;&rarr;
                                    </a>
                                  </td>
                                </tr>
                              </table>
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

        private static string BuildNewsletterBody(string body)
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
                        <p style="margin:0 0 14px 0;font-family:Inter,'Segoe UI',Arial,sans-serif;font-size:15px;line-height:28px;color:#f5efe1;">
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
