using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NileGuideApi.Swagger
{
    // Adds concrete examples so Swagger shows actual API messages instead of only "string".
    public sealed class SwaggerResponseExamplesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;
            var path = "/" + (context.ApiDescription.RelativePath ?? string.Empty).Split('?')[0].Trim('/');
            path = path.ToLowerInvariant();

            ApplyNoBodyDescriptions(operation);
            ApplyGenericValidationExample(operation);
            ApplyValidationExamples(operation, method, path);
            ApplyEndpointExamples(operation, method, path);
        }

        private static void ApplyNoBodyDescriptions(OpenApiOperation operation)
        {
            if (operation.Responses.TryGetValue("401", out var unauthorized))
            {
                unauthorized.Description =
                    "Missing, expired, or invalid Bearer token. JWT middleware usually returns 401 with no JSON body before the controller runs.";
            }

            if (operation.Responses.TryGetValue("403", out var forbidden))
            {
                forbidden.Description =
                    "Authenticated user does not have the required role or policy. Authorization middleware usually returns 403 with no JSON body.";
            }

            if (operation.Responses.TryGetValue("429", out var tooManyRequests))
            {
                tooManyRequests.Description =
                    "Rate limit exceeded. The current rate limiter returns HTTP 429 with no JSON body.";
                tooManyRequests.Content.Clear();
            }
        }

        private static void ApplyGenericValidationExample(OpenApiOperation operation)
        {
            if (!operation.Responses.TryGetValue("400", out var badRequest))
                return;

            AddJsonExample(
                badRequest,
                "validationFailed",
                "Validation failed",
                new OpenApiObject
                {
                    ["message"] = new OpenApiString("Validation failed"),
                    ["errors"] = new OpenApiObject
                    {
                        ["fieldName"] = new OpenApiString("Validation error message")
                    }
                });
        }

        private static void ApplyValidationExamples(OpenApiOperation operation, string method, string path)
        {
            switch (method, path)
            {
                case ("GET", "/api/activities"):
                    AddValidationErrors(
                        operation,
                        "invalidActivityFilters",
                        ("Search", "Search must be at most 100 characters"),
                        ("SortBy", "SortBy must be one of: default, priceLowToHigh, priceHighToLow, name"),
                        ("Page", "Page must be between 1 and 10000"),
                        ("PageSize", "PageSize must be between 1 and 50"),
                        ("CategoryIds", "CategoryIds must contain positive values only"),
                        ("CityIds", "CityIds must contain positive values only"));
                    break;

                case ("GET", "/api/wishlist"):
                    AddValidationErrors(
                        operation,
                        "invalidWishlistFilters",
                        ("Page", "Page must be between 1 and 10000"),
                        ("PageSize", "PageSize must be between 1 and 50"));
                    break;

                case ("POST", "/api/auth/register"):
                    AddValidationErrors(
                        operation,
                        "invalidRegisterBody",
                        ("Email", "Email is required"),
                        ("Password", "Password must be at least 8 characters and include letters and numbers"),
                        ("FullName", "FullName must be at least 2 characters"),
                        ("Nationality", "Nationality is required"),
                        ("DateOfBirth", "DateOfBirth cannot be in the future"));
                    break;

                case ("POST", "/api/auth/login"):
                    AddValidationErrors(
                        operation,
                        "invalidLoginBody",
                        ("Email", "Email format is invalid"),
                        ("Password", "Password is required"));
                    break;

                case ("POST", "/api/auth/refresh"):
                    AddValidationErrors(
                        operation,
                        "invalidRefreshBody",
                        ("RefreshToken", "RefreshToken is required"));
                    break;

                case ("POST", "/api/auth/forgot-password"):
                    AddValidationErrors(
                        operation,
                        "invalidForgotPasswordBody",
                        ("Email", "Invalid email format"));
                    break;

                case ("POST", "/api/auth/verify-reset-code"):
                    AddValidationErrors(
                        operation,
                        "invalidVerifyResetCodeBody",
                        ("Email", "Invalid email format"),
                        ("Code", "Code must be 6 digits"));
                    break;

                case ("POST", "/api/auth/reset-password"):
                    AddValidationErrors(
                        operation,
                        "invalidResetPasswordBody",
                        ("Email", "Email format is invalid"),
                        ("Code", "Code must be 6 digits"),
                        ("NewPassword", "NewPassword must be at least 8 characters and include letters and numbers"));
                    break;

                case ("POST", "/api/newsletter/subscribe"):
                case ("POST", "/api/newsletter/unsubscribe"):
                    AddValidationErrors(
                        operation,
                        "invalidNewsletterBody",
                        ("Email", "Email format is invalid"));
                    break;

                case ("POST", "/api/newsletter/send"):
                    AddValidationErrors(
                        operation,
                        "invalidSendNewsletterBody",
                        ("Subject", "Subject is required"),
                        ("Body", "Body must be at most 10000 characters"));
                    break;

                case ("POST", "/api/plan/items"):
                    AddValidationErrors(
                        operation,
                        "invalidPlanItemBody",
                        ("ActivityId", "Activity id must be positive"),
                        ("StartTime", "StartTime must be in HH:mm format"));
                    break;

                case ("POST", "/api/users/me/profile-picture"):
                    AddMessage(operation, "400", "imageRequired", "Image is required");
                    AddMessage(operation, "400", "imageTooLarge", "Image must be at most 5MB");
                    AddMessage(operation, "400", "invalidImageType", "Image must be jpg, png, or webp");
                    break;
            }
        }

        private static void ApplyEndpointExamples(OpenApiOperation operation, string method, string path)
        {
            switch (method, path)
            {
                case ("GET", "/api/activities/{id}"):
                    AddMessage(operation, "400", "invalidActivityId", "Activity id must be positive");
                    AddMessage(operation, "404", "activityNotFound", "Activity not found");
                    break;

                case ("POST", "/api/auth/register"):
                    AddMessage(operation, "409", "emailExists", "Email already exists");
                    break;

                case ("POST", "/api/auth/login"):
                    AddMessage(operation, "401", "invalidCredentials", "Invalid credentials");
                    break;

                case ("GET", "/api/auth/me"):
                    AddMessage(operation, "401", "invalidToken", "Invalid token");
                    AddMessage(operation, "401", "userNotFound", "User not found");
                    break;

                case ("POST", "/api/auth/refresh"):
                    AddMessage(operation, "401", "invalidRefreshToken", "Invalid refresh token");
                    break;

                case ("POST", "/api/auth/logout"):
                    AddMessage(operation, "200", "loggedOut", "Logged out");
                    AddMessage(operation, "401", "invalidToken", "Invalid token");
                    break;

                case ("POST", "/api/auth/forgot-password"):
                    AddMessage(operation, "200", "resetCodeAccepted", "If the email exists, a reset code was sent.");
                    break;

                case ("POST", "/api/auth/verify-reset-code"):
                    AddMessage(operation, "200", "codeValid", "Code is valid");
                    AddMessage(operation, "400", "invalidCode", "Invalid code");
                    break;

                case ("POST", "/api/auth/reset-password"):
                    AddMessage(operation, "200", "passwordUpdated", "Password updated");
                    AddMessage(operation, "400", "invalidCode", "Invalid code");
                    AddMessage(operation, "400", "samePassword", "New password cannot be the same as the old password");
                    break;

                case ("POST", "/api/newsletter/subscribe"):
                    AddMessage(operation, "200", "subscribed", "Subscribed successfully");
                    AddMessage(operation, "200", "alreadySubscribed", "Email is already subscribed");
                    AddMessage(operation, "200", "reactivated", "Subscription reactivated");
                    break;

                case ("POST", "/api/newsletter/unsubscribe"):
                    AddMessage(operation, "200", "unsubscribeAccepted", "If the email is subscribed, it has been unsubscribed.");
                    break;

                case ("POST", "/api/wishlist/{activityid}"):
                    AddMessage(operation, "200", "added", "Activity added to wishlist");
                    AddMessage(operation, "200", "alreadyExists", "Activity already in wishlist");
                    AddMessage(operation, "400", "invalidActivityId", "Activity id must be positive");
                    AddMessage(operation, "404", "activityNotFound", "Activity not found");
                    break;

                case ("DELETE", "/api/wishlist/{activityid}"):
                    AddMessage(operation, "200", "removed", "Activity removed from wishlist");
                    AddMessage(operation, "400", "invalidActivityId", "Activity id must be positive");
                    break;

                case ("GET", "/api/wishlist/status/{activityid}"):
                    AddMessage(operation, "400", "invalidActivityId", "Activity id must be positive");
                    AddMessage(operation, "404", "activityNotFound", "Activity not found");
                    break;

                case ("POST", "/api/plan/items"):
                    AddMessage(operation, "400", "invalidActivityId", "Activity id must be positive");
                    AddMessage(operation, "400", "missingScheduledDate", "ScheduledDate is required");
                    AddMessage(operation, "400", "invalidStartTime", "StartTime must be in HH:mm format");
                    AddMessage(operation, "404", "activityNotFound", "Activity not found");
                    break;

                case ("DELETE", "/api/plan/items/{planitemid}"):
                    AddMessage(operation, "400", "invalidPlanItemId", "Plan item id must be positive");
                    AddMessage(operation, "404", "planItemNotFound", "Plan item not found");
                    break;

                case ("POST", "/api/users/me/profile-picture"):
                    AddMessage(operation, "401", "invalidToken", "Invalid token");
                    AddMessage(operation, "404", "userNotFound", "User not found");
                    AddMessage(operation, "502", "cloudinaryFailed", "Profile picture upload failed");
                    AddMessage(operation, "503", "cloudinaryNotConfigured", "Cloudinary is not configured");
                    break;
            }
        }

        private static void AddMessage(OpenApiOperation operation, string statusCode, string exampleName, string message)
        {
            if (!operation.Responses.TryGetValue(statusCode, out var response))
                return;

            AddJsonExample(
                response,
                exampleName,
                message,
                new OpenApiObject
                {
                    ["message"] = new OpenApiString(message)
                });
        }

        private static void AddValidationErrors(
            OpenApiOperation operation,
            string exampleName,
            params (string Field, string Message)[] errors)
        {
            if (!operation.Responses.TryGetValue("400", out var response))
                return;

            var errorObject = new OpenApiObject();
            foreach (var (field, message) in errors)
                errorObject[field] = new OpenApiString(message);

            AddJsonExample(
                response,
                exampleName,
                "Validation failed",
                new OpenApiObject
                {
                    ["message"] = new OpenApiString("Validation failed"),
                    ["errors"] = errorObject
                });
        }

        private static void AddJsonExample(
            OpenApiResponse response,
            string exampleName,
            string summary,
            IOpenApiAny value)
        {
            if (!response.Content.TryGetValue("application/json", out var mediaType))
            {
                mediaType = new OpenApiMediaType();
                response.Content["application/json"] = mediaType;
            }

            mediaType.Examples[exampleName] = new OpenApiExample
            {
                Summary = summary,
                Value = value
            };
        }
    }
}
