namespace NileGuideApi.Options
{
    public sealed class ConnectionStringsOptions
    {
        public const string SectionName = "ConnectionStrings";

        public string? DefaultConnection { get; set; }
    }
}
