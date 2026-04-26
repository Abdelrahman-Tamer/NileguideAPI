namespace NileGuideApi.Options
{
    public sealed class EmailSettingsOptions
    {
        public const string SectionName = "EmailSettings";

        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
    }
}
