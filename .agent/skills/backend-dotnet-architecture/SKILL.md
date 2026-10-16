---
name: backend-dotnet-architecture
description: >-
  Architecture and coding conventions for .NET backend projects (Clean Architecture with a CQRS application layer — Api → Application → Domain, Api → Infrastructure → Application → Domain). MUST be used whenever writing, reviewing, or editing .NET backend code — including creating Commands, Queries, Handlers, Validators, Controllers, DTOs, Entities, migrations, exception handling, EF Core queries, or setting up a new project. Trigger even on generic requests like "create an API for...", "add a feature...", "write a command...", "fix this controller...", since these conventions apply to ALL .NET code written, not only when the user explicitly says "follow the convention".
---

# Backend .NET Architecture — My Tarot Reader

## 1. Project structure & layering

Clean Architecture with a **CQRS application layer** (MediatR). Dependencies point only inwards (toward Domain):

```
Api -> Infrastructure -> Application -> Domain
Api -> Application -> Domain
```

- **Domain**: entities, enums, common. Pure C# — NO Entity Framework Core or external packages.
- **Application**: CQRS features (commands, queries, handlers, validators, pipeline behaviors), `IAppDbContext`, infrastructure service ports, settings, constants, exceptions, models. Contains no concrete infrastructure implementation.
- **Infrastructure**: DbContext, migrations, concrete infrastructure services, common infrastructure (JWT, email, Google, Gemini, PayOS), templates.
- **Api**: controllers, extensions (DI), helpers, middlewares, Program.cs.

Namespaces: `MyTarotReader.Api`, `MyTarotReader.Application`, `MyTarotReader.Domain`, `MyTarotReader.Infrastructure`.

## 2. Canonical folder tree

```
Backend
├── dockerfile
├── MyTarotReader.sln
├── scripts/
│   ├── add-migration.{sh,cmd}
│   ├── clean-build.{sh,cmd}
│   ├── run-local.{sh,cmd}
│   └── update-database.{sh,cmd}
├── src/
│   ├── Api/
│   │   ├── Api.csproj
│   │   ├── Api.http
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/
│   │   ├── Extensions/
│   │   ├── Helpers/
│   │   ├── Middlewares/
│   │   └── Properties/
│   ├── Application/
│   │   ├── Application.csproj
│   │   ├── Features/                 # CQRS slices, one folder per use case
│   │   │   └── <Domain>/
│   │   │       ├── Commands/
│   │   │       │   └── <Feature>/    # <Feature>Command.cs, <Feature>CommandHandler.cs, <Feature>CommandValidator.cs
│   │   │       ├── Queries/
│   │   │       │   └── <Feature>/    # <Feature>Query.cs, <Feature>QueryHandler.cs
│   │   │       └── Helpers/          # optional per-domain helpers
│   │   ├── Backgrounds/              # hosted services (EmailBackground, TokenCleanupBackground, PayOsWebhookConfirmBackground)
│   │   ├── Services/                 # I<N>Service infrastructure ports (concrete impls live in Infrastructure)
│   │   ├── Persistence/              # IAppDbContext.cs (the EF Core seam handlers depend on)
│   │   ├── Common/
│   │   │   ├── Behaviors/            # ValidationBehavior
│   │   │   ├── Exceptions/           # AppException.cs (BaseException + subtypes)
│   │   │   ├── Helpers/              # PaginationHelper, ReadingInputGuard, StreakHelper
│   │   │   └── Models/               # ApiResponse.cs, PaginationResponse.cs
│   │   ├── Constants/
│   │   │   ├── Errors/
│   │   │   ├── Shop/
│   │   │   └── Tarot/
│   │   └── Settings/
│   ├── Domain/
│   │   ├── Domain.csproj
│   │   ├── Common/
│   │   ├── Entities/
│   │   └── Enums/
│   └── Infrastructure/
│       ├── Infrastructure.csproj
│       ├── Common/
│       ├── Helpers/
│       ├── Persistence/
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Services/
│       └── Templates/
└── test/
    └── UnitTest/
        ├── Features/                 # mirrors Application/Features (handler + validator tests)
        ├── Backgrounds/
        └── Common/                   # TestDbContextFactory
```

File name must match the main class name. Avoid deviations like: `AuthExtension.cs` containing `AuthenticationExtension`, `IJwtGenerator.cs` containing `IJwtTokenGenerator`.

