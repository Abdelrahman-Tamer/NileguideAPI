# NileGuideApi Backend Audit

Audit date: 2026-06-08  
Branch reviewed: `codex/nileguide-api-updates`  
Scope: ASP.NET Core API source, EF Core model/migrations, controllers, services, runtime pipeline, deployment files, and local verification commands. Generated `bin/`, `obj/`, archives, and local IDE files were not treated as source.

## Verification Performed

- `dotnet --version`: `8.0.421`.
- `dotnet build NileGuideApi\NileGuideApi.csproj -c Release`: passed with 0 warnings and 0 errors.
- `dotnet build NileGuideApi.slnx --no-restore`: failed with `MSB4068: The element <Solution> is unrecognized`.
- `dotnet test NileGuideApi\NileGuideApi.csproj -c Release --no-build --list-tests --verbosity normal`: build passed, but no tests were listed/discovered.
- `dotnet list NileGuideApi\NileGuideApi.csproj package --vulnerable --include-transitive`: no known vulnerable packages from configured NuGet sources.
- `dotnet ef migrations list --project NileGuideApi\NileGuideApi.csproj --startup-project NileGuideApi\NileGuideApi.csproj --no-build`: latest migration `20260607235321_EnforceUserDateOfBirthRequired` is present and no pending marker was reported.

## Executive Summary

The backend currently builds and the database migration chain includes the corrective `DateOfBirth` migration. The project is better than the previous audit in two important areas: SQL Server transient retry is configured, and the `Users.date_of_birth` mismatch has a follow-up migration that normalizes null rows and makes the column required.

It is still not production-ready without targeted fixes. The highest risks are runtime migrations on app startup, an unauthenticated SignalR dashboard hub, no automated tests, an invalid activity-view metric because no code writes `ActivityViews`, and several non-atomic flows around refresh tokens, reviews, and Cloudinary image operations.

Overall backend readiness score: 48/100.

## Current Good State

- The API project compiles cleanly in Release.
- EF Core uses `EnableRetryOnFailure` for SQL Server in both runtime and design-time context creation (`Program.cs:62-69`, `Data\DesignTimeDbContextFactory.cs:29-35`).
- `DateOfBirth` is required in the EF model and has a corrective migration (`Data\AppDbContext.cs:89-94`, `Migrations\20260607235321_EnforceUserDateOfBirthRequired.cs:13-23`).
- Admin controllers protect HTTP dashboard, reports, and activity management with `AdminOnly` (`Controllers\DashboardController.cs:8`, `Controllers\ReportsController.cs:7`, `Controllers\AdminActivitiesController.cs:8`).
- CORS is restricted to known frontend origins (`Program.cs:98-109`).
- Options validation exists for connection string, JWT, email, and reset-code pepper (`Program.cs:22-52`).
- Auth-sensitive endpoints have named rate limit policies for register, login/refresh, and reset flows (`Program.cs:158-194`, `Controllers\AuthController.cs:58`, `Controllers\AuthController.cs:188`, `Controllers\AuthController.cs:295`).

## Critical And High Findings

### P1 - Dashboard SignalR Hub Is Not Authorized

`DashboardHub` has no `[Authorize]`, and `app.MapHub<DashboardHub>("/hubs/dashboard")` is mapped without `.RequireAuthorization()` (`Hubs\DashboardHub.cs:3-6`, `Program.cs:288-289`). The HTTP dashboard endpoints are admin-only, but the realtime channel is public.

Impact: any client that can reach the API can connect to the hub and receive admin dashboard update signals such as `ActivitiesUpdated`. Today the payload is only an event name, but this is still an authorization gap and becomes serious if dashboard data is later sent through the hub.

Fix: add `[Authorize(Policy = "AdminOnly")]` to `DashboardHub` or map it with `.RequireAuthorization("AdminOnly")`, then configure the frontend SignalR client to pass the Bearer token.

### P1 - Migrations Run Automatically On App Startup

`db.Database.Migrate()` runs during startup (`Program.cs:291-295`). This is risky in production and was directly related to the earlier Azure SQL startup failure when the database was unavailable.

Impact: a transient SQL outage can prevent the whole API from starting. Multiple app instances can also race during deployment. Schema changes become tied to web process boot instead of a controlled release step.

Fix: remove startup migrations from production. Run migrations from CI/CD, a one-off migration job, or a manually controlled admin step. If automatic migration is kept for development, gate it behind an explicit configuration flag and environment check.

### P1 - No Real Automated Test Suite

