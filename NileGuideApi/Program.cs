using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NileGuideApi.Data;
using NileGuideApi.Middleware;
using NileGuideApi.Options;
using NileGuideApi.Services;
using NileGuideApi.Swagger;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

// Build the host and register all API dependencies here.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<ConnectionStringsOptions>()
    .Bind(builder.Configuration.GetSection(ConnectionStringsOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.DefaultConnection), "ConnectionStrings:DefaultConnection is missing")
    .ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key is missing")
    .Validate(o => Encoding.UTF8.GetByteCount(o.Key ?? string.Empty) >= 32, "Jwt:Key must be at least 32 bytes")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is missing")
    .Validate(o => o.AccessTokenMinutes > 0 || o.ExpiryMinutes > 0, "Jwt:AccessTokenMinutes or Jwt:ExpiryMinutes must be greater than zero")
    .Validate(o => o.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be greater than zero")
    .Validate(o => o.RefreshTokenRememberMeDays > 0, "Jwt:RefreshTokenRememberMeDays must be greater than zero")
    .ValidateOnStart();

builder.Services.AddOptions<EmailSettingsOptions>()
    .Bind(builder.Configuration.GetSection(EmailSettingsOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.SmtpServer), "EmailSettings:SmtpServer is missing")
    .Validate(o => o.SmtpPort is > 0 and <= 65535, "EmailSettings:SmtpPort must be between 1 and 65535")
    .Validate(o => !string.IsNullOrWhiteSpace(o.SmtpUsername), "EmailSettings:SmtpUsername is missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.SmtpPassword), "EmailSettings:SmtpPassword is missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.FromEmail), "EmailSettings:FromEmail is missing")
    .Validate(o => !string.IsNullOrWhiteSpace(o.FromName), "EmailSettings:FromName is missing")
    .ValidateOnStart();

builder.Services.AddOptions<SecurityOptions>()
    .Bind(builder.Configuration.GetSection(SecurityOptions.SectionName))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ResetCodePepper), "Security:ResetCodePepper is missing")
    .Validate(o => Encoding.UTF8.GetByteCount(o.ResetCodePepper ?? string.Empty) >= 32, "Security:ResetCodePepper must be at least 32 bytes")
    .ValidateOnStart();

builder.Services.Configure<CloudinaryOptions>(
    builder.Configuration.GetSection(CloudinaryOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing");

// The main EF Core context for users, reset tokens, and newsletter subscribers.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connectionString));

// Trust proxy headers so redirects, auth, and rate limiting see the real client address.
var trustedProxyAddresses = builder.Configuration
    .GetSection("ForwardedHeaders:KnownProxies")
    .Get<string[]>()
    ?.Select(value => IPAddress.TryParse(value, out var address) ? address : null)
    .Where(address => address != null)
    .Cast<IPAddress>()
    .ToArray()
    ?? Array.Empty<IPAddress>();

builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opt.ForwardLimit = 1;

    // Keep ASP.NET Core's safe defaults unless trusted proxies are explicitly configured.
    if (trustedProxyAddresses.Length > 0)
    {
        opt.KnownNetworks.Clear();
        opt.KnownProxies.Clear();

        foreach (var address in trustedProxyAddresses)
            opt.KnownProxies.Add(address);
    }
});

// Frontend clients are restricted to the known Angular/local origins.
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Frontend", p =>
        p.WithOrigins(
            "http://localhost:4200",
            "http://127.0.0.1:4200",
            "https://nileguide.online",
            "https://www.nileguide.online"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

builder.Services.AddControllers();

// Keep validation errors in the same lightweight JSON shape expected by the frontend.
builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                    .First()
            );

        return new BadRequestObjectResult(new
        {
            message = "Validation failed",
            errors
        });
    };
});

// Auth/session services.
builder.Services.AddScoped<IAuthTokenService, AuthTokenService>();

// Email services used by password reset and newsletter flows.
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

// Public discovery and user content services.
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IProfilePictureService, CloudinaryProfilePictureService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportService, ReportService>();
// The frontend already depends on these policy names, so only the internals are tuned.
builder.Services.AddRateLimiter(opt =>
{
    opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opt.AddPolicy("RegisterPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    opt.AddPolicy("LoginPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    opt.AddPolicy("ResetPolicy", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

// JWT auth backs the login/register/refresh flows and the protected endpoints.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        var jwtOptions = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key ?? string.Empty)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Keep the admin policy registered even if only some environments use it.
builder.Services.AddAuthorization(o =>
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));

// Swagger is intentionally public so the frontend team can inspect the deployed API contract.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NileGuide API",
        Version = "v1",
        Description = "Frontend-facing API documentation for NileGuide."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    c.TagActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"];
        return new[] { controller ?? "API" };
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.OperationFilter<SwaggerAuthorizeOperationFilter>();
    c.OperationFilter<SwaggerResponseExamplesOperationFilter>();
});

var app = builder.Build();

// Forwarded headers must run first so the downstream middleware sees the real scheme/IP.
app.UseForwardedHeaders();

// Centralized exception handling keeps server errors in one consistent response shape.
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "NileGuide API Docs";
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NileGuide API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    c.DefaultModelsExpandDepth(-1);
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
    c.EnableFilter();
});

app.UseHttpsRedirection();

app.UseCors("Frontend");

// Apply throttling before auth endpoints execute controller logic.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
