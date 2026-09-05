# MyTarotReader — Backend

> ⚠️ **Part of an app under active development.** The API and its contracts are not stable yet.

The backend of MyTarotReader is a **.NET 8 API** built with **Clean Architecture**, providing authentication, tarot card draws, AI-generated readings, AI chat, reading history, and a wallet coin system.

## Tech stack

- **.NET 8** (ASP.NET Core, EF Core 8)
- **Architecture**: Clean Architecture — 4 projects: `Api`, `Application`, `Infrastructure`, `Domain`
- **Database**: SQL Server (EF Core), **Redis** (StackExchange.Redis)
- **Auth**: Google OAuth (Google.Apis.Auth) + JWT bearer tokens
- **AI**: Google Gemini (`gemini-2.0-flash`)

## Solution layout

The solution `MyTarotReader.sln` (in `Backend/`) contains four projects:

```
Backend/
├── MyTarotReader.sln
├── dockerfile                  # multi-stage .NET 8 build → ASP.NET runtime image
├── scripts/                    # helper scripts (see below)
└── src/
    ├── Api/                    # HTTP layer: controllers, middleware, DI, config
    ├── Application/            # use cases, contracts, DTOs, exceptions, settings
    ├── Infrastructure/        # EF Core, migrations, external-service impls
    └── Domain/                 # pure entities and enums, no dependencies
```

### Dependency flow

Only these references are allowed — the `Api` project **never** injects `Infrastructure` directly:

```
Api ────────────► Application ────────────► Domain
Api ──► Infrastructure ──► Application ──► Domain
```

## Folder structure

```
src/Api/
├── Program.cs                 # pipeline: Swagger (dev), global exceptions, CORS, auth, controllers
├── Controllers/               # Auth, Tarot, History, AITarot, AiChat, Test
├── Extensions/                # All/Auth/Cors/Database/DI/Http/Redis/Setting extensions
├── Middleware/                 # GlobalExceptionMiddleware
├── Helpers/                   # CookieHelper, JwtHelper
└── Backgrounds/               # TokenCleanupBackgroundService

src/Application/
├── Constants/                 # TarotConstants
├── Contracts/Persistence/     # IAppDbContext
├── Contracts/Services/        # IAiChatService, IAiTarotService, IAuthService,
│                              # IHistoryService, ITarotService, IWalletService
├── Dtos/                      # request/response DTO records
├── Exceptions/                # AppException, ErrorMessageCode
└── Settings/                  # AiTarot, Google, Jwt, TokenCleanup, Wallet settings

src/Domain/
├── Common/BaseEntity.cs
├── Entities/                  # User, Wallet, RefreshToken, ReadHistory,
│                              # AIReadHistory, ChatMessage
└── Enums/                     # CardCount, ChatSessionStatus, QuestionType, Role

src/Infrastructure/
├── Persistence/AppDbContext.cs
├── Persistence/Configurations/   # EF Core entity configurations
├── Persistence/Migrations/       # generated EF Core migrations
└── Services/                     # AiChat, AiTarot, Auth, History, Tarot, Wallet
```

## Endpoints

All authenticated routes resolve the current user via `JwtHelper.GetUserId` (from the bearer token) — never from a request parameter.

| Controller | Route | HTTP | Purpose |
| --- | --- | --- | --- |
| `AuthController` | `api/v1/auth` | `POST oauth` | Google OAuth sign-in |
| | | `POST refresh` | Refresh access token |
| | | `POST logout` | Sign out (revoke session) |
| | | `GET me` | Get current user |
| `TarotController` | `api/v1/tarot` | `GET/POST draw` | Draw for authenticated user |
| | | `GET/POST/DELETE guest-draw` | Draw (and undo) as guest |
| `HistoryController` | `api/v1/history` | `GET` | List reading history |
| | | `DELETE {historyId}` | Delete one history entry |
| `AITarotController` | `api/v1/ai-tarot` | `POST reading` | One-shot AI reading from a draw |
| `AiChatController` | `api/v1/ai-chat` | `POST session` | Start an AI chat session |
| | | `POST chat` | Send a message to the AI reader |
| | | `POST reading` | Request a full reading from the chat |
| `TestController` | `api/test` | `GET ok / not-found / bad / validation / boom` | Exercise the global exception middleware |

## Domain model

- **`User`** — account, bound via Google OAuth; has a `Role` and a `Wallet`.
- **`Wallet`** — **white / red coins**, earned and spent on services such as AI readings/draws.
- **`RefreshToken`** — used with the JWT refresh flow; cleaned up by the background service.
- **`ReadHistory`** / **`AIReadHistory`** — persisted reading records shown on the history page.
- **`ChatMessage`** — a message within an `AiChat` session (status-tracked via `ChatSessionStatus`).

## Configuration

All values are read from `src/Api/appsettings*.json` (override with your own or environment variables / secrets):

| Section | What it configures |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Redis` | StackExchange.Redis connection (draw caching / guest state) |
| `Cors:FrontendUrl` | Allowed frontend origin |
| `Google:ClientId` | Google OAuth client ID |
| `AiTarot` | Gemini API key, model, max output tokens |
| `Jwt` | Secret, issuer, audience, token lifetimes |
| `TokenCleanup` | Refresh-token cleanup interval |
| `Wallet` | Initial white coins for new users |

## Prerequisites

- **.NET 8 SDK**
- **SQL Server** (e.g. start one with Docker: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrongPassword!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`)
- **Redis** (default `localhost:6379`)
- **Google OAuth** credentials (`ClientId`)
- **A Google Gemini API key**

## Run locally

The scripts under `Backend/scripts/` wrap the common .NET commands:

```bash
# Run the API (from Backend/)
./scripts/run-local.sh

# Force a clean bin/obj delete + fresh build (warnings as errors)
./scripts/clean-build.sh

# Add an EF Core migration (saved to src/Infrastructure/Persistence/Migrations)
./scripts/add-migration.sh <MigrationName>

# Apply pending migrations to the database
./scripts/update-database.sh
```

> `add-migration.sh` and `update-database.sh` restore the `dotnet-ef` tool first and target the `Infrastructure` project with the `Api` as startup project.

Once running, open the Swagger UI at `http://localhost:7000/swagger` (dev environment).

## Conventions (summary)

- Controllers: route shape `api/v{n}/{resource}`; methods are `Async`, take a `CancellationToken`, and return `Ok(ApiResponse.Success(...))`.
- `userId` always comes from `JwtHelper.GetUserId`, never a parameter.
- DTOs are `record`s with full XML docs; entities are `class`es with per-property XML docs.
- New entity → add a `DbSet` in `IAppDbContext`/`AppDbContext` + an EF Core `Configuration` + a migration.
- New service → register it in `DIExtension`.
- The full rules live in the project's [`CLAUDE.md`](../CLAUDE.md) and the **`dotnet-clean-architecture-api`** skill.

## CI

GitHub Actions (`.github/workflows/ci.yml`) builds the solution with **warnings treated as errors** on every push/PR to `main`, then deploys the backend to **Render** (Docker image from `Backend/dockerfile`) via a Deploy Hook when code is merged to `main`.
