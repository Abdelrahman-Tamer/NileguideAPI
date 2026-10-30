namespace NileGuideApi.Options
{
    public sealed class MiniRagOptions
    {
        public const string SectionName = "MiniRag";

        public string BaseUrl { get; set; } = "https://mini-rag-alpha.vercel.app";

        private string? _adminApiKey;

        public string? ApiKey { get; set; }

        public string? AdminApiKey
        {
            get => _adminApiKey ?? ApiKey;
            set => _adminApiKey = value;
        }
    }
}
