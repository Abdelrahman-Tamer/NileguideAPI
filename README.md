# NileGuide API

NileGuide API is the ASP.NET Core backend for the NileGuide travel platform. It provides authentication, activity discovery, user profiles, wishlists, trip planning, reviews, reporting, newsletters, and admin-facing content management endpoints.

## Tech Stack

- ASP.NET Core Web API on .NET 8
- Entity Framework Core 8 with SQL Server
- JWT bearer authentication with refresh tokens
- Swagger / OpenAPI documentation
- SignalR dashboard hub
- MailKit SMTP email delivery
- Cloudinary profile picture uploads
- Docker support

## Project Structure

```text
NileGuideApi/
  Controllers/      HTTP API controllers
  Data/             EF Core DbContext and design-time factory
  DTOs/             Request/response contracts
  Hubs/             SignalR hubs
  Middleware/       Central API exception handling
  Migrations/       EF Core database migrations
  Models/           Domain entities
  Options/          Typed configuration options
  Services/         Business logic and integrations
  Swagger/          Swagger filters and response examples
```

## Main Features

- User registration, login, logout, refresh tokens, and password reset flows
- JWT-protected current-user profile endpoints
- Activity listing, filtering, details, images, opening hours, booking links, reviews, and view tracking
- Category and city lookup endpoints for frontend filters
- Wishlist and trip plan management for authenticated users
- Admin activity CRUD and user management endpoints
- Dashboard metrics and SignalR updates
- Newsletter subscription, unsubscription, and admin send flow
- Reporting endpoints for activity views, user growth, categories, and top activities

## Requirements

- .NET SDK 8.0.420 or compatible .NET 8 SDK
- SQL Server or Azure SQL
- EF Core CLI tools
- SMTP credentials for email flows
- Cloudinary credentials for profile picture upload flows

Install EF Core tools if needed:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The app validates required settings at startup. Keep secrets out of `appsettings.json`; use user secrets locally or environment variables in deployment.

Required configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Database-ConnectionString"
  },
  "Jwt": {
    "Key": "at-least-32-bytes-secret-key-value",
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
    "ResetCodePepper": "at-least-32-bytes-secret-pepper-value"
  },
  "Cloudinary": {
    "CloudName": "cloud-name",
    "ApiKey": "api-key",
    "ApiSecret": "api-secret"
  }
}
```

Example local setup with user secrets:

```bash
cd NileGuideApi
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=NileGuideDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "replace-with-a-strong-32-byte-minimum-secret"
dotnet user-secrets set "Security:ResetCodePepper" "replace-with-a-strong-32-byte-minimum-pepper"
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

Apply migrations:

```bash
dotnet ef database update --project NileGuideApi/NileGuideApi.csproj
```

Run the API:

```bash
dotnet run --project NileGuideApi/NileGuideApi.csproj --launch-profile https
```

Local URLs:

- HTTPS API: `https://localhost:7069`
- HTTP API: `http://localhost:5126`
- Swagger UI: `https://localhost:7069/swagger`
- Dashboard hub: `https://localhost:7069/hubs/dashboard`

The app also calls `db.Database.Migrate()` on startup, so pending migrations are applied when the API starts with a valid database connection.

## API Overview

| Area | Routes |
| --- | --- |
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`, `POST /api/auth/refresh`, `POST /api/auth/logout`, password reset endpoints |
| Activities | `GET /api/activities`, `GET /api/activities/{id}` |
| Reviews | `GET /api/activities/{activityId}/reviews`, `POST /api/activities/{activityId}/reviews` |
| Lookups | `GET /api/categories`, `GET /api/cities` |
| Wishlist | `GET /api/wishlist`, `POST /api/wishlist/{activityId}`, `DELETE /api/wishlist/{activityId}` |
| Plan | `GET /api/plan`, `POST /api/plan/items`, `DELETE /api/plan/items/{planItemId}` |
| Profile | `GET /api/users/me/profile`, `PUT /api/users/me/profile`, profile picture endpoints |
| Admin | `api/admin/activities`, `api/users`, `api/dashboard`, `api/reports/*`, `api/newsletter/send` |
| Chat | `api/chat/conversations` |

Full frontend contract notes are available in `NileGuideApi/FRONTEND_ENDPOINTS.md`, and interactive API docs are available through Swagger.

## Authentication

Protected endpoints require a JWT access token:

```http
Authorization: Bearer {accessToken}
```

The API uses role-based authorization for admin-only flows. The registered `AdminOnly` policy requires the `Admin` role.

## CORS

The configured frontend origins are:

- `http://localhost:4200`
- `http://127.0.0.1:4200`
- `https://nileguide.online`
- `https://www.nileguide.online`

Credentials are allowed for the configured origins.

## Docker

Build the image from the API project directory:

```bash
cd NileGuideApi
docker build -t nileguide-api .
docker run -p 8080:8080 --env-file .env nileguide-api
```

The container listens on port `8080`.

## Documentation

- `NileGuideApi/FRONTEND_ENDPOINTS.md`: frontend-facing endpoint contract
- `BACKEND_AUDIT.md`: backend audit and implementation notes
- Swagger UI: interactive API documentation at `/swagger`
