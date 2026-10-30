using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NileGuideApi.Controllers;
using Xunit;

namespace NileGuideApi.Tests
{
    public class AdminAuthorizationIntegrationTests
    {
        private readonly IAuthorizationService _authorizationService;

        public AdminAuthorizationIntegrationTests()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            var serviceProvider = services.BuildServiceProvider();
            _authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        }

        [Fact]
        public async Task AnonymousUser_IsRejectedByAdminOnlyPolicy()
        {
            // Anonymous user has no identity and no claims
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

            var result = await _authorizationService.AuthorizeAsync(anonymous, "AdminOnly");

            Assert.False(result.Succeeded);
            Assert.Contains(result.Failure.FailedRequirements, r => r is Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement);
        }

        [Fact]
        public async Task TouristUser_IsRejectedByAdminOnlyPolicy()
        {
            // Authenticated user with "Tourist" role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user-tourist-123"),
                new Claim(ClaimTypes.Name, "tourist_traveler"),
                new Claim(ClaimTypes.Role, "Tourist")
            };
            var touristIdentity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation");
            var tourist = new ClaimsPrincipal(touristIdentity);

            var result = await _authorizationService.AuthorizeAsync(tourist, "AdminOnly");

            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task AdminUser_IsAuthorizedByAdminOnlyPolicy()
        {
            // Authenticated user with "Admin" role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-user-001"),
                new Claim(ClaimTypes.Name, "system_administrator"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var adminIdentity = new ClaimsIdentity(claims, "AuthenticationTypes.Federation");
            var admin = new ClaimsPrincipal(adminIdentity);

            var result = await _authorizationService.AuthorizeAsync(admin, "AdminOnly");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task UserWithMultipleRoles_IncludingAdmin_IsAuthorized()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "superadmin-001"),
                new Claim(ClaimTypes.Role, "Tourist"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var multiRoleIdentity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(multiRoleIdentity);

            var result = await _authorizationService.AuthorizeAsync(principal, "AdminOnly");

            Assert.True(result.Succeeded);
        }

        [Fact]
        public void AdminAiController_RequiresAdminOnlyPolicy()
        {
            var controllerType = typeof(AdminAiController);
            var attributes = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

            var authAttr = Assert.Single(attributes) as AuthorizeAttribute;
            Assert.NotNull(authAttr);
            Assert.Equal("AdminOnly", authAttr.Policy);
        }

        [Fact]
        public async Task ControllerContext_TouristPrincipal_EvaluationRejectsAccess()
        {
            var touristIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-456"),
                new Claim(ClaimTypes.Role, "Tourist")
            }, "TestAuth");
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(touristIdentity)
            };

            var controller = new AdminAiController(new MockAiAdminService())
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            // Evaluate authorization for this controller's user against AdminOnly
            var authResult = await _authorizationService.AuthorizeAsync(controller.User, "AdminOnly");
            Assert.False(authResult.Succeeded);
        }

        [Fact]
        public async Task ControllerContext_AdminPrincipal_EvaluationGrantsAccess()
        {
            var adminIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-789"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth");
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(adminIdentity)
            };

            var controller = new AdminAiController(new MockAiAdminService())
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };

            var authResult = await _authorizationService.AuthorizeAsync(controller.User, "AdminOnly");
            Assert.True(authResult.Succeeded);
        }
    }
}
