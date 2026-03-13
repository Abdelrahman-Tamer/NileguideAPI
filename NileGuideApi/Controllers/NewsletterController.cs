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
    [Route("api/newsletter")]
    [ApiController]
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

        [HttpPost("subscribe")]
        [AllowAnonymous]
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

        [HttpPost("unsubscribe")]
        [AllowAnonymous]
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

        [HttpPost("send")]
        [Authorize(Policy = "AdminOnly")]
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

            using var throttler = new SemaphoreSlim(maxParallelSends);

            var sendTasks = subscribers.Select(async email =>
            {
                await throttler.WaitAsync(HttpContext.RequestAborted);

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
                    _logger.LogError(ex, "Failed to send newsletter to {Email}", email);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(sendTasks);

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
