using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NileGuideApi.Swagger
{
    // Applies the Bearer security requirement only to endpoints that are actually protected.
    public sealed class SwaggerAuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                return;

            var allowsAnonymous =
                actionDescriptor.MethodInfo.GetCustomAttributes(inherit: true).OfType<AllowAnonymousAttribute>().Any() ||
                actionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true).OfType<AllowAnonymousAttribute>().Any();

            if (allowsAnonymous)
                return;

            var requiresAuthorization =
                actionDescriptor.MethodInfo.GetCustomAttributes(inherit: true).OfType<AuthorizeAttribute>().Any() ||
                actionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true).OfType<AuthorizeAttribute>().Any();

            if (!requiresAuthorization)
                return;

            operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse
            {
                Description = "Unauthorized"
            });

            operation.Responses.TryAdd(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse
            {
                Description = "Forbidden"
            });

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }
}