There is no separate test project. `dotnet test` only builds the web project and lists no tests.

Impact: auth, admin authorization, migrations, review aggregation, refresh token reuse, file uploads, and reporting can regress without signal.

Fix: add integration tests with `WebApplicationFactory`, plus focused service tests for auth token rotation, reviews, plans, wishlist, profile updates, and admin activity management.

## Medium Findings

### P2 - Activity View Reports Have No Writer

`ActivityViews` exists in the DbContext and reports read it (`Data\AppDbContext.cs:54`, `Services\ReportService.cs:21-24`), but repository search found no `new ActivityView` or `_context.ActivityViews.Add(...)` write path.

Impact: `/api/reports/activity-views` is not measuring real API activity from this codebase. The dashboard can show zeros or stale data even when users browse activities.

Fix: write an `ActivityView` row when `GetActivityByIdAsync` serves an active activity, or replace the report with analytics data from the actual frontend/event source.

### P2 - Admin Activity Image Operations Are Not Atomic

`CreateAsync` saves the activity first, uploads Cloudinary images next, then saves image rows (`Services\AdminActivityService.cs:86-102`). `UpdateAsync` deletes existing Cloudinary images before the DB save (`Services\AdminActivityService.cs:175-205`). `DeleteAsync` deletes external images before the soft delete is committed (`Services\AdminActivityService.cs:225-241`).

Impact: Cloudinary failure or DB failure can leave partial activities, missing external images, or DB rows pointing at deleted files.

Fix: use a transaction for DB work, upload before final commit where possible, and add compensation cleanup for uploaded files when DB save fails. For deletes, prefer DB soft delete first and queue external cleanup.

### P2 - Refresh Token Rotation Has A Race Window

Refresh reads the token, checks `RevokedAt`, creates a replacement, revokes the old token, and saves without a transaction or concurrency token (`Controllers\AuthController.cs:202-238`, `Controllers\AuthController.cs:604-608`).

Impact: two concurrent refresh requests can both observe the same active token and issue more than one valid replacement.

Fix: wrap refresh rotation in a transaction with strict isolation or add a row-version concurrency token and fail stale updates.

### P2 - Soft-Deleted Users Still Block Email Reuse

Users have a global unique index on `Email` while the entity also uses soft delete (`Data\AppDbContext.cs:60-67`). Admin create checks only non-deleted users before insert (`Controllers\UsersController.cs:152-155`).

Impact: recreating a deleted email can become a database unique constraint failure and bubble as a 500 in some flows.

Fix: choose a clear policy. Either use a filtered unique index for active users, or keep global uniqueness but check ignored query filters and return a clear conflict message.

### P2 - Review Policy And Aggregate Updates Are Weak

Reviews only have separate indexes on `ActivityId` and `UserId`; there is no unique index for `(ActivityId, UserId)` (`Data\AppDbContext.cs:637-641`). Creating a review saves the review, then loads all active reviews and recalculates rating/count in memory (`Services\ReviewService.cs:75-89`).

Impact: one user can review the same activity repeatedly unless that is intentionally allowed. Concurrent reviews can also race and overwrite aggregate values.

Fix: define the product rule. If one review per user/activity is intended, add a unique index. Recalculate aggregates with a DB-side aggregate inside one transaction or move aggregates to query-time/reporting.

### P2 - Upload Validation Trusts Client Metadata

Profile picture validation checks only file length, `ContentType`, and extension (`Controllers\UsersController.cs:321-334`). Activity image upload does not validate type/size in `ActivityImageService` beyond skipping empty files (`Services\ActivityImageService.cs:34-48`).

Impact: mislabeled or malicious files can be sent to Cloudinary. Cloudinary helps, but the API should still validate image signatures and enforce consistent upload limits before handing off.

Fix: check magic bytes, decode/re-encode images server-side when feasible, set explicit Cloudinary resource/image transformations, and apply the same validation to admin activity images.

### P2 - Root Solution Entry Point Is Broken

`NileGuideApi.slnx` fails with the current .NET SDK/build path. The nested project builds, but the repo root build command fails.

Impact: CI/CD and new developer setup are easy to misconfigure. A pipeline that builds the root solution will fail before tests or publish.

Fix: replace the root `.slnx` with a compatible `.sln`, upgrade the build tooling that supports `.slnx`, or make CI build the nested project/solution explicitly.

## Lower Findings

### P3 - Public Swagger Is Intentional But Still A Production Exposure

Swagger is always enabled (`Program.cs:265-276`). The source comment says this is intentional for the frontend team (`Program.cs:221`).

