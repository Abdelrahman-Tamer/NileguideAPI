<div align="center">

![NileGuide API](https://capsule-render.vercel.app/api?type=waving&height=220&color=0:0F172A,35:0369A1,70:14B8A6,100:F59E0B&text=NileGuide%20API&fontColor=ffffff&fontSize=52&fontAlignY=34&desc=Secure%20travel%20discovery%20backend%20for%20Egypt%20experiences&descAlignY=55&descSize=18&animation=fadeIn)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-0F172A?style=for-the-badge&logo=dotnet&logoColor=14B8A6)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-0369A1?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![EF Core](https://img.shields.io/badge/EF%20Core-SQL%20Server-14B8A6?style=for-the-badge&logo=microsoftsqlserver&logoColor=0F172A)](https://learn.microsoft.com/ef/core/)
[![JWT](https://img.shields.io/badge/JWT-Secure%20Auth-F59E0B?style=for-the-badge&logo=jsonwebtokens&logoColor=0F172A)](https://jwt.io/)
[![Docker](https://img.shields.io/badge/Docker-Ready-0369A1?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)

![Typing](https://readme-typing-svg.demolab.com?font=Inter&weight=700&size=24&duration=2400&pause=750&color=14B8A6&center=true&vCenter=true&width=850&lines=Explore+Egypt+activities+with+a+clean+API;JWT+auth+with+refresh+token+rotation;Wishlists%2C+plans%2C+reviews%2C+profiles;Admin+reports+with+SignalR+dashboard+updates;Swagger-first+developer+experience)

[![Status](https://img.shields.io/badge/status-active-14B8A6?style=flat-square)](#overview)
[![API Docs](https://img.shields.io/badge/docs-swagger-F59E0B?style=flat-square)](#api-surface)
[![Database](https://img.shields.io/badge/database-sql%20server-0369A1?style=flat-square)](#configuration)
[![Repository](https://img.shields.io/badge/repository-source%20only-0F172A?style=flat-square)](#quality-bar)

<a href="#overview">Overview</a> |
<a href="#feature-map">Features</a> |
<a href="#architecture">Architecture</a> |
<a href="#api-surface">API</a> |
<a href="#run-locally">Run Locally</a> |
<a href="#docker">Docker</a>

</div>

![Divider](https://capsule-render.vercel.app/api?type=rect&height=4&color=0:0F172A,35:0369A1,70:14B8A6,100:F59E0B)

## Overview

NileGuide API is the ASP.NET Core backend behind the NileGuide travel platform. It provides the product layer for activity discovery, authentication, user profiles, wishlists, trip planning, reviews, reports, newsletters, and admin operations.

The codebase is designed as a production-facing Web API: typed options are validated at startup, EF Core migrations manage the SQL Server schema, JWT bearer auth protects private flows, rate limiting guards sensitive endpoints, Swagger documents the contract, and SignalR streams dashboard updates.

## Brand Palette

| Token | Color | Usage |
| --- | --- | --- |
| Nile Midnight | `#0F172A` | Primary dark surface, security, admin tone |
| Nile Blue | `#0369A1` | API/navigation identity |
| Oasis Teal | `#14B8A6` | Success, realtime, discovery highlights |
| Sun Gold | `#F59E0B` | Premium accent and callouts |

## Feature Map

| Product Area | What It Handles |
| --- | --- |
| Authentication | Register, login, logout, refresh tokens, current user, password reset code flow |
| Discovery | Activities, categories, cities, filters, details, images, hours, booking links |
| User Workspace | Profile preferences, profile pictures, wishlists, trip plan items |
| Social Proof | Activity reviews, reviewer metadata, rating aggregation support |
| Admin Console | Activity CRUD, user management, dashboard metrics, reports, newsletter send |
| Platform Services | SQL Server, EF Core migrations, JWT, MailKit, Cloudinary, SignalR, Swagger |

## Technology

<div align="center">

[![Tech](https://skillicons.dev/icons?i=dotnet,cs,azure,docker,git,github)](https://skillicons.dev)

</div>

| Layer | Stack |
| --- | --- |
| Runtime | .NET 8, ASP.NET Core Web API |
| Persistence | SQL Server or Azure SQL, Entity Framework Core 8 |
| Security | JWT bearer tokens, refresh tokens, role-based admin policy |
| Realtime | SignalR dashboard hub |
| Email | MailKit SMTP |
| Media | Cloudinary profile picture uploads |
| Documentation | Swagger / OpenAPI with XML comments and operation filters |
| Deployment | Docker-ready ASP.NET Core container |

## Architecture

```mermaid
flowchart LR
    Client["Web Client"] --> API["ASP.NET Core API"]
    API --> Auth["JWT Auth + Rate Limits"]
    API --> Services["Application Services"]
    Services --> DB["SQL Server + EF Core"]
    Services --> Mail["SMTP / MailKit"]
    Services --> Media["Cloudinary"]
    API --> Swagger["Swagger UI"]
    API --> Hub["SignalR Dashboard Hub"]
    Admin["Admin Console"] --> API
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
  Swagger/          Swagger filters and response examples
```

## API Surface

| Area | Routes |
| --- | --- |
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/refresh`, `POST /api/auth/logout` |
| Password Reset | `POST /api/auth/forgot-password`, `POST /api/auth/verify-reset-code`, `POST /api/auth/reset-password` |
| Activities | `GET /api/activities`, `GET /api/activities/{id}` |
| Reviews | `GET /api/activities/{activityId}/reviews`, `POST /api/activities/{activityId}/reviews` |
| Lookups | `GET /api/categories`, `GET /api/cities` |
| Wishlist | `GET /api/wishlist`, `GET /api/wishlist/activity-ids`, `POST /api/wishlist/{activityId}`, `DELETE /api/wishlist/{activityId}` |
| Plan | `GET /api/plan`, `POST /api/plan/items`, `DELETE /api/plan/items/{planItemId}` |
| Profile | `GET /api/users/me/profile`, `PUT /api/users/me/profile`, profile picture upload/delete |
| Chat | `POST /api/chat/conversations`, `GET /api/chat/conversations`, `DELETE /api/chat/conversations/{conversationId}` |
| Admin | `api/admin/activities`, `api/users`, `api/dashboard`, `api/reports/*`, `api/newsletter/send` |

Swagger is available at:

```text
/swagger
```

## Requirements

- .NET SDK `8.0.420` or a compatible .NET 8 SDK
- SQL Server or Azure SQL
- EF Core CLI tools
- SMTP credentials for email flows
- Cloudinary credentials for profile image uploads

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The API validates required configuration at startup. Do not commit real values, connection strings, credentials, tokens, provider keys, or production hostnames. Use local user secrets for development and environment variables in deployed environments.

| Section | Required Keys |
| --- | --- |
| `ConnectionStrings` | `DefaultConnection` |
| `Jwt` | `Key`, `Issuer`, `Audience`, `AccessTokenMinutes`, `RefreshTokenDays`, `RefreshTokenRememberMeDays` |
| `EmailSettings` | `SmtpServer`, `SmtpPort`, `SmtpUsername`, `SmtpPassword`, `FromEmail`, `FromName` |
| `Security` | `ResetCodePepper` |
| `Cloudinary` | `CloudName`, `ApiKey`, `ApiSecret` |

Use ASP.NET Core's double-underscore convention for environment variables:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Security__ResetCodePepper
EmailSettings__SmtpPassword
Cloudinary__ApiSecret
```

For local development, set real values with `dotnet user-secrets` inside the API project and keep those values outside the repository.

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

The application also runs pending EF Core migrations on startup when a valid database connection is configured.

## Authentication

Protected endpoints require a bearer token:

```http
Authorization: Bearer {accessToken}
```

Admin-only endpoints use the `AdminOnly` authorization policy, which requires the `Admin` role.

## CORS

The API is configured for known frontend origins in application configuration/code. Keep production domains environment-specific and avoid documenting private deployment targets in public repository files.

## Docker

Build the image from the API project directory so the Dockerfile context is correct:

```bash
cd NileGuideApi
docker build -t nileguide-api .
```

Run the container with environment variables or an ignored env file:

```bash
docker run --rm -p 8080:8080 --env-file .env nileguide-api
```

The container listens on port `8080`. Startup requires the same configuration listed above, including a reachable SQL Server connection. The API applies pending EF Core migrations during startup.

## Quality Bar

| Area | Implementation |
| --- | --- |
| Error handling | Central exception middleware returns consistent API errors |
| Validation | Model validation uses a lightweight `{ message, errors }` shape |
| Security | JWT validation, role policies, reset-code pepper, no committed secrets |
| Resilience | SQL retry-on-failure for transient Azure SQL startup failures |
| API contract | Swagger UI, XML comments, auth operation filters, response examples |
| Repository hygiene | Build output, IDE state, local secrets, request samples, and scratch files are ignored |

<div align="center">

![Motion](https://readme-typing-svg.demolab.com?font=Inter&weight=700&size=18&duration=2200&pause=700&color=F59E0B&center=true&vCenter=true&width=760&lines=Built+for+clean+API+contracts;Designed+for+secure+travel+workflows;Ready+for+frontend+integration)

![Footer](https://capsule-render.vercel.app/api?type=waving&height=140&section=footer&color=0:F59E0B,35:14B8A6,70:0369A1,100:0F172A)

</div>
