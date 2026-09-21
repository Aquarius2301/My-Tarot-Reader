# Backend

ASP.NET Core 8 Web API for **My Tarot Reader**, built with Clean Architecture. It serves the tarot draw flows (guest + authenticated), the daily check-in streak & coin economy, Google OAuth, and AI tarot readings.

## Requirements

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) — primary database
- [Redis](https://redis.io/download/) — guest draw cooldown & device fingerprints (local, or Upstash-style `rediss://` URI)
- **Google OAuth credentials** (Client ID) — required for Google login
- **Google Gemini API key** — required for AI tarot readings
- **SMTP credentials** — optional, used for transactional email

## Architecture

Clean Architecture; dependencies point only inwards (toward Domain):

```
Api -> Infrastructure -> Application -> Domain
Api -> Application -> Domain
```

```
Backend
├── dockerfile
├── MyTarotReader.sln
├── scripts/                 # run-local, clean-build, add-migration, update-database (.sh + .cmd)
├── src/
│   ├── Api/                 # controllers, DI extensions, middlewares, Program.cs, appsettings.json
│   ├── Application/         # contracts (services, persistence), settings, constants/errors, exceptions, models, validators
│   ├── Domain/              # entities, enums, common — pure C#, no EF Core
│   └── Infrastructure/      # EF Core DbContext + migrations, service implementations, JWT/Google/email, templates
└── test/
    └── UnitTest/            # xUnit + Moq + EF Core InMemory + FluentAssertions
```

- **Domain** — entities (`User`, `TarotReading`, `Streak`, `Wallet`, `Order`, `RefreshToken`, ...), enums (`UserRole`, `QuestionType`, `CardCount`, ...). No EF Core.
- **Application** — service interfaces, DTO records (`<Fn>Request` / `<Fn>Result`), settings classes, error codes, `ApiResponse<T>`, FluentValidation validators.
- **Infrastructure** — `AppDbContext`, migrations, concrete services, JWT generator, Google auth validator, email handler, AI tarot client.
- **Api** — controllers (one per domain), DI wiring, `GlobalExceptionMiddleware`, `Program.cs`.

## Configuration

Configuration lives in `src/Api/appsettings.json` (placeholders in the repo — never commit real values). Override per environment via env vars or your deploy platform (e.g. Render env variables).

| Key | Meaning | Example |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=MyTarotReader;Username=postgres;Password=YourStrongPassword!` |
| `Redis:Configuration` | Redis connection string | `localhost:6379` |
| `Cors:FrontendUrl` | Allowed frontend origin (CORS, with credentials) | `https://your-frontend-domain.example` |
| `Google:ClientId` | Google OAuth client ID | `YOUR_PRODUCTION_CLIENT_ID.apps.googleusercontent.com` |
| `AiTarot:ApiKey` | Gemini API key | `YOUR_GEMINI_API_KEY` |
| `AiTarot:Model` | Gemini model name | `gemini-2.0-flash` |
| `AiTarot:MaxOutputTokens` | Max tokens per AI reading | `16384` |
| `AiTarot:Costs` | White coin cost per reading, keyed by card count (3/5/7/10) | `{ "3": 2, "5": 3, "7": 4, "10": 5 }` |
| `Jwt:SecretKey` | Signing key for JWT | `YOUR_VERY_LONG_SECRET_KEY_FOR_LOCAL_DEV_ENVIRONMENT` |
| `Jwt:Issuer` | JWT issuer | `https://localhost:7000` |
| `Jwt:Audience` | JWT audience (frontend origin) | `https://localhost:5173` |
| `Jwt:AccessTokenDurationMinutes` | Access token lifetime | `480` |
| `Jwt:RefreshTokenDurationDays` | Refresh token lifetime | `7` |
| `TokenCleanup:IntervalMinutes` | Interval for expiring-token cleanup job | `60` |
| `Wallet:InitialWhiteCoins` | New-user starting white coins | `5` |
| `Wallet:ExpireDays` | White coin batch expiry in days | `30` |
| `Streak:CycleDays` | Days in a check-in streak cycle | `7` |
| `Streak:DailyCheckInRewards` | Coins per day in the cycle | `[1, 1, 1, 1, 2, 2, 3]` |
| `Email:Host` / `Email:Port` / `Email:Username` / `Email:Password` | SMTP settings | `smtp.example.com` / `587` |
| `Email:FromAddress` / `Email:FromName` | Sender identity | `you@your-domain.com` / `My Tarot Reader` |
| `Email:EnableSsl` | TLS for SMTP | `true` |

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

- **Swagger:** `/swagger` (Dev environment only; production builds reference the contract via controllers).
- **Response envelope:** every response is `ApiResponse<T>: { success, message, data }`. Writes (`POST`/`PUT`/`DELETE`) return `200 OK` with `data = null`; reads return `200 OK` with the payload in `data`.
- **Errors:** thrown as `BaseException` subtypes (`BadRequestException`, `ValidationException`, `UnauthorizedException`, `ForbiddenException`, `NotFoundException`, `ConflictException`, `TooManyRequestsException`, `InternalServerException`) and mapped by `GlobalExceptionMiddleware` to the envelope with i18n error keys (`error.<domain>.<camelCase>`).
- **Authentication:** JWT in **HttpOnly cookies** (`accessToken`, `refreshToken`); refresh tokens are rotated on every refresh and bound to the device fingerprint sent in the `X-Device-Id` header. Guest flows use Redis for draw cooldowns.

| Method | Route | Description | Auth |
| --- | --- | --- | --- |
| `POST` | `api/auth/oauth` | Google OAuth login/register | Public |
| `POST` | `api/auth/refresh` | Rotate access token | Cookie |
| `POST` | `api/auth/logout` | Logout, revoke tokens | Cookie |
| `GET` | `api/auth/me` | Current user | JWT |
| `GET` | `api/streak` | Current streak & check-in status | JWT |
| `PUT` | `api/streak/checkin` | Daily check-in, grant coins | JWT |
| `GET` | `api/tarot/draw` | Last drawn card (auth) | JWT |
| `POST` | `api/tarot/draw` | Draw a card (auth) | JWT |
| `GET` | `api/tarot/guest-draw` | Last drawn card (guest) | Public |
| `POST` | `api/tarot/guest-draw` | Draw a card (guest, Redis cooldown) | Public |
| `GET` | `api/tarot` | Reading history | JWT |
| `DELETE` | `api/tarot/{readingId:guid}` | Delete a reading (soft delete) | JWT |
| `PUT` | `api/aiTarot` | Create an AI tarot reading (Gemini), persist result | JWT |
| `GET` | `api/aiTarot/{readingId:guid}` | Get one AI tarot reading | JWT |
| `GET` | `api/aiTarot` | Get all AI tarot readings (short answer excerpt only) | JWT |
| `GET` | `api/test/*` | Dev/test-only endpoints (`ok`, `not-found`, `bad`, `validation`, `boom`) | Public |

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

## Code Conventions

Full conventions live in the repo skills — read them before writing code:

- `.agent/skills/backend-dotnet-architecture/SKILL.md` — layering, folders, DTO/controller/service naming, `ApiResponse<T>`, error codes, soft delete, settings via `IOptions<T>`, transactions, EF queries.
- `.agent/skills/backend-dotnet-testing/SKILL.md` — test naming (`<Method>_<Scenario>_<ExpectedResult>`), Arrange/Act/Assert, InMemory vs Moq, FluentAssertions.

Highlights:

- Controller `<N>Controller` uses service interface `I<N>Service`; methods take exactly one `<Fn>Request` and return `<Fn>Result`.
- DTOs are `record`s, `Data` lists use the `Item` suffix (`GetAllReadingItem`); responses always wrapped in `ApiResponse<T>`.
- Services read via `AsNoTracking()` + `Select(...)`; writes use transactions; soft delete via `DeletedAt`; enums stored as strings.
- Validation via FluentValidation (`ValidationHelper`), never inline in controllers.
- Never read `appsettings*.json` directly in services — use `IOptions<T>`.