namespace NileGuideApi.Models
{
    // Ownership mapping between an authenticated user and an AI-managed conversation.
    public class ChatConversation
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public string ConversationId { get; set; } = string.Empty;
    }
}