- A feature file may hold more than one type when they belong together: the request record and its `Result` record live in the same file (eg `GetStreakQuery.cs` holds both `GetStreakQuery` and `GetStreakResult`).
- Controller-level payload records (eg `CreateDrawForAuthRequest`) may be declared at the top of the controller file.
- Namespace convention for a feature is `MyTarotReader.Application.Features.<Domain>` (one namespace per domain, eg `...Features.Streak`, `...Features.Tarot`) — every file in a domain's command/query/helper slices shares that single namespace regardless of its folder depth.

## 3. Naming conventions

- Controller: `N + Controller`, eg `AuthController`, `TarotReadingController`, `WalletController`.
- Command: `N + Command` — a `record` implementing `IRequest<TResponse>`, eg `CheckInCommand : IRequest<Unit>`.
- Query: `N + Query` — a `record` implementing `IRequest<TResponse>`, eg `GetStreakQuery : IRequest<GetStreakResult>`.
- Handler: `N + CommandHandler` / `N + QueryHandler`, implementing `IRequestHandler<TRequest, TResponse>`, eg `CheckInCommandHandler`, `GetStreakQueryHandler`. The handler sits next to its request in the feature folder.
- Validator: `N + CommandValidator` (class **and** file), eg `CheckInCommandValidator`. Queries are usually not validated.
- Query result: `N + Result`, declared in the same file as the query, eg `GetStreakResult`.
- One controller action maps to exactly one request with matching names.
  VD: `StreakController.CheckInAsync(...)` -> `CheckInCommand` -> `CheckInCommandHandler`.

## 4. DTO conventions

- DTOs use `record`. Name: `FunctionName + Request` / `FunctionName + Result`, eg `CreateDrawForAuthRequest`, `GetLastDrawnCardForAuthResult`.
- The request/result records are declared in the **same file as the command/query** (no separate DTO folder). Query results are declared in the query file.
- Command handlers may return a value: `Unit` for fire-and-forget writes, or the newly created resource id (eg `CreateAiTarotReadingCommand : IRequest<Guid>`).
- List items inside a result use the `Item` suffix, eg: `GetAllReadingItem`, `GetAllReadingResult(List<GetAllReadingItem> Items)`.
- Paged list endpoints return `PaginationResponse<T>(List<T> Items, int Page, int PageSize, int TotalItems)`; pagination inputs are normalized with `PaginationHelper.Normalize(page, pageSize)`.
- Use `ApiResponse<T>(bool Success, string? Message, T? Data)` + factories `ApiResponse.Success()/Success<T>()/Failure()/Failure<T>()` from `Application/Common/Models/ApiResponse.cs`.

## 5. Controller

- Inject `IMediator` (never a handler or a service), and dispatch with `await mediator.Send(new SomeCommand(...), cancellationToken)`.
- Route as `api/<domain>`, eg `[Route("api/streak")]`.
- Attributes: `[ApiController]`, `[ProducesErrorResponseType(typeof(ApiResponse<object>))]`.
- `[ProducesResponseType(typeof(ApiResponse<X>), StatusCodes.Status200OK)]` — type always wrapped in `ApiResponse<T>`.
- POST/PUT/DELETE always return `Ok(ApiResponse.Success())` (data = null); GET returns `Ok(ApiResponse.Success(result))`. Reason: frontend uses TanStack Query to invalidate cache.
  - Exception: when the frontend needs the newly created resource id (eg `AiTarotReadingController.CreateAiTarotReadingAsync` returns `CreateAiTarotReadingResult`), the POST/PUT/DELETE endpoint may return `Ok(ApiResponse.Success(result))`.
- The authenticated user id is read from the JWT via `JwtHelper.GetUserId(HttpContext)`.
- Header `X-Device-Id` is the device fingerprint for both login and guest flows.
- Errors are thrown by handlers and mapped by middleware; never return errors directly from the controller.

## 6. Command / Query handlers

- Handlers live in `Features/<Domain>/{Commands,Queries}/<Feature>/`, one handler class per use case.
- Handlers depend on `IAppDbContext` for data access (never `AppDbContext` directly) and may inject infrastructure service ports (`Application/Services/`, eg `IWalletService`, `IGeminiClientService`) and `IOptions<T>`.
- Read-only queries always use `AsNoTracking()` and `Select(...)` to fetch only needed fields, avoiding full-entity queries and EF tracking overhead.
  - Exception: when the entity must be updated after querying (eg `CheckInCommandHandler` mutates the streak, `RefreshCommandHandler` rotates tokens) — query tracked, then `SaveChangesAsync`.
