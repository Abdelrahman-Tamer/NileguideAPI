# NileGuideApi Backend Audit

Audit date: 2026-06-11  
Branch reviewed: `main`  
Commit reviewed: `0309763 Enable SQL retry for Azure cold starts`  
Reviewer stance: senior backend review of the ASP.NET Core API, EF Core model/migrations, controllers, services, runtime pipeline, deployment files, local publish behavior, and operational readiness. Generated `bin/`, `obj/`, local IDE files, local publish profiles, and ignored archives were inspected only for risk signals, not treated as source.

## Verification Performed

- `dotnet --version`: `8.0.422`.
- `dotnet build NileGuideApi\NileGuideApi.csproj -c Release`: passed with 0 warnings and 0 errors.
- `dotnet build NileGuideApi\NileGuideApi.sln -c Release --no-restore`: passed with 0 warnings and 0 errors.
- `dotnet publish NileGuideApi\NileGuideApi.csproj -c Release --no-restore`: passed and produced `NileGuideApi\bin\Release\net8.0\publish\`.
- `dotnet test NileGuideApi\NileGuideApi.csproj -c Release --no-build --list-tests --verbosity normal`: build passed, but no test cases were listed because there is no test project.
- `dotnet list NileGuideApi\NileGuideApi.csproj package --vulnerable --include-transitive`: no known vulnerable packages from the configured NuGet sources.
- `dotnet build NileGuideApi.slnx --no-restore`: failed with `MSB4068: The element <Solution> is unrecognized`.
- `dotnet ef migrations list --project NileGuideApi\NileGuideApi.csproj --startup-project NileGuideApi\NileGuideApi.csproj --no-build`: listed migrations through `20260607235321_EnforceUserDateOfBirthRequired`, but could not determine applied/pending status because Azure SQL firewall blocked the current IP.
- `dotnet format NileGuideApi\NileGuideApi.csproj --verify-no-changes --verbosity minimal`: failed on whitespace formatting in `Program.cs`, `Middleware\ApiExceptionMiddleware.cs`, `Controllers\AuthController.cs`, and `Services\EmailTemplateService.cs`.

## Executive Summary

The backend builds cleanly and the SQL Server retry change is correctly present in both runtime and design-time EF Core configuration. `EnableRetryOnFailure` now uses 5 retries, a 30 second max delay, and explicitly includes Azure SQL error `40613` (`Program.cs:62-69`, `Data\DesignTimeDbContextFactory.cs:29-35`).

The codebase is still not production-ready as a backend system. The highest risks are authorization drift for disabled users, an unauthenticated SignalR admin hub, runtime migrations during API startup, no real automated test suite, and non-atomic database/external-service flows. The app can be deployed for controlled testing, but it should not be treated as a mature production backend until the P1/P2 items below are fixed.

Overall backend readiness score: 45/100.

## Current Good State

- Release build and publish both succeed.
- SQL transient retry is configured for Azure SQL cold-start/transient errors (`Program.cs:65-69`, `Data\DesignTimeDbContextFactory.cs:31-35`).
- JWT validation checks issuer, audience, lifetime, signing key, and uses zero clock skew (`Program.cs:198-217`).
- Passwords are hashed with BCrypt (`Controllers\AuthController.cs:76`, `Controllers\UsersController.cs:164`).
- Password reset codes are generated with `RandomNumberGenerator` and stored hashed with a pepper (`Controllers\AuthController.cs:487-498`).
- CORS is restricted to explicit frontend origins and does not use `AllowAnyOrigin` (`Program.cs:98-109`).
- Options validation exists for connection string, JWT, email settings, and reset-code pepper (`Program.cs:22-52`).
- Admin HTTP controllers use `AdminOnly` on dashboard, reports, users, and admin activities (`Controllers\DashboardController.cs:8`, `Controllers\ReportsController.cs:7`, `Controllers\UsersController.cs:44`, `Controllers\AdminActivitiesController.cs:8`).
- Wishlist and plan duplicate inserts are guarded by unique indexes and conflict handling (`Data\AppDbContext.cs:501-502`, `Data\AppDbContext.cs:551-552`, `Services\WishlistService.cs:96-106`, `Services\PlanService.cs:82-93`).
- `DateOfBirth` is required in the EF model and has a corrective migration (`Data\AppDbContext.cs:89-94`, `Migrations\20260607235321_EnforceUserDateOfBirthRequired.cs:13-23`).

## Critical And High Findings

### P1 - Disabled Users Can Still Use Most Authenticated APIs Until JWT Expiry

`AuthController.Me`, refresh, and review creation check `IsActive`, but most `[Authorize]` endpoints only trust the JWT and do not re-check user status. Examples: wishlist (`Controllers\WishlistController.cs:12`), plan (`Controllers\PlanController.cs:12`), user profile (`Controllers\UserProfilesController.cs:10`), and profile-picture endpoints under `UsersController` (`Controllers\UsersController.cs:14`, `Controllers\UsersController.cs:295`, `Controllers\UsersController.cs:373`).

Impact: when an admin blocks a user through `UpdateStatus`, the user can continue using valid access tokens against many protected endpoints until token expiry. With a 30 minute access token, that is a real enforcement gap.

Fix: centralize active-user enforcement. Good options are `JwtBearerEvents.OnTokenValidated`, a custom authorization requirement/handler, or middleware that loads the current user and rejects inactive/deleted accounts for authenticated endpoints. Also revoke active refresh tokens when a user is disabled.

### P1 - Dashboard SignalR Hub Is Not Authorized

`DashboardHub` has no `[Authorize]`, and `app.MapHub<DashboardHub>("/hubs/dashboard")` is mapped without `.RequireAuthorization()` (`Hubs\DashboardHub.cs:3-6`, `Program.cs:289`). HTTP dashboard endpoints are admin-only, but the realtime channel is public.

Impact: any reachable client can connect to the dashboard hub and receive admin update signals. The current payload is only `ActivitiesUpdated`, but this is still an admin-channel authorization gap and becomes worse if dashboard data is later pushed through the hub.

Fix: add `[Authorize(Policy = "AdminOnly")]` to `DashboardHub` or map it with `.RequireAuthorization("AdminOnly")`. Configure JWT bearer `OnMessageReceived` for SignalR access tokens if the frontend cannot send normal Authorization headers during the WebSocket negotiation.

### P1 - Migrations Run Automatically On API Startup

`db.Database.Migrate()` runs during app startup (`Program.cs:291-295`).

Impact: a transient SQL outage or Azure SQL cold start can prevent the whole API from booting. Multiple app instances can race during deployment. Schema changes become tied to the web process instead of a controlled release step.

Fix: remove startup migrations from production. Run migrations from CI/CD, a one-off migration job, or a manually controlled deployment step. If automatic migration is kept for development, gate it behind an explicit config flag plus environment check.

### P1 - No Real Automated Test Suite

There is no test project. `dotnet test` only builds the web project and lists no tests.

Impact: auth, authorization, refresh-token rotation, migrations, review aggregation, upload flows, reports, and admin activity changes can regress without signal.

Fix: add a test project using `Microsoft.AspNetCore.Mvc.Testing` and SQLite in-memory or a SQL Server test container. Prioritize integration tests for auth/admin boundaries, disabled-user enforcement, refresh rotation, review creation, profile updates, wishlist, plans, reports, and startup configuration.

## Medium Findings

### P2 - Refresh Token Rotation Has A Race Window

Refresh reads the token, checks revoked/expired state, creates a replacement, revokes the old token, and saves without a transaction or concurrency token (`Controllers\AuthController.cs:202-238`, `Controllers\AuthController.cs:604-608`).

Impact: two concurrent refresh requests can both observe the same active token and issue more than one valid replacement.

Fix: wrap refresh rotation in a transaction with appropriate isolation, or add a row-version concurrency token and reject stale updates. Add a test that sends concurrent refresh calls with the same token.

### P2 - Activity View Reports Have No Writer

`ActivityViews` exists in the DbContext and reports read it (`Data\AppDbContext.cs:54`, `Services\ReportService.cs:21-24`), but repository search found no `new ActivityView` or `_context.ActivityViews.Add(...)` write path.

Impact: `/api/reports/activity-views` is not measuring actual activity detail traffic from this API. The report can show zeros or stale data even when users browse activities.

Fix: write an `ActivityView` row when `GetActivityByIdAsync` serves an active activity, or replace this report with data from the actual analytics/event source.

### P2 - Admin Activity Image Operations Are Not Atomic

`CreateAsync` saves the activity first, uploads Cloudinary images next, then saves image rows (`Services\AdminActivityService.cs:86-102`). `UpdateAsync` deletes existing Cloudinary images before the database save (`Services\AdminActivityService.cs:175-205`). `DeleteAsync` deletes external images before the soft delete is committed (`Services\AdminActivityService.cs:225-241`).

Impact: Cloudinary failure or DB failure can leave partial activities, missing external images, orphaned Cloudinary assets, or DB rows pointing at deleted files.

Fix: use transactions for DB work, upload before final commit where possible, and add compensation cleanup for uploaded files when DB save fails. For deletes, prefer DB soft delete first and queue external cleanup.

### P2 - Upload Validation Is Inconsistent And Trusts Client Metadata

Profile pictures check length, content type, and extension only (`Controllers\UsersController.cs:321-335`). Admin activity images are uploaded by `ActivityImageService` with no size/type/signature validation beyond skipping empty files (`Services\ActivityImageService.cs:27-59`). Admin activity endpoints also do not declare route-level request size limits (`Controllers\AdminActivitiesController.cs:19-52`).

Impact: a client can submit mislabeled files or oversized multipart requests. Cloudinary helps, but the API should reject bad files before handing data to external storage.

Fix: enforce size, count, content type, extension, and image magic-byte checks consistently. Consider decoding/re-encoding images server-side, set Cloudinary image transformations, and add `[RequestSizeLimit]`/form limits to admin upload endpoints.

### P2 - Local Development Appsettings Are Copied Into Publish Output

`appsettings.Development.json` is ignored by git, but `dotnet publish` still copied it into the Release publish folder during verification.

Impact: local development settings or secrets can accidentally ship in publish artifacts even though they are not tracked by git.

Fix: ensure `appsettings.Development.json` contains no secrets and exclude it from publish in the project file if deployment should never include it. Prefer Azure App Service settings, user secrets locally, and CI/CD secret stores.

### P2 - Soft-Deleted Users Still Block Email Reuse

Users have a global unique index on `Email` while the entity also uses soft delete (`Data\AppDbContext.cs:60-67`). Admin create checks only non-deleted users before insert (`Controllers\UsersController.cs:154-155`). Profile email update has a similar non-transactional duplicate check (`Services\UserProfileService.cs:55-62`).

Impact: recreating or changing to an email used by a soft-deleted account can become a database unique constraint failure and bubble as a 500 in some flows.

Fix: choose a product policy. Either use a filtered unique index for active users, or keep global uniqueness but check ignored query filters and return a clear conflict response. Catch unique constraint failures consistently.

### P2 - Review Policy And Aggregate Updates Are Weak

Reviews only have separate indexes on `ActivityId` and `UserId`; there is no unique index for `(ActivityId, UserId)` (`Data\AppDbContext.cs:638-641`). Creating a review saves the review, then loads all active reviews and recalculates rating/count in memory (`Services\ReviewService.cs:75-89`).

Impact: one user can review the same activity repeatedly unless that is intentional. Concurrent reviews can race and overwrite aggregate values.

Fix: define the product rule. If one review per user/activity is intended, add a unique index. Recalculate aggregates with DB-side aggregate queries inside one transaction, or compute aggregates at query/report time.

### P2 - Reports Use Inefficient In-Memory Date Bucketing

`GetActivityViewsLast7DaysAsync` filters on `x.ViewedAt.Date` and loads rows into memory (`Services\ReportService.cs:21-31`). `GetUserGrowthAsync` loads users into memory and counts by month in process (`Services\ReportService.cs:43-56`).

Impact: these reports get slower as tables grow and date predicates may not use indexes efficiently.

Fix: use range predicates and database-side `GroupBy` projections. Avoid applying `.Date` to the column side of SQL predicates.

### P2 - Newsletter Sending Runs Inside The Request

The admin send endpoint loads every active subscriber and sends email from the HTTP request using `Parallel.ForEachAsync` (`Controllers\NewsletterController.cs:162-204`).

Impact: larger campaigns can time out, partially send, duplicate on retry, and hide delivery state. The SMTP provider can also throttle the API process.

Fix: enqueue a campaign job, persist per-recipient delivery state, and process in a background worker with retry/backoff and rate limits.

### P2 - Rate Limiting Is IP-Only And Proxy-Sensitive

Rate limiter partitions use `ctx.Connection.RemoteIpAddress` (`Program.cs:158-194`). Forwarded headers only trust configured known proxies (`Program.cs:75-95`).

Impact: if Azure/App Service proxy addresses are not configured, many users can collapse into the same rate-limit bucket or logs can record proxy IPs instead of client IPs. IP-only rate limiting also does not stop credential attacks distributed across IPs.

Fix: confirm `ForwardedHeaders:KnownProxies` in production, add account/email-based limits for login/reset, and consider a distributed limiter if the app scales beyond one instance.

## Lower Findings

### P3 - Public Swagger Is Intentional But Still A Production Exposure

Swagger is always enabled (`Program.cs:265-276`). The source comment says this is intentional for the frontend team.

Impact: endpoint shape, DTO names, auth behavior, and admin routes are discoverable in production.

Fix: keep it public only if accepted. Otherwise restrict Swagger to development, admin auth, or an IP allowlist.

### P3 - Missing Health Checks And Observability

There are no health/readiness endpoints, SQL health checks, Application Insights/OpenTelemetry registration, request correlation IDs, or metrics in `Program.cs`.

Impact: deployment and incident handling depend on raw logs and manual endpoint checks.

Fix: add `/health/live` and `/health/ready`, include SQL readiness, and enable structured tracing/metrics through Application Insights or OpenTelemetry.

### P3 - Error Responses Do Not Include Trace IDs Or ProblemDetails

`ApiExceptionMiddleware` logs the exception and returns only `{ "message": "Server error" }` (`Middleware\ApiExceptionMiddleware.cs:23-39`).

Impact: production 500s are hard to correlate with client reports and logs.

Fix: return a ProblemDetails-compatible response or include `traceId = HttpContext.TraceIdentifier` while keeping internal details out of the response body.

### P3 - Formatting Gate Currently Fails

`dotnet format --verify-no-changes` reports whitespace diagnostics in `Program.cs`, `Middleware\ApiExceptionMiddleware.cs`, `Controllers\AuthController.cs`, and `Services\EmailTemplateService.cs`.

Impact: adding formatting to CI would fail immediately.

Fix: run `dotnet format` in a dedicated formatting commit or clean up the listed files manually.

### P3 - API Responses Are Inconsistent

The API mixes DTOs, anonymous `{ message = ... }` objects, custom validation JSON, `200 OK` for creates, and `204 NoContent` for deletes. Examples include admin activity create returning `Ok` (`Controllers\AdminActivitiesController.cs:23-25`) and review create returning `Ok` (`Controllers\ReviewsController.cs:43-46`).

Impact: clients need endpoint-specific response handling, and OpenAPI documentation is less reliable.

Fix: standardize error responses with ProblemDetails or a single envelope, and use `201 Created` for resource creation when a location is available.

### P3 - User Search Is Non-Sargable

Admin user search lowers database columns before `Contains` (`Controllers\UsersController.cs:64-68`). Profile email duplicate checks also lower a database column (`Services\UserProfileService.cs:58-59`).

Impact: indexes are unlikely to help as user volume grows.

Fix: use normalized columns, database collation, full-text search, or provider-specific case-insensitive search patterns.

### P3 - Root Solution Entry Point Is Broken

`NileGuideApi.slnx` fails with the current SDK/build path (`NileGuideApi.slnx:1`). The nested `.sln` builds successfully.

Impact: CI/CD and new developer setup can fail if they build from the repo root.

Fix: replace the root `.slnx` with a compatible `.sln`, upgrade build tooling that supports `.slnx`, or document/build the nested solution explicitly.

### P3 - Dockerfile Is Context-Sensitive And Lightly Hardened

The Dockerfile copies `*.csproj` from the Docker build context root (`Dockerfile:1-6`). That works if the context is `NileGuideApi\`, not the repository root. The runtime image has no `HEALTHCHECK` and no explicit non-root user (`Dockerfile:8-14`).

Impact: container builds can fail depending on where the command is run. Runtime hardening is minimal.

Fix: either document `NileGuideApi` as the build context or adjust paths for repo-root context. Add a healthcheck and run as a non-root user where the hosting target supports it.

### P3 - Mixed UTC And SQL Local Time Defaults

Most application code uses `DateTime.UtcNow`, but several EF defaults use `GETDATE()` instead of UTC (`Data\AppDbContext.cs:439`, `Data\AppDbContext.cs:792`).

Impact: timestamps can drift by environment/server timezone and make reports or audit trails harder to reason about.

Fix: standardize on UTC and use `SYSUTCDATETIME()` or `GETUTCDATE()` consistently in database defaults.

### P3 - Local Deployment Artifacts Exist

Ignored local artifacts include `NileGuideApi.zip`, local publish profiles, and `appsettings.Development.json`.

Impact: these are not tracked by git, but they can still be copied, shared, or packaged accidentally.

Fix: keep local archives out of handoff channels, rotate publish credentials if ever shared, and deploy through CI/CD secrets or Azure service connections.

## Architecture Review

Architecture score: 5/10.

The project is a single ASP.NET Core Web API assembly using controllers, services, DTOs, EF Core, middleware, options, SignalR, and Cloudinary/MailKit integrations. That is acceptable for a small backend, but boundaries are inconsistent.

Strengths:

- The code uses controllers and DTOs rather than exposing EF entities as public API contracts.
- Most read/query services use `AsNoTracking`.
- Options classes exist for major configuration groups.
- Some duplicate insert races are handled with unique indexes and DB exception guards.

Weaknesses:

- `AuthController`, `UsersController`, and `NewsletterController` contain substantial business/data logic.
- `AppDbContext.OnModelCreating` is very large and should be split into `IEntityTypeConfiguration<T>` classes.
- Role strings, user-id extraction, and response shapes are repeated across controllers.
- External-service effects are executed inline in request handlers without durable job/outbox handling.
- User preferences are stored as JSON strings, which is simple but limits relational querying and integrity.

Recommended direction: keep the single-project structure for now, but extract auth/session/user/newsletter logic into cohesive services, add a reusable current-user service, split EF mappings, and introduce background jobs for email and external asset cleanup before adding more features.

## Security Review

Security score: 52/100.

Strong points:

- JWT validation is not using permissive defaults.
- Passwords and reset codes are not stored in plain text.
- Public CORS origins are explicit.
- Admin HTTP controllers are protected.
- Reset/login/register endpoints have named rate-limit policies.

Main gaps:

- Disabled users are not globally rejected from authenticated endpoints.
- SignalR dashboard hub is public.
- Refresh-token rotation is not concurrency-safe.
- Upload validation is incomplete.
- Public Swagger exposes the contract.
- No HSTS/security headers are configured for production.
- No security audit log exists for admin/user/security actions.

## Data And Migration Review

Database score: 5/10.

- EF model currently requires `Users.date_of_birth` (`Data\AppDbContext.cs:89-94`).
- Corrective migration updates null dates to `2000-01-01` before altering the column to non-null (`Migrations\20260607235321_EnforceUserDateOfBirthRequired.cs:13-23`).
- EF CLI listed the latest migration but could not determine applied/pending status because Azure SQL rejected the current IP.
- Runtime migrations remain the largest database operations risk.
- Backfilled `2000-01-01` dates are placeholders. The schema is fixed, but those users should be asked to confirm or update real dates of birth.
- Avoid editing migrations already applied to shared databases. Use follow-up migrations.

## API Design Review

API design score: 6/10.

Good:

- Controllers use `[ApiController]` and attribute routing.
- Most endpoints have DTOs and validation attributes.
- Admin routes are separated from public routes.
- Pagination exists for catalog and wishlist.

Needs work:

- No API versioning.
- Response shapes are inconsistent.
- Some creation endpoints return `200 OK` instead of `201 Created`.
- `ReportsController.GetTopActivities` accepts any `top` value without lower/upper bounds (`Controllers\ReportsController.cs:41-45`).
- User-id extraction is repeated and inconsistent across controllers.

## Performance And Scalability Review

Performance score: 4/10. Scalability score: 3/10.

The app is acceptable for low traffic, but several paths will struggle as data grows:

- Reports load rows into memory for date buckets.
- Review aggregation loads all reviews and updates denormalized counters after insert.
- Activity listing and wishlist load multiple includes and then map in memory.
- Newsletter sending runs in the request.
- Rate limiting is in-memory and per-instance.
- No caching strategy exists for relatively static category/city data.

Recommended path: fix report queries first, add tests around review aggregates, then introduce background jobs and cache low-change lookup data.

## DevOps And Operations Review

DevOps score: 3/10. Observability score: 2/10.

Gaps:

- No CI workflow exists under `.github/workflows`.
- Root `NileGuideApi.slnx` build fails.
- No health/readiness endpoints.
- No automatic test gate because no tests exist.
- Formatting verification fails.
- Dockerfile is context-sensitive and lacks runtime hardening.
- Runtime migrations are coupled to API startup.
- Publish output includes local `appsettings.Development.json`.
- No documented backup, rollback, smoke-test, or deployment plan.

Recommended path: create a minimal CI pipeline that restores, formats, builds the nested solution or project, runs tests, checks vulnerabilities, and publishes only after migrations are handled separately.

## Production Readiness Checklist

- Project Release build: PASS.
- Nested solution Release build: PASS.
- Root `.slnx` build: FAIL.
- Publish output generation: PASS.
- SQL retry for transient Azure SQL errors: PASS.
- Database migration applied status: UNKNOWN from local verification due Azure SQL firewall.
- Runtime migration safety: FAIL.
- HTTP admin authorization: PASS.
- SignalR admin authorization: FAIL.
- Disabled-user enforcement: FAIL.
- Upload validation: PARTIAL.
- Automated tests: FAIL.
- Vulnerable package check: PASS.
- Formatting gate: FAIL.
- CI/CD: FAIL.
- Health/readiness: FAIL.
- Observability: FAIL.
- Docker production hardening: PARTIAL.
- Secrets/publish artifact hygiene: PARTIAL.

## Top Priority Fixes

1. Enforce `IsActive` and soft-delete status globally for authenticated users.
2. Protect `/hubs/dashboard` with `AdminOnly` and configure SignalR JWT token handling.
3. Remove or gate `db.Database.Migrate()` from production startup.
4. Add an integration test project and cover auth/admin/disabled-user/refresh/review/wishlist/plan/profile/report flows.
5. Make refresh-token rotation concurrency-safe.
6. Add a real `ActivityView` write path or remove/fix that report.
7. Harden upload validation and Cloudinary failure handling.
8. Move newsletter sending and external asset cleanup to background jobs.
9. Exclude local development appsettings from publish output if they can contain secrets.
10. Fix the root build entry point, formatting gate, health checks, and CI workflow.

## Release Recommendation

The latest retry-on-failure change is valid and should reduce Azure SQL cold-start failures after publish. It does not remove the broader startup risk caused by running migrations in the API process.

Current recommendation: safe for controlled development/testing deployments; not ready for unattended production operation until P1 items are fixed and a basic test/CI/health-check baseline exists.
