namespace NileGuideApi.Options
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public double ExpiryMinutes { get; set; }
        public double AccessTokenMinutes { get; set; }
        public double RefreshTokenDays { get; set; }
        public double RefreshTokenRememberMeDays { get; set; }
    }
}