- Soft delete by setting `DeletedAt`; no hard deletes.
- When a write flow spans multiple steps/services, wrap in a transaction:
  ```csharp
  await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
  // ... write steps ...
  await transaction.CommitAsync(cancellationToken);
  ```
- Non-critical async side effects that outlive the request (eg the welcome email) run in the background via the email queue: enqueue an `IEmailQueueService` message **after** the DB transaction commits; a singleton hosted service (`Application/Backgrounds/EmailBackground`) delivers it in its own scope. Never `_ =` fire-and-forget, and never pass the request's `CancellationToken` to it.
- Always use `async/await`; never `.Result`/`.Wait()` (avoids deadlocks).
- Always accept `CancellationToken cancellationToken = default` and pass it down.
- Throw HTTP errors via the exception hierarchy (section 10) — do not return error objects.

## 7. Application services (infrastructure ports)

- `Application/Services/` holds **ports** for cross-cutting infrastructure capabilities only: `IJwtTokenGeneratorService`, `IGoogleAuthService`, `IEmailHandlerService`, `IEmailSenderService`, `IEmailQueueService`, `IEmailTemplateEngineService`, `IGeminiClientService`, `IPayOsClientService`, and `IWalletService`.
- Concrete implementations live in `Infrastructure/Services/`; register them in `Api/Extensions/DependencyInjectionExtension.cs`.
- **Do not create a use-case service anymore.** Business logic for a use case lives in its handler. `IWalletService` is the only service that still owns domain operations (red/white coin ledger) because several handlers share it.
- Never inject `AppDbContext` directly; use `IAppDbContext`.

## 8. Domain & Persistence

- Entities inherit `BaseEntity` (`Id`, `CreatedAt`, `DeletedAt`). No EF Core in Domain.
- Relationships are configured at the **weak side**: `builder.HasOne(x => x.User).WithMany(x => x.Xs).HasForeignKey(x => x.UserId)` in the child's Configuration, not the parent's.
- Every Configuration adds the soft-delete global filter: `HasQueryFilter(x => x.DeletedAt == null)`.
- Enums stored as strings: `.HasConversion<string>().HasMaxLength(...)`.
- Add check constraints for invariants (eg `CK_Wallets_RedCoin_NonNegative`).
- Watch out for multiple cascade paths; use `DeleteBehavior.Restrict` for secondary FKs.

## 9. Settings

- Never read `appsettings.json` directly in handlers/services. Always use `IOptions<T>`.
- One settings class per domain in `Application/Settings/` (eg `JwtSetting`, `GoogleSetting`, `WalletSetting`, `StreakSetting`, `EmailSetting`, `AiTarotSetting`, `TokenCleanupSetting`).
- Bound in `Api/Extensions/SettingExtension.cs`.

## 10. Exceptions & Error codes

- Throw HTTP errors via the exception hierarchy in `Application/Common/Exceptions/AppException.cs`: `BadRequestException` (400, can carry field errors), `UnauthorizedException` (401), `ForbiddenException` (403), `NotFoundException` (404), `ConflictException` (409), `TooManyRequestsException` (429), `InternalServerException` (500). All inherit `BaseException`.
- **There is no `Result<T>` type**: handlers return the DTO on success or throw on failure.
- Never return errors directly from the controller; `GlobalExceptionMiddleware` maps `BaseException` → status + `ApiResponse.Failure(...)` (499 when the client cancels the request).
- Error codes live in `Application/Constants/Errors/*ErrorCode.cs`, i18n format `error.<domain>.<camelCase>` (eg `error.wallet.walletNotFound`, `error.streak.alreadyCheckedIn`).

## 11. Validation

- Always use FluentValidation; never validate directly in controllers or handlers.
- A validator is co-located with its command in the feature folder and named `<Command>CommandValidator` (file and class), eg `CheckInCommandValidator : AbstractValidator<CheckInCommand>`.
- Validators are auto-discovered (`AddValidatorsFromAssembly`) and auto-executed by the single `ValidationBehavior<TRequest, TResponse>` pipeline behavior — handlers do **not** call validation manually.
- On failure `ValidationBehavior` throws `BadRequestException(fieldErrors: [...])`; `FieldError(string Key, string Value)` is defined in `Application/Common/Models/ApiResponse.cs`.
- Commands with no input (eg only a `UserId`) need no validator; queries are not validated.

