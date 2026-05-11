namespace NileGuideApi.Options
{
    public sealed class SecurityOptions
    {
        public const string SectionName = "Security";

        public string? ResetCodePepper { get; set; }
    }
}
