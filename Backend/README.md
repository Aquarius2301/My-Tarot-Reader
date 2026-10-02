# Backend

ASP.NET Core 8 Web API for **My Tarot Reader**, built with Clean Architecture. It serves the tarot draw flows (guest + authenticated), the daily check-in streak & coin economy, Google OAuth, and AI tarot readings — both the single-spread flow and the multi-card deep spreads (12 astrological houses, 12 months, crossroads).

## Requirements

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) — primary database
- [Redis](https://redis.io/download/) — guest draw cooldown & device fingerprints (local, or Upstash-style `rediss://` URI)
- **Google OAuth credentials** (Client ID) — required for Google login
- **Google Gemini API key** — required for AI tarot readings
- **Resend API key** — optional, used for the transactional welcome email

## Architecture

Clean Architecture; dependencies point only inwards (toward Domain):

```
Api -> Infrastructure -> Application -> Domain
Api -> Application -> Domain
```

```
Backend
├── dockerfile
├── .dockerignore         # keeps local bin/obj out of the image (see dockerfile: COPY . .)
├── MyTarotReader.sln
├── scripts/              # run-local, clean-build, add-migration, update-database (.sh + .cmd)
├── src/
│   ├── Api/
│   │   ├── Controllers/  # one controller per domain
│   │   ├── Extensions/   # DI wiring (dependencies, CORS, Redis, Swagger, settings file)
│   │   ├── Helpers/      # CookieHelper, JwtHelper, [DevelopmentOnly] attribute + controller convention
│   │   ├── Middlewares/  # GlobalExceptionMiddleware, request/response logging
│   │   ├── Properties/   # launchSettings.json
│   │   └── Program.cs, appsettings.json
│   ├── Application/
│   │   ├── Common/       # ApiResponse<T>, exceptions, validators (ValidationHelper), helpers (StreakHelper, ReadingInputGuard)
│   │   ├── Constants/    # errors/ (error.<domain>.<camelCase>) + tarot/ (TarotConstant, DeepTarotConstant)
│   │   ├── Contracts/    # services, persistence, backgrounds/, common/
│   │   ├── Models/       # <Fn>Request / <Fn>Result records
│   │   ├── Settings/     # Jwt, Wallet, Streak, Gemini, Email, DevAuth, TokenCleanup
│   │   └── Validators/   # FluentValidation validators
│   ├── Domain/           # entities, enums, common — pure C#, no EF Core
│   └── Infrastructure/
│       ├── Backgrounds/  # EmailBackgroundQueue + Worker, TokenCleanupWorker (hosted services)
│       ├── Common/       # GeminiClient, GeminiThinkingLevelCatalog, EmailSender, templates
│       ├── Persistence/  # AppDbContext, Configurations/ (IEntityTypeConfiguration), migrations
│       └── Services/     # concrete I<N>Service implementations
└── test/
    └── UnitTest/         # xUnit + Moq + EF Core InMemory + FluentAssertions
        └── Backgrounds/  # worker + queue tests
```

- **Domain** — entities (`User`, `TarotReading`, `AIDeepTarotReading`, `Streak`, `Wallet`, `Order`, `RefreshToken`, ...), enums (`UserRole`, `QuestionType`, `CardCount`, `CrossroadsTimeFrame`, ...). No EF Core.
- **Application** — service interfaces, DTO records (`<Fn>Request` / `<Fn>Result`), settings classes, error codes, `ApiResponse<T>`, FluentValidation validators. No EF Core implementation.
- **Infrastructure** — `AppDbContext`, entity configurations, migrations, concrete services, hosted background workers, JWT generator, Google auth validator, email handler, AI tarot client.
- **Api** — controllers (one per domain), DI wiring, `GlobalExceptionMiddleware`, `Program.cs`.

### Dependencies

Target framework: `net8.0` for all five projects.

| Project | Package | Version |
| --- | --- | --- |
| Api | `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.11 |
| Api | `Microsoft.AspNetCore.OpenApi` | 8.0.11 |
| Api | `Microsoft.EntityFrameworkCore.Design` (design-time only) | 8.0.11 |
| Api | `Swashbuckle.AspNetCore` | 6.6.2 |
| Application | `FluentValidation.AspNetCore` | 11.3.1 |
| Application | `Microsoft.EntityFrameworkCore` | 8.0.11 |
| Application | `Microsoft.Extensions.Configuration.Abstractions` | 8.0.0 |
| Domain | _(none — pure C#)_ | |
| Infrastructure | `Microsoft.Extensions.Hosting.Abstractions` | 8.0.1 |
| Infrastructure | `Microsoft.IdentityModel.Tokens` | 8.22.0 |
| Infrastructure | `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.11 |
| Infrastructure | `Google.Apis.Auth` | 1.76.0 |
| Infrastructure | `StackExchange.Redis` | 2.8.0 |
| Infrastructure | `System.IdentityModel.Tokens.Jwt` | 8.22.0 |
| UnitTest | `Microsoft.NET.Test.Sdk` | 17.14.1 |
| UnitTest | `xunit` / `xunit.runner.visualstudio` | 2.9.3 / 2.8.2 |
| UnitTest | `Microsoft.EntityFrameworkCore.InMemory` | 8.0.11 |
| UnitTest | `Moq` | 4.20.70 |
| UnitTest | `FluentAssertions` | 6.12.2 |
| UnitTest | `coverlet.collector` | 6.0.0 |

## Configuration

Configuration lives in `src/Api/appsettings.json` (placeholders in the repo — never commit real values). Override per environment via env vars or your deploy platform (e.g. Render env variables).

| Key                                    | Meaning                                                                                            | Example                                                                                          |
| -------------------------------------- | -------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `ConnectionStrings:DefaultConnection`  | PostgreSQL connection string                                                                       | `Host=localhost;Port=5432;Database=MyTarotReader;Username=postgres;Password=YourStrongPassword!` |
| `Redis:Configuration`                  | Redis connection string                                                                            | `localhost:6379`                                                                                 |
| `Cors:FrontendUrl`                     | Allowed frontend origin (CORS, with credentials)                                                   | `https://your-frontend-domain.example`                                                           |
| `Google:ClientId`                      | Google OAuth client ID                                                                             | `YOUR_PRODUCTION_CLIENT_ID.apps.googleusercontent.com`                                           |
| `DevAuth:ProviderKey`                  | Provider key identifying the seeded dev-only user; looked up on every dev login                   | `dev-swagger`                                                                                    |
| `DevAuth:Email`                        | Email of the seeded dev-only user                                                                  | `dev@my-tarot-reader.local`                                                                      |
| `DevAuth:FullName`                     | Display name of the seeded dev-only user                                                             | `Swagger Dev User`                                                                                |
| `DevAuth:InitialWhiteCoins`            | White coins granted when the dev user's row is first created                                        | `1000`                                                                                            |
| `DevAuth:WhiteCoinExpireDays`          | Expiry in days of the dev user's initial white coin batch                                           | `365`                                                                                             |
| `DevAuth:DefaultDeviceFingerprint`     | Fingerprint used when the request carries no `X-Device-Id`                                         | `swagger-dev`                                                                                     |
| `Gemini:Apis`                          | Ordered list of Gemini API keys, tried from the first one on every call                            | `[ "KEY1", "KEY2" ]`                                                                             |
| `Gemini:Models`                        | Ordered list of models `{ Model, ThinkingLevel }`, each one paired with every key in `Gemini:Apis` | `[ { "Model": "gemini-3.8-flash", "ThinkingLevel": "low" } ]`                                    |
| `Gemini:MaxRetries`                    | Extra attempts on the same (api, model) pair before failing over                                   | `2`                                                                                              |
| `Gemini:RetryDelayMilliseconds`        | Base delay of the exponential backoff between two attempts                                         | `1000`                                                                                           |
| `Gemini:MaxTotalWaitSeconds`           | Total time spent retrying before giving up; `0` = unlimited                                        | `30`                                                                                             |
| `AiTarot:Costs`                        | White coin cost per reading, keyed by card count (3/5/7/10)                                        | `{ "3": 2, "5": 3, "7": 4, "10": 5 }`                                                            |
| `Jwt:SecretKey`                        | Signing key for JWT                                                                                | `YOUR_VERY_LONG_SECRET_KEY_FOR_LOCAL_DEV_ENVIRONMENT`                                            |
| `Jwt:Issuer`                           | JWT issuer                                                                                         | `https://localhost:7000`                                                                         |
| `Jwt:Audience`                         | JWT audience (frontend origin)                                                                     | `https://localhost:5173`                                                                         |
| `Jwt:AccessTokenDurationMinutes`       | Access token lifetime                                                                              | `480`                                                                                            |
| `Jwt:RefreshTokenDurationDays`         | Refresh token lifetime                                                                             | `7`                                                                                              |
| `TokenCleanup:IntervalMinutes`         | Interval for expiring-token cleanup job                                                            | `60`                                                                                             |
| `Wallet:InitialWhiteCoins`             | New-user starting white coins                                                                      | `5`                                                                                              |
| `Wallet:ExpireDays`                    | White coin batch expiry in days                                                                    | `30`                                                                                             |
| `Streak:CycleDays`                     | Days in a check-in streak cycle                                                                    | `7`                                                                                              |
| `Streak:DailyCheckInRewards`           | Coins per day in the cycle                                                                         | `[1, 1, 1, 1, 2, 2, 3]`                                                                          |
| `Email:ApiKey`                         | Resend API key — secret, never committed                                                           | `re_YOUR_API_KEY`                                                                                |
| `Email:Endpoint`                       | Resend API base URL                                                                                | `https://api.resend.com`                                                                         |
| `Email:FromAddress` / `Email:FromName` | Sender identity (the domain must be verified in Resend)                                            | `you@your-domain.com` / `My Tarot Reader`                                                        |

`Gemini:Apis` and `Gemini:Models` are arrays, so on Render (or any env-var platform) set one variable per entry using the `__` separator and a numeric index (these override `appsettings.json`):

```
Gemini__Apis__0=KEY_1
Gemini__Apis__1=KEY_2
Gemini__Models__0__Model=gemini-3.8-flash
Gemini__Models__0__ThinkingLevel=low
Gemini__Models__1__Model=gemini-3.5-flash
Gemini__Models__1__ThinkingLevel=low
Gemini__Models__2__Model=gemini-3.1-flash-lite
Gemini__Models__2__ThinkingLevel=low
...
```

Set every index when you want to replace the complete array. If an index is omitted,
the value from `appsettings.json` can still remain available through configuration
source merging. For production, the secret settings file below is usually simpler
because it replaces the whole JSON configuration in one document.

### Gemini retry and failover

Every call walks the `(api, model)` pairs from `Api1/Model1` and never revisits a pair. On each pair it makes `1 + MaxRetries` attempts: the first retry waits `RetryDelayMilliseconds`, then every next one doubles it and adds a random jitter of up to `RetryDelayMilliseconds` (so `1000ms`, then `2000–2999ms`), and a `Retry-After` header from Gemini always wins when it asks for a longer wait. `MaxTotalWaitSeconds` caps the time spent retrying in a single call.

Once the attempts of a pair are used up, the next pair depends on the failure:

| Failure                                       | Retry with backoff | Next pair                |
| --------------------------------------------- | ------------------ | ------------------------ |
| `429`                                         | yes                | next **api**, same model |
| `500` / `502` / `503` / `504` / network error | yes                | next **model**, same api |
| `400` / `404` (model or parameter rejected)   | no                 | next **model**, same api |
| `401` / `403` (api key rejected)              | no                 | next **api**, same model |
| `200` with an empty or unreadable body        | no                 | next **model**, same api |

`thinking-level` is only sent when the model accepts it: `GeminiThinkingLevelCatalog` (`src/Infrastructure/Common/`) lists the levels of each model, and anything unsupported (including the 2.5 series, which only takes `thinkingBudget`) is left out so the call falls back to the model default instead of failing with a `400`.

Welcome emails are delivered through the **Resend HTTPS API** (`POST https://api.resend.com/emails`, bearer `Email:ApiKey`) rather than Resend's SMTP endpoint. Render's free tier blocks outbound traffic to the SMTP ports `25`, `465` and `587`, so an SMTP client times out on connect there; HTTPS on port `443` is unaffected.

On an env-var-only platform (no settings file), `Email:ApiKey` is set as:

```
Email__ApiKey=re_YOUR_API_KEY
```

### Production configuration

Production keeps all of its settings in a single JSON document instead of `A__B` environment variables, so the whole file can be copied in one go. In production `Program.cs` loads it from the path below **before** the services are registered (JWT, Redis, the connection string and CORS are read eagerly at that point, so a file added later would be ignored by them).

1. Render Dashboard → your service → **Environment** → **Secret Files** → **+ Add Secret File**
2. Filename: `appsettings.Production.json`
3. Contents: the JSON below with your real values
4. **Save Changes** — Render deploys again and mounts the file at `/etc/secrets/`

Delete the `A__B` variables the file replaces in the same save, so the service never starts without its settings. Keep the platform variables Render generates itself (`PORT`, `ASPNETCORE_*`).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require"
  },
  "Redis": {
    "Configuration": "rediss://default:...@....upstash.io:6379"
  },
  "Cors": {
    "FrontendUrl": "https://your-frontend-domain.example"
  },
  "Google": {
    "ClientId": "YOUR_PRODUCTION_CLIENT_ID.apps.googleusercontent.com"
  },
  "Gemini": {
    "MaxRetries": 2,
    "RetryDelayMilliseconds": 1000,
    "MaxTotalWaitSeconds": 30,
    "Apis": ["KEY_1", "KEY_2"],
    "Models": [
      { "Model": "gemini-3.8-flash", "ThinkingLevel": "low" },
      { "Model": "gemini-3.5-flash", "ThinkingLevel": "low" }
    ]
  },
  "Jwt": {
    "SecretKey": "YOUR_VERY_LONG_SECRET_KEY",
    "Issuer": "https://your-api-domain.example",
    "Audience": "https://your-frontend-domain.example",
    "AccessTokenDurationMinutes": 480,
    "RefreshTokenDurationDays": 7
  },
  "TokenCleanup": { "IntervalMinutes": 60 },
  "Wallet": { "InitialWhiteCoins": 5, "ExpireDays": 30 },
  "Streak": { "CycleDays": 7, "DailyCheckInRewards": [1, 1, 1, 1, 2, 2, 3] },
  "AiTarot": { "Costs": { "3": 2, "5": 3, "7": 4, "10": 5 } },
  "Email": {
    "ApiKey": "re_YOUR_API_KEY",
    "Endpoint": "https://api.resend.com",
    "FromAddress": "noreply@your-domain.com",
    "FromName": "My Tarot Reader"
  }
}
```

The `DevAuth` section is deliberately absent from this sample. It is only consumed by the
development-only `POST /api/dev/auth/token` route, which does not exist in Production, so the
defaults in `appsettings.json` are enough — leave it out unless you also run a Development
build locally.

Priority order, highest first:

1. the settings file (when present)
2. environment variables (`A__B`)
3. `appsettings.{Environment}.json`
4. `appsettings.json`

A key that the file does not mention still comes from the sources below it, so settings can be migrated to the file one by one. On startup the app logs whether the file was loaded — `Loaded secret settings from …` or `No secret settings file at …`, so a missing or misnamed file is never a silent fallback.

The path can be changed with the `SETTINGS_FILE` environment variable, which also makes the file testable locally. This is the one documented exception to the "always go through `Backend/scripts/*`" rule, because the scripts do not forward extra environment variables:

```bash
SETTINGS_FILE=./appsettings.Production.json ASPNETCORE_ENVIRONMENT=Production dotnet run --project src/Api
```

`*.Production.json` is git-ignored, so a local copy of the file is never committed.

### Render deployment checks

Before the first production deploy:

- Configure the Render Docker service with **Root Directory = `Backend`** and use `/health` as the health-check path.
- Verify the service port matches the port used by the ASP.NET container. `EXPOSE` in `dockerfile` is documentation only; it does not choose the port the process listens on.
- Render terminates TLS at its proxy. The app calls `UseHttpsRedirection` but configures **no** forwarded-header handling — there is no `UseForwardedHeaders` or `ForwardedHeadersOptions` anywhere in the codebase — so the process cannot tell that the original request already arrived over HTTPS. When an HTTPS port can be resolved the middleware answers with a `307` to `https://`, and because it runs before routing this also hits `/health`, which can loop through the proxy. Treat this as a gap in the code, not only a dashboard setting: either add forwarded-header handling, or confirm the middleware stays inert (it logs a warning and does nothing when no HTTPS port is resolvable) before trusting the health check.
- Apply EF Core migrations against the production PostgreSQL database before serving traffic. The API does not automatically migrate the database at startup.
- The current GitHub Actions workflow builds the backend but does not run backend unit tests. Treat `dotnet test` as a separate release gate until CI is updated.

## Install & Run

> Always use the scripts in `Backend/scripts/` — never raw `dotnet run` / `dotnet ef`.

```bash
# 1. Apply EF Core migrations (requires PostgreSQL running)
./scripts/update-database.sh        # Windows: update-database.cmd

# 2. Run the API (Development profile, Swagger enabled)
./scripts/run-local.sh              # Windows: run-local.cmd
```

The API runs at **http://localhost:5271** and Swagger UI at **http://localhost:5271/swagger** (Development only).

## API

- **Swagger:** `/swagger` (Dev environment only; production builds reference the contract via controllers). `Swashbuckle.AspNetCore` is a normal, non-`PrivateAssets` reference and `AddSwaggerGen` runs in **every** environment — only the `UseSwagger()` / `UseSwaggerUI()` middleware is gated to Development, so the OpenAPI document can still be generated from a production build.
- **Response envelope:** service results are always `ApiResponse<T>: { success, message, data }`. Reads return `200 OK` with the payload in `data`. Writes (`POST`/`PUT`/`DELETE`) normally return `200 OK` with `data = null` — except the routes that create and return a resource (`POST api/aiTarot`, the three `POST api/aiDeepTarot/*` spreads and `POST api/dev/auth/token`), which return the created reading (or token) in `data`. The one place the envelope does not apply is `[ApiController]` model-binding failures: no `InvalidModelStateResponseFactory` is registered, so a missing or malformed request body returns ASP.NET Core's RFC 7807 `ValidationProblemDetails` instead.
- **Errors:** service-thrown failures are `BaseException` subtypes (`BadRequestException`, `ValidationException`, `UnauthorizedException`, `ForbiddenException`, `NotFoundException`, `ConflictException`, `TooManyRequestsException`, `InternalServerException`) mapped by `GlobalExceptionMiddleware` to the envelope with i18n error keys (`error.<domain>.<camelCase>`). Two further branches are not exception types: a client disconnect becomes HTTP `499` with `error.system.requestAborted`, and any non-app exception becomes HTTP `500` with `error.system.internalServerError`.
- **Authentication:** JWT in **HttpOnly cookies** (`accessToken`, `refreshToken`); refresh tokens are rotated on every refresh and bound to the device fingerprint sent in the `X-Device-Id` header. Guest flows use Redis for draw cooldowns. An `Authorization: Bearer <token>` header is also honoured, and takes precedence over the cookie — that is what Swagger's **Authorize** button uses.
- **Health:** `/health` returns `200 OK` in every environment and bypasses the frontend CORS policy (separate `HealthCors` policy allows any origin). Point Render's health-check / wake-up URL at `https://<api>/health`.
- **Coins:** AI tarot readings spend **white** coins (`AiTarot:Costs`, drained FIFO from the oldest active `WhiteCoinBatch`). Deep tarot readings spend **red** coins instead — the cost is hardcoded per topic in `DeepTarotConstant` (`TwelveHouses` = 3, `TwelveMonths` = 3). The red balance is checked before Gemini is called and charged only once the answer has been generated; a shortfall returns `error.wallet.insufficientRedCoin` (400).

| Method         | Route                              | Description                                                                                                                                      | Auth   |
| -------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------ |
| `POST`         | `api/auth/oauth`                   | Google OAuth login/register                                                                                                                      | Public |
| `POST`         | `api/auth/refresh`                 | Rotate access token                                                                                                                              | Cookie |
| `POST`         | `api/auth/logout`                  | Logout, revoke tokens                                                                                                                            | Cookie |
| `GET`          | `api/auth/me`                      | Current user                                                                                                                                     | JWT    |
| `GET`          | `api/streak`                       | Current streak & check-in status                                                                                                                 | JWT    |
| `PUT`          | `api/streak/checkin`               | Daily check-in, grant coins                                                                                                                      | JWT    |
| `GET`          | `api/tarot/draw`                   | Last drawn card (auth)                                                                                                                           | JWT    |
| `POST`         | `api/tarot/draw`                   | Draw a card (auth)                                                                                                                               | JWT    |
| `GET`          | `api/tarot/guest-draw`             | Last drawn card (guest)                                                                                                                          | Public |
| `POST`         | `api/tarot/guest-draw`             | Draw a card (guest, Redis cooldown)                                                                                                              | Public |
| `DELETE`       | `api/tarot/guest-draw`             | Clear last drawn card (guest) — dev-only, never mapped in production                                                                             | Public |
| `GET`          | `api/tarot`                        | Reading history                                                                                                                                  | JWT    |
| `DELETE`       | `api/tarot/{readingId:guid}`       | Delete a reading (soft delete)                                                                                                                   | JWT    |
| `POST`         | `api/aiTarot`                      | Create an AI tarot reading (Gemini), persist result                                                                                              | JWT    |
| `GET`          | `api/aiTarot/{readingId:guid}`     | Get one AI tarot reading                                                                                                                         | JWT    |
| `GET`          | `api/aiTarot`                      | Get all AI tarot readings (short answer excerpt only)                                                                                            | JWT    |
| `DELETE`       | `api/aiTarot/{readingId:guid}`     | Delete an AI tarot reading (soft delete)                                                                                                         | JWT    |
| `POST`         | `api/aiDeepTarot/twelveHouses`     | Create a 12 astrological houses deep tarot reading (12 cards, Gemini), spends 3 red coins                                                        | JWT    |
| `POST`         | `api/aiDeepTarot/twelveMonths`     | Create a 12 months deep tarot reading (12 cards, Gemini), spends 3 red coins; the spread runs from the month after the current one               | JWT    |
| `POST`         | `api/aiDeepTarot/crossroads`       | Create a crossroads deep tarot reading (2–4 options, Gemini), spends 1 red coin per option; the spread holds 3 cards per option + 1 closing card | JWT    |
| `GET`          | `api/aiDeepTarot/{readingId:guid}` | Get one deep tarot reading                                                                                                                       | JWT    |
| `GET`          | `api/aiDeepTarot`                  | Get all deep tarot readings (short answer excerpt only)                                                                                          | JWT    |
| `DELETE`       | `api/aiDeepTarot/{readingId:guid}` | Delete a deep tarot reading (soft delete)                                                                                                        | JWT    |
| `GET` / `HEAD` | `health`                           | Health check — reachable from any origin (used to wake up Render)                                                                                | Public |
| `GET`          | `api/test/*`                       | Dev-only test endpoints (`ok`, `not-found`, `bad`, `validation`, `boom`); mapped only in the Development environment                             | Public |
| `POST`         | `api/dev/auth/token`               | **Dev-only** fake login — mints a real JWT for a seeded dev user, no Google OAuth needed; mapped only in the Development environment             | Public |
| `GET`          | `api/wallet`                       | Wallet balance + active white coin batches ordered by expiry                                                                                     | JWT    |
| `POST`         | `api/wallet/convert`               | Convert red coins to white coins (1 red = 2 white)                                                                                               | JWT    |

### Testing `[Authorize]` endpoints in Swagger

Every `JWT` route above needs a signed token. Since Google OAuth needs a real Google
account, the API ships a development-only login that issues a genuine JWT for a single
seeded user, so you can exercise protected routes straight from Swagger.

**1. Get a token** — expand `POST /api/dev/auth/token` and click **Execute**. The body is
optional, so it works with an empty request. Set the `X-Device-Id` header so the refresh
token is bound to a device:

```
X-Device-Id: swagger-dev
```

The response returns the dev user's identity, so you can copy it into path parameters:

```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "userId": "31914a3f-d572-4d70-9df9-6c18942a6c42",
    "email": "dev@my-tarot-reader.local",
    "whiteCoin": 1000,
    "role": "registered"
  }
}
```

**2a. Authorize with the token** — click **Authorize** at the top of the page and paste
the `accessToken`. An `Authorization: Bearer` header takes precedence over the
`accessToken` cookie, so this always wins even if you are also logged in on the frontend.

**2b. Or rely on the cookies** — the endpoint also sets the `accessToken` / `refreshToken`
HttpOnly cookies, so requests work without **Authorize**. Prefer 2a: browsers only send
`Secure` cookies over HTTPS (or `http://localhost`), and the header path is immune to that.

**3. Test the refresh flow** — `POST /api/auth/refresh` reads the cookie and requires the
same `X-Device-Id` value used in step 1, otherwise the token is treated as stolen and all
of that user's tokens are revoked.

Notes:

- The dev user is looked up by `ProviderKey` in the `DevAuth` settings section and reused
  across calls, so readings and streak state accumulate. Soft-delete the row from the
  database to start clean.
- Seeded with `DevAuth:InitialWhiteCoins` white coins so coin-deducting routes
  (`api/aiDeepTarot/*`, `api/aiTarot`) and the rewarding check-in (`api/streak/checkin`) stay
  testable. `api/tarot/draw` is free and does not touch the wallet. Add more coins via
  `api/wallet/convert` or a new dev login after raising the setting.
- Send `{ "role": "pro" }` in the body to switch the dev user's role.
- Without the `X-Device-Id` header, tokens are bound to `DevAuth:DefaultDeviceFingerprint`.
- The controller is marked `[DevelopmentOnly]`, so the route **does not exist** outside
  Development — it is removed from the application model, not merely blocked.

## Useful Commands

```bash
# Clean + restore + build the solution
./scripts/clean-build.sh            # Windows: clean-build.cmd

# Create a new EF Core migration
./scripts/add-migration.sh <MigrationName>

# Apply migrations to PostgreSQL
./scripts/update-database.sh

# Run the API locally
./scripts/run-local.sh

# Run the unit tests
dotnet test test/UnitTest/UnitTest.csproj
```

Every `scripts/*.sh` has a Windows `*.cmd` twin.

### Test suite

296 cases across 12 files in `test/UnitTest/`, run with EF Core InMemory (services) and Moq
(everything else). Scope is deliberately narrow: every test targets a service, a client or a
worker — **there are no controller tests**, so the route table, the auth attributes and the
`[DevelopmentOnly]` route gating above are not covered by automation. Verify those from Swagger.

## Code Conventions

Full conventions live in the repo skills — read them before writing code:

- `.agent/skills/backend-dotnet-architecture/SKILL.md` — layering, folders, DTO/controller/service naming, `ApiResponse<T>`, error codes, soft delete, settings via `IOptions<T>`, transactions, EF queries.
- `.agent/skills/backend-dotnet-testing/SKILL.md` — test naming (`<Method>_<Scenario>_<ExpectedResult>`), Arrange/Act/Assert, InMemory vs Moq, FluentAssertions.

Highlights:

- Controller `<N>Controller` uses service interface `I<N>Service`, and every action returns `IActionResult` wrapping `ApiResponse<T>`. Actions that take a body accept exactly one `<Fn>Request` and put that request's `<Fn>Result` in `data`; parameterless actions (`GET api/streak`, `PUT api/streak/checkin`, `GET api/wallet`, `GET api/tarot`, `POST api/auth/refresh` / `logout`, `GET api/auth/me`, the `guest-draw` routes and the dev/test controllers) build their payload in the controller instead.
- DTOs are `record`s, `Data` lists use the `Item` suffix (`GetAllReadingItem`); responses always wrapped in `ApiResponse<T>`.
- Services read via `AsNoTracking()` + `Select(...)`; writes use transactions; soft delete via `DeletedAt`; enums stored as strings.
- Validation via FluentValidation, never inline in controllers. `ValidationHelper` has two entry points: `ValidateOrThrow` raises a `BadRequestException` that carries only the i18n code in `message`, while `ValidateOrThrowForm` raises a `ValidationException` that also populates `data` with `FieldError` entries (`{ key, value }`) for per-field display.
- Time-dependent logic reads the clock through the injected `TimeProvider` (`StreakHelper.GetVietnamNow(_timeProvider)`), so tests can freeze it instead of depending on the day they run.
- Never read `appsettings*.json` directly in services — use `IOptions<T>`. (`Cors:FrontendUrl` and `Redis:Configuration` are the two exceptions: both are read eagerly from `IConfiguration` during startup because CORS and Redis are wired before any request is served.)
