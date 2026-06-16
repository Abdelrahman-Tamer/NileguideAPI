using System.Text.Json.Serialization;

namespace NileGuideApi.Models
{
    // Persisted chat message shape stored inside ChatSession.Messages.
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
