using System.Net;
using System.Text.Json;

namespace NileGuideApi.Middleware
    {
    // Converts unhandled exceptions into the simple JSON error shape used by the API.
    public class ApiExceptionMiddleware
        {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiExceptionMiddleware> _logger;

        public ApiExceptionMiddleware( RequestDelegate next, ILogger<ApiExceptionMiddleware> logger )
            {
            _next = next;
            _logger = logger;
            }

        public async Task Invoke( HttpContext context )
            {
            try
                {
                await _next(context);
                }
            catch ( Exception ex )
                {
                _logger.LogError(ex, "Unhandled exception");

                if ( context.Response.HasStarted )
                    {
                    _logger.LogWarning("The response has already started, so the exception middleware will rethrow the exception.");
                    throw;
                    }

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var body = JsonSerializer.Serialize(new { message = "Server error" });
                await context.Response.WriteAsync(body);
                }
            }
        }
    }
