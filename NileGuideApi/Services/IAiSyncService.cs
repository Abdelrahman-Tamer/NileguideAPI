namespace NileGuideApi.Services
{
    public interface IAiSyncService
    {
        Task SyncActivityAsync(int activityId, string title, string city, string category, decimal price, string currency, string description, string imageUrl, bool isActive, string action = "upsert");
        Task DeleteActivityAsync(int activityId);
        Task DeleteConversationHistoryAsync(string conversationId);
    }
}
