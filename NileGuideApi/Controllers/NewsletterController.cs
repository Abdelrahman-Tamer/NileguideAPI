using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NileGuideApi.Data;
using NileGuideApi.DTOs;
using NileGuideApi.Models;
using NileGuideApi.Services;

namespace NileGuideApi.Controllers
{
    /// <summary>
    /// Newsletter subscription endpoints and admin newsletter sending.
    /// </summary>
    [Route("api/newsletter")]
    [ApiController]
    [Produces("application/json")]
    public class NewsletterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ILogger<NewsletterController> _logger;

        public NewsletterController(
            AppDbContext context,
            IEmailSender emailSender,
            IEmailTemplateService emailTemplateService,
            ILogger<NewsletterController> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _logger = logger;
        }

        /// <summary>
        /// Subscribes an email address to the newsletter.
        /// </summary>
        /// <remarks>
        /// If the email already exists but is inactive, the subscription is reactivated.
        /// </remarks>
        /// <param name="dto">Subscriber email address.</param>
        /// <returns>A subscription status message.</returns>
        /// <response code="200">The email is subscribed, already subscribed, or reactivated.</response>
        /// <response code="400">Returned when the request body fails validation.</response>
        [HttpPost("subscribe")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeNewsletterDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            var now = DateTime.UtcNow;

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(x => x.Email == email);

            if (subscriber != null)
            {
                if (subscriber.IsActive)
                {
                    return Ok(new { message = "Email is already subscribed" });
                }

                subscriber.IsActive = true;
                subscriber.SubscribedAt = now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Subscription reactivated" });
            }

            var newSubscriber = new NewsletterSubscriber
            {
                Email = email,
                SubscribedAt = now,
                IsActive = true
            };

            _context.NewsletterSubscribers.Add(newSubscriber);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Subscribed successfully" });
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _context.Entry(newSubscriber).State = EntityState.Detached;

                var existingSubscriber = await _context.NewsletterSubscribers
                    .FirstOrDefaultAsync(x => x.Email == email);

                if (existingSubscriber == null)
                {
                    throw;
                }

                if (existingSubscriber.IsActive)
                {
                    return Ok(new { message = "Email is already subscribed" });
                }

                existingSubscriber.IsActive = true;
                existingSubscriber.SubscribedAt = now;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Subscription reactivated" });
            }
        }

        /// <summary>
        /// Unsubscribes an email address from the newsletter.
        /// </summary>
        /// <remarks>
        /// The response is intentionally generic so callers cannot enumerate subscribed emails.
        /// </remarks>
        /// <param name="dto">Subscriber email address.</param>
        /// <returns>A generic unsubscribe status message.</returns>
        /// <response code="200">The request was accepted.</response>
        /// <response code="400">Returned when the request body fails validation.</response>
        [HttpPost("unsubscribe")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeNewsletterDto dto)
        {
            var email = NormalizeEmail(dto.Email);

            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(x => x.Email == email);

            if (subscriber != null && subscriber.IsActive)
            {
                subscriber.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "If the email is subscribed, it has been unsubscribed." });
        }

        /// <summary>
        /// Sends a newsletter campaign to all active subscribers.
        /// </summary>
        /// <remarks>
        /// Requires the authenticated user to satisfy the AdminOnly authorization policy.
        /// </remarks>
        /// <param name="dto">Newsletter subject and body.</param>
        /// <returns>Counts for targeted, sent, and failed newsletter emails.</returns>
        /// <response code="200">The send operation completed and returns delivery counts.</response>
        /// <response code="400">Returned when the request body fails validation.</response>
        /// <response code="401">Returned when the bearer token is missing or invalid.</response>
        /// <response code="403">Returned when the authenticated user is not an admin.</response>
        [HttpPost("send")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(NewsletterSendResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Send([FromBody] SendNewsletterDto dto)
        {
            var subscribers = await _context.NewsletterSubscribers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => x.Email)
                .ToListAsync();

            if (subscribers.Count == 0)
            {
                return Ok(new
                {
                    totalSubscribers = 0,
                    sentCount = 0,
                    failedCount = 0
                });
            }

            var emailContent = _emailTemplateService.BuildNewsletterEmail(dto.Subject, dto.Body);
            var sentCount = 0;
            var failedCount = 0;
            const int maxParallelSends = 5;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxParallelSends
            };

            await Parallel.ForEachAsync(subscribers, parallelOptions, async (email, _) =>
            {
                try
                {
                    await _emailSender.SendEmailAsync(
                        email,
                        dto.Subject,
                        emailContent.PlainTextBody,
                        emailContent.HtmlBody);

                    Interlocked.Increment(ref sentCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failedCount);
                    _logger.LogError(ex, "Failed to send newsletter message");
                }
            });

            return Ok(new
            {
                totalSubscribers = subscribers.Count,
                sentCount,
                failedCount
            });
        }

        private static string NormalizeEmail(string email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException { Number: 2601 or 2627 };
        }
    }
}
