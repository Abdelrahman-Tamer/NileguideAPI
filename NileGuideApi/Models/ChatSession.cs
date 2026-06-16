namespace NileGuideApi.Models
{
    // One chat conversation for a specific authenticated user.
    public class ChatSession
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string Title { get; set; } = "New chat";

        public string Messages { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
