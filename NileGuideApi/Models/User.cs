namespace NileGuideApi.Models
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;

        public string Role { get; set; } = "Tourist";
    }
}
