using NileGuideApi.Models;

namespace NileGuideApi.Services
{
    public interface IChatAiService
    {
        Task<string> GetReplyAsync(IReadOnlyList<ChatMessage> messages, CancellationToken cancellationToken = default);
    }
}
