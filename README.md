<div align="center">

![NileGuide API](https://capsule-render.vercel.app/api?type=waving&height=180&color=0:0f172a,45:0369a1,100:14b8a6&text=NileGuide%20API&fontColor=ffffff&fontSize=44&fontAlignY=35&desc=ASP.NET%20Core%20backend%20for%20a%20modern%20Egypt%20travel%20platform&descAlignY=58&animation=fadeIn)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-0A7EA4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-EF%20Core-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![JWT](https://img.shields.io/badge/Auth-JWT-111827?style=for-the-badge&logo=jsonwebtokens&logoColor=white)](https://jwt.io/)
[![Swagger](https://img.shields.io/badge/API-Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=111827)](https://swagger.io/)

![Typing](https://readme-typing-svg.demolab.com?font=Inter&weight=600&size=22&duration=2500&pause=900&color=14B8A6&center=true&vCenter=true&width=720&lines=Travel+discovery+API;JWT+authentication+and+refresh+tokens;Activities%2C+plans%2C+wishlists%2C+reviews;Admin+dashboard+and+SignalR+updates)

</div>

## Overview

NileGuide API is the backend service for the NileGuide travel platform. It powers activity discovery, user authentication, profile preferences, wishlists, trip planning, reviews, reporting, newsletters, admin workflows, and realtime dashboard updates.

The project is built as a production-oriented ASP.NET Core 8 Web API with typed configuration validation, Entity Framework Core migrations, SQL Server persistence, JWT bearer authentication, rate limiting, CORS, Swagger documentation, centralized exception handling, and Docker support.

## Highlights

| Capability | Details |
| --- | --- |
| Authentication | Registration, login, logout, refresh tokens, current user endpoint, password reset code flow |
| Discovery | Activities, categories, cities, filtering, details, images, opening hours, booking links |
| User features | Profiles, profile pictures, wishlists, trip plan items, reviews |
| Admin features | Activity management, user management, dashboard metrics, reports, newsletter sending |
| Platform services | SQL Server, EF Core migrations, JWT, MailKit SMTP, Cloudinary, SignalR |
| Developer experience | Swagger UI, XML docs, typed options validation, Dockerfile, clean repository rules |

## Tech Stack

```text
Runtime       .NET 8 / ASP.NET Core Web API
Database      SQL Server / Azure SQL
ORM           Entity Framework Core 8
Auth          JWT bearer tokens + refresh token rotation
Docs          Swagger / OpenAPI
Realtime      SignalR
Email         MailKit SMTP
Media         Cloudinary
Deployment    Docker-ready ASP.NET Core container
```

## Project Structure

```text
NileGuideApi/
  Controllers/      HTTP endpoints grouped by product area
  Data/             AppDbContext and design-time EF factory
  DTOs/             Request and response contracts
  Hubs/             SignalR hubs
  Middleware/       Centralized API exception handling
  Migrations/       EF Core database migrations
  Models/           Domain entities
  Options/          Typed configuration objects
  Services/         Business logic and integrations
  Swagger/          Swagger filters and examples
```

## API Areas

| Area | Routes |
| --- | --- |
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/refresh`, `POST /api/auth/logout` |
| Password reset | `POST /api/auth/forgot-password`, `POST /api/auth/verify-reset-code`, `POST /api/auth/reset-password` |
| Activities | `GET /api/activities`, `GET /api/activities/{id}` |
| Reviews | `GET /api/activities/{activityId}/reviews`, `POST /api/activities/{activityId}/reviews` |
| Lookups | `GET /api/categories`, `GET /api/cities` |
| Wishlist | `GET /api/wishlist`, `GET /api/wishlist/activity-ids`, `POST /api/wishlist/{activityId}`, `DELETE /api/wishlist/{activityId}` |
| Plan | `GET /api/plan`, `POST /api/plan/items`, `DELETE /api/plan/items/{planItemId}` |
| Profile | `GET /api/users/me/profile`, `PUT /api/users/me/profile`, profile picture upload/delete |
| Chat | `POST /api/chat/conversations`, `GET /api/chat/conversations`, `DELETE /api/chat/conversations/{conversationId}` |
| Admin | `api/admin/activities`, `api/users`, `api/dashboard`, `api/reports/*`, `api/newsletter/send` |

## Requirements

- .NET SDK `8.0.420` or a compatible .NET 8 SDK
- SQL Server or Azure SQL
- EF Core CLI tools
- SMTP account for email flows
- Cloudinary account for profile image uploads

Install EF Core tools if needed:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The API validates required configuration at startup. Keep secrets out of committed files and use user secrets locally or environment variables in hosted environments.

Required settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=NileGuideDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "replace-with-a-strong-secret-at-least-32-bytes",
    "Issuer": "NileGuideApi",
    "Audience": "NileGuideClient",
    "AccessTokenMinutes": 30,
    "RefreshTokenDays": 1,
    "RefreshTokenRememberMeDays": 30
  },
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "username",
    "SmtpPassword": "password",
    "FromEmail": "no-reply@example.com",
    "FromName": "NileGuide"
  },
  "Security": {
    "ResetCodePepper": "replace-with-a-strong-pepper-at-least-32-bytes"
  },
  "Cloudinary": {
    "CloudName": "cloud-name",
    "ApiKey": "api-key",
    "ApiSecret": "api-secret"
  }
}
```

Local user-secrets example:

```bash
cd NileGuideApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=NileGuideDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "replace-with-a-strong-secret-at-least-32-bytes"
dotnet user-secrets set "Security:ResetCodePepper" "replace-with-a-strong-pepper-at-least-32-bytes"
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.example.com"
dotnet user-secrets set "EmailSettings:SmtpUsername" "username"
dotnet user-secrets set "EmailSettings:SmtpPassword" "password"
dotnet user-secrets set "EmailSettings:FromEmail" "no-reply@example.com"
dotnet user-secrets set "Cloudinary:CloudName" "cloud-name"
dotnet user-secrets set "Cloudinary:ApiKey" "api-key"
dotnet user-secrets set "Cloudinary:ApiSecret" "api-secret"
```

## Run Locally

Restore and build:

```bash
dotnet restore NileGuideApi/NileGuideApi.csproj
dotnet build NileGuideApi/NileGuideApi.csproj
```

Apply database migrations:

```bash
dotnet ef database update --project NileGuideApi/NileGuideApi.csproj
```

Run the API:

```bash
dotnet run --project NileGuideApi/NileGuideApi.csproj
```

Open Swagger from the URL printed by the app:

```text
/swagger
```

The application also runs pending EF Core migrations on startup when a valid database connection is configured.

## Authentication

Protected endpoints require a bearer token:

```http
Authorization: Bearer {accessToken}
```

Admin-only endpoints use the `AdminOnly` authorization policy, which requires the `Admin` role.

## CORS

The API currently allows these frontend origins:

```text
http://localhost:4200
http://127.0.0.1:4200
https://nileguide.online
https://www.nileguide.online
```

## Docker

Build and run the API container:

```bash
cd NileGuideApi
docker build -t nileguide-api .
docker run -p 8080:8080 --env-file .env nileguide-api
```

The container listens on port `8080`.

## Quality Notes

- Central exception middleware keeps API error responses consistent.
- Request validation returns a lightweight `{ message, errors }` response shape.
- Rate limiting is enabled for registration, login, refresh, and reset flows.
- Forwarded headers are supported for trusted reverse-proxy deployments.
- Local artifacts, build outputs, IDE state, secrets, and scratch files are ignored.

<div align="center">

![Footer](https://capsule-render.vercel.app/api?type=waving&height=120&section=footer&color=0:14b8a6,100:0f172a)

</div>
