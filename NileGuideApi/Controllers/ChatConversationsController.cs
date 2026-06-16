using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.Models;
using System.Security.Claims;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Authenticated user's AI conversation ownership mappings.
    /// </summary>
    [Authorize]
    [Route("api/chat/conversations")]
    [ApiController]
    [Produces("application/json")]
    public class ChatConversationsController : ControllerBase
    {
        private const int MaxConversationIdLength = 200;

        private readonly AppDbContext _context;

        public ChatConversationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SaveConversation([FromBody] SaveChatConversationRequest request, CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var conversationId = request.ConversationId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(conversationId))
                return BadRequest(new { message = "ConversationId is required" });

            if (conversationId.Length > MaxConversationIdLength)
                return BadRequest(new { message = "ConversationId must be at most 200 characters" });

            var exists = await _context.ChatConversations
                .AsNoTracking()
                .AnyAsync(
                    item => item.UserId == userId.Value && item.ConversationId == conversationId,
                    cancellationToken);

            if (exists)
                return Ok(new { conversationId });

            var conversation = new ChatConversation
            {
                UserId = userId.Value,
                ConversationId = conversationId
            };

            _context.ChatConversations.Add(conversation);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _context.Entry(conversation).State = EntityState.Detached;
            }

            return Ok(new { conversationId });
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations(CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            var conversations = await _context.ChatConversations
                .AsNoTracking()
                .Where(item => item.UserId == userId.Value)
                .OrderBy(item => item.ConversationId)
                .Select(item => new
                {
                    conversationId = item.ConversationId
                })
                .ToListAsync(cancellationToken);

            return Ok(conversations);
        }

        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> DeleteConversation([FromRoute] string conversationId, CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token" });

            conversationId = conversationId?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(conversationId))
                return BadRequest(new { message = "ConversationId is required" });

            if (conversationId.Length > MaxConversationIdLength)
                return BadRequest(new { message = "ConversationId must be at most 200 characters" });

            var conversation = await _context.ChatConversations
                .FirstOrDefaultAsync(
                    item => item.UserId == userId.Value && item.ConversationId == conversationId,
                    cancellationToken);

            if (conversation == null)
                return NotFound(new { message = "Conversation not found" });

            _context.ChatConversations.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        private int? GetAuthenticatedUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var userId) ? userId : null;
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }

        public class SaveChatConversationRequest
        {
            public string? ConversationId { get; set; }
        }
    }
}
