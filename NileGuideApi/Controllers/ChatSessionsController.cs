using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.Models;
using NileGuideApi.Services;
using System.Security.Claims;
using System.Text.Json;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Authenticated user's chat sessions and messages.
    /// </summary>
    [Authorize]
    [Route("api/chat/sessions")]
    [ApiController]
    [Produces("application/json")]
    public class ChatSessionsController : ControllerBase
    {
        private static readonly JsonSerializerOptions MessageJsonOptions = new(JsonSerializerDefaults.Web);

        private readonly AppDbContext _context;
        private readonly IChatAiService? _chatAiService;

        public ChatSessionsController(AppDbContext context, IChatAiService? chatAiService = null)
        {
            _context = context;
            _chatAiService = chatAiService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSession(CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var session = new ChatSession
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                Title = "New chat",
                Messages = "[]",
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { sessionId = session.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var sessions = await _context.ChatSessions
                .AsNoTracking()
                .Where(session => session.UserId == userId.Value)
                .OrderByDescending(session => session.CreatedAt)
                .ThenByDescending(session => session.Id)
                .Select(session => new
                {
                    id = session.Id,
                    title = session.Title,
                    createdAt = session.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Ok(sessions);
        }

        [HttpPost("{id:guid}/message")]
        public async Task<IActionResult> SendMessage(Guid id, [FromBody] JsonElement request, CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            if (!TryGetContent(request, out var content))
                return BadRequest(new { message = "Content is required" });

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(
                    item => item.Id == id && item.UserId == userId.Value,
                    cancellationToken);

            if (session == null)
                return NotFound(new { message = "Chat session not found" });

            if (_chatAiService == null)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Chat AI service is not configured" });

            var messages = DeserializeMessages(session.Messages);
            var isFirstMessage = messages.Count == 0;

            messages.Add(new ChatMessage
            {
                Role = "user",
                Content = content
            });

            var reply = await _chatAiService.GetReplyAsync(messages, cancellationToken);

            messages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = reply ?? string.Empty
            });

            if (isFirstMessage)
                session.Title = BuildTitle(content);

            session.Messages = JsonSerializer.Serialize(messages, MessageJsonOptions);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { reply });
        }

        private int? GetAuthenticatedUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var userId) ? userId : null;
        }

        private static bool TryGetContent(JsonElement request, out string content)
        {
            content = string.Empty;

            if (request.ValueKind != JsonValueKind.Object ||
                !request.TryGetProperty("content", out var contentProperty) ||
                contentProperty.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            content = contentProperty.GetString()?.Trim() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(content);
        }

        private static List<ChatMessage> DeserializeMessages(string messagesJson)
        {
            if (string.IsNullOrWhiteSpace(messagesJson))
                return new List<ChatMessage>();

            return JsonSerializer.Deserialize<List<ChatMessage>>(messagesJson, MessageJsonOptions)
                ?? new List<ChatMessage>();
        }

        private static string BuildTitle(string content)
        {
            var title = content.Trim();
            return title.Length <= 50 ? title : title[..50];
        }
    }
}