Impact: endpoint shape, DTO names, auth behavior, and admin routes are discoverable in production.

Fix: keep it public only if that is an accepted product decision. Otherwise restrict to Development, admin auth, or an IP allowlist.

### P3 - Missing Health Checks And Observability

There are no health/readiness endpoints, SQL health checks, OpenTelemetry, Application Insights registration, request correlation IDs, or metrics in `Program.cs`.

Impact: deployment and incident handling depend on raw logs and manual endpoint checks.

Fix: add `/health/live` and `/health/ready`, include a SQL Server readiness check, and enable Application Insights or OpenTelemetry with trace IDs in responses/logs.

### P3 - Error Responses Do Not Include Trace IDs

`ApiExceptionMiddleware` logs the exception and returns `{ "message": "Server error" }` (`Middleware\ApiExceptionMiddleware.cs:24-39`).

Impact: production 500s are hard to correlate with logs from a client report.

Fix: return a ProblemDetails-compatible response or at least include `traceId = HttpContext.TraceIdentifier` while keeping internal details out of the body.

### P3 - Report Queries Are Inefficient

`ReportService.GetActivityViewsLast7DaysAsync` filters on `x.ViewedAt.Date` and loads rows into memory (`Services\ReportService.cs:21-31`). `GetUserGrowthAsync` also loads users and counts in memory (`Services\ReportService.cs:43-56`).

Impact: reports will get slower as the tables grow and may not use indexes efficiently.

Fix: use date range predicates and database-side `GroupBy` projections.

### P3 - Search Uses Non-Sargable Lowercase Contains

Admin user search lowers database columns before `Contains` (`Controllers\UsersController.cs:64-68`).

Impact: indexes are unlikely to help once the table grows.

Fix: use normalized columns, database collation, full text search, or provider-specific case-insensitive search.

### P3 - Newsletter Sending Runs Inside The Request

Newsletter send loads all active subscribers and sends emails synchronously from the controller (`Controllers\NewsletterController.cs:154-204`).

Impact: a large subscriber list can time out the request and cause duplicate/partial sends.

Fix: enqueue newsletter jobs and process them in a background worker with retries and delivery tracking.

### P3 - Dockerfile Is Fragile For Repo-Root Builds