## 12. Auth & Sessions

- JWT travels via **HttpOnly cookies** (`accessToken`, `refreshToken`), not headers. `Api/Extensions/AuthExtension.cs` reads the token from the cookie via `OnMessageReceived`.
- Device fingerprint comes from the `X-Device-Id` header, bound to the refresh token to detect theft (mismatch → revoke all tokens + throw Unauthorized).
- Guest flows use Redis (key `tarot:draw:{guestKey}`, cooldown TTL).
- Refresh tokens are always rotated (old one soft-deleted `DeletedAt = now`, new one issued).

## 13. Comment conventions

- `<summary>` for classes, methods, properties — short, max 1 sentence.
- `<remarks>` for details longer than 1 sentence.
- `<param>` for important parameters.
- `<exception>` for exceptions a method may throw, eg:
  ```csharp
  /// <exception cref="NotFoundException">Thrown when the user is not found.</exception>
  ```
- `<returns>` briefly describing the result.
- Controller/Handler classes may omit `<summary>`.

## 14. Run / Migration

- Always use scripts in `scripts/` instead of `dotnet run` / `dotnet ef` directly (avoids environment-specific errors):
  - `./scripts/run-local.sh` — run the API locally (Windows: `run-local.cmd`).
  - `./scripts/add-migration.sh <MigrationName>` — create a new migration.
  - `./scripts/update-database.sh` — update the database.
  - `./scripts/clean-build.sh` — clean, restore, build.

## 15. Caution

- Only read `appsettings.json` for config; never read `appsettings.Development.json`.

## Quick checklist for new code

- [ ] Use case is a `record` `<Fn>Command`/`<Fn>Query : IRequest<TResponse>` + `<Fn>Handler : IRequestHandler<,>` in `Features/<Domain>/{Commands,Queries}/<Fn>/`, namespace `Features.<Domain>`
- [ ] Controller named `<N>Controller`, injects `IMediator`, dispatches with `mediator.Send(new ..., cancellationToken)`
- [ ] Query results are `<Fn>Result` records declared in the query file; list items use the `Item` suffix (`GetAllReadingItem`)
- [ ] Responses always wrapped in `ApiResponse<T>`; POST/PUT/DELETE return `Ok(ApiResponse.Success())` (data = null), GET returns `Ok(ApiResponse.Success(result))` (exception: create endpoints that must return the new resource id may return `Ok(ApiResponse.Success(result))`)
- [ ] `[ProducesResponseType(typeof(ApiResponse<X>), 200)]` + `[ProducesErrorResponseType(typeof(ApiResponse<object>))]` on controllers
- [ ] Handlers inject `IAppDbContext` (never `DbContext`/`AppDbContext` directly); no use-case services (only infra ports from `Application/Services/`)
- [ ] Read-only queries use `AsNoTracking()` + `Select(...)` (exception: update-after-read like streak check-in or refresh-token rotation)
- [ ] Multi-step/multi-service writes wrapped in `BeginTransactionAsync`/`CommitAsync`
- [ ] Soft delete via `DeletedAt`; every Configuration has `HasQueryFilter(x => x.DeletedAt == null)`
- [ ] Relationships configured at the weak side (child Configuration); enum stored as string; `Restrict` where needed to avoid multiple cascade paths
- [ ] Settings via `IOptions<T>` (never read `appsettings.json` directly)
- [ ] Errors thrown through `BaseException` subtypes; error codes as i18n keys `error.<domain>.<camelCase>`; mapped only by `GlobalExceptionMiddleware` (no `Result<T>`)
- [ ] Validation via a co-located `<Command>CommandValidator : AbstractValidator<TCommand>`; run automatically by `ValidationBehavior` (never call a `ValidationHelper`)
- [ ] `async`/`await` only (no `.Result`/`.Wait()`), `CancellationToken` threaded through
- [ ] XML docs: `<summary>` (1 sentence), `<remarks>`/`<param>`/`<exception>`/`<returns>` as needed
- [ ] File name matches class name; run via `scripts/*.{sh,cmd}`