The Dockerfile copies `*.csproj` from the Docker build context root (`Dockerfile:1-6`). That works only if the context is `NileGuideApi\`, not the repository root. The runtime image also has no healthcheck and no explicit non-root user.

Impact: container builds can fail depending on where the command is run. Runtime hardening is minimal.

Fix: either document/build with `NileGuideApi` as context or adjust paths for repo-root context. Add a healthcheck and non-root runtime user.

### P3 - Local Publish Profiles Exist

Publish profiles are ignored by git, and `git ls-files` shows they are not tracked. They still exist locally under `NileGuideApi\Properties\PublishProfiles`.

Impact: local saved deployment credentials or encrypted user profile data can be copied accidentally outside git.

Fix: remove local saved passwords, rotate publish credentials if they were ever shared, and deploy through CI/CD secrets or Azure service connections.

## Database And Migration Status

- EF model currently requires `Users.date_of_birth` (`Data\AppDbContext.cs:89-94`).
- Corrective migration updates null dates to `2000-01-01` before altering the column to non-null (`Migrations\20260607235321_EnforceUserDateOfBirthRequired.cs:13-23`).
- EF CLI migration list includes `20260607235321_EnforceUserDateOfBirthRequired` with no pending marker.
- Data quality caveat: rows backfilled to `2000-01-01` are placeholders. The schema is fixed, but those users should be asked to confirm or update their real date of birth.
- Process caveat: do not edit migrations that were already applied to a shared database. Use follow-up migrations, as the latest correction does.

## Security Review

Security score: 60/100.

Strong points:

- JWT validation checks issuer, audience, lifetime, signing key, and zero clock skew.
- Passwords use BCrypt.
- Reset codes are generated with `RandomNumberGenerator`.
- Sensitive runtime config is empty in `appsettings.json`; local/prod values must come from user secrets, environment variables, or platform settings.
- Admin HTTP controllers use `AdminOnly`.

Main gaps:

- SignalR dashboard hub is not protected.
- Refresh token rotation is not concurrency-safe.
- Upload validation is incomplete.
- Public Swagger exposes the API contract.
- No HSTS/security headers are configured for production.
- No structured security audit log exists for admin/user/security actions.

## Architecture Review

Architecture score: 5/10.

The project is a single ASP.NET Core Web API assembly using controllers, services, EF Core, DTOs, middleware, and options. That is acceptable for a small backend, but boundaries are inconsistent:

- Some controllers delegate to services, while `AuthController`, `UsersController`, and `NewsletterController` still contain substantial business/data logic.
- `AppDbContext.OnModelCreating` is very large and configures all entities in one method.
- Role strings and user-id extraction are repeated across controllers.
- External storage behavior is mixed into request flow without durable outbox/background processing.

Recommended direction: keep the current single-project structure for now, but move controller logic into cohesive services and split EF mappings into `IEntityTypeConfiguration<T>` classes before adding more features.

## API Design Review

API design score: 6/10.

Good:

- Controllers use attribute routing and `[ApiController]`.
- Most endpoints have DTOs and validation attributes.
- Admin routes are separated under `/api/admin/...`, `/api/dashboard`, and `/api/reports`.

Needs work:

- No API versioning.
- Response bodies are inconsistent: typed DTOs, anonymous `{ message = ... }`, and custom validation JSON are mixed.
- Creation endpoints return `200 OK` in places where `201 Created` would be clearer (`AdminActivitiesController.Create`, `ReviewsController.Create`, auth register).
- `ReportsController.GetTopActivities` accepts any integer `top` without a lower/upper bound (`Controllers\ReportsController.cs:41-45`).

## Performance And Scalability Review

Performance score: 4/10. Scalability score: 3/10.

The app is fine for low traffic, but several hot paths are not ready for larger production traffic:

- Reporting loads rows into memory for date buckets.
- Review aggregation loads all reviews and updates denormalized counters after insert.
- Activity listing uses multiple `Include`s and maps in memory; `AsSplitQuery` helps, but DTO projection would scale better.
- Newsletter send is synchronous in the HTTP request.
- Rate limiting is in-memory and per-instance.

Recommended path: optimize reports first, add tests around aggregates, then introduce background jobs for newsletter and external cleanup work.

## DevOps And Operations Review

DevOps score: 3/10. Observability score: 2/10.

Gaps:

- No CI workflow exists under `.github/workflows`.
- Root solution build fails.
- No health checks/readiness checks.
- No automatic test gate because no tests exist.
- Dockerfile is context-sensitive and lacks runtime hardening.
- Runtime migrations are coupled to API startup.
- No documented backup/rollback/deployment plan.

Recommended path: create a minimal CI pipeline that restores, builds `NileGuideApi\NileGuideApi.csproj`, runs tests, lists vulnerabilities, and publishes only after migrations are handled separately.

## Production Readiness Checklist

- Build: PASS for project, FAIL for root `.slnx`.
- Database migration chain: PASS, latest corrective migration present.
- Runtime DB resilience: PARTIAL, SQL retry exists but startup migration remains risky.
- Authentication/authorization: PARTIAL, HTTP admin endpoints protected but SignalR hub is not.
- Validation: PARTIAL, DTO validation exists but upload validation is weak.
- Tests: FAIL, no test suite.
- CI/CD: FAIL, no workflow.
- Health/readiness: FAIL.
- Observability: FAIL.
- Security headers: FAIL.
- Docker: PARTIAL.
- Secrets hygiene: PARTIAL, appsettings are empty but local publish profiles should be cleaned.

## Top Priority Fixes

1. Protect `/hubs/dashboard` with `AdminOnly`.
2. Remove or gate `db.Database.Migrate()` from production startup.
3. Add a real `ActivityView` write path or remove/fix the report.
4. Add integration tests for auth, admin auth, migrations-critical paths, reviews, wishlist, plans, profile, and reports.
5. Fix root build entry point or update CI to build the correct project.
6. Make refresh-token rotation concurrency-safe.
7. Add a clear review uniqueness policy and transactional aggregate updates.
8. Harden image upload validation and Cloudinary failure handling.
9. Add health/readiness checks plus Application Insights/OpenTelemetry.
10. Move newsletter sending and external cleanup to background jobs.

## Release Recommendation

Do not treat publish as proof that everything is updated. Publishing deploys the current code, but the release is only healthy if:

- The correct branch/commit is deployed.
- The target database has all migrations applied.
- Startup logs show no migration/database errors.
- Smoke tests pass against the deployed URL.
- Admin-only endpoints and the SignalR hub reject unauthenticated users.
- The dashboard/report data is backed by real writes, not empty tables.

Current recommendation: safe for continued development and controlled testing; not ready for unattended production deployment until the P1 items are fixed.
