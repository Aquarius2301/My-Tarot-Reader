---
name: backend-dotnet-architecture
description: >-
  Architecture and coding conventions for .NET backend projects (Clean Architecture - Api → Application → Domain, Api → Application → Infrastructure → Domain). MUST be used whenever writing, reviewing, or editing .NET backend code for Khang — including creating Controllers, Services, DTOs, Entities, migrations, validation, exception handling, EF Core queries, or setting up a new project. Trigger even on generic requests like "create an API for...", "add a feature...", "write a service...", "fix this controller...", since these conventions apply to ALL .NET code written, not only when the user explicitly says "follow the convention".
---

# Backend .NET Architecture — My Tarot Reader

## Project structure & layering

Clean Architecture. Dependencies point only inwards (toward Domain):

```
Api -> Infrastructure -> Application -> Domain
Api -> Application -> Domain
```

- **Domain**: entities, enums, common. Pure C# — NO Entity Framework Core or external packages.
- **Application**: contracts (services, persistence, common), settings, constants, exceptions, models, validators.
- **Infrastructure**: DbContext, migrations, services, common infrastructure (JWT, email, Google), templates.
- **Api**: controllers, extensions (DI), helpers, middlewares, Program.cs.

Namespaces: `MyTarotReader.Api`, `MyTarotReader.Application`, `MyTarotReader.Domain`, `MyTarotReader.Infrastructure`.

## Canonical folder tree

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
│   │   ├── Common/
│   │   │   ├── Exceptions/
│   │   │   ├── Helpers/
│   │   │   ├── Models/
│   │   │   └── Validators/
│   │   ├── Constants/
│   │   │   ├── Errors/
│   │   │   └── Tarot/
│   │   ├── Contracts/
│   │   │   ├── Common/
│   │   │   ├── Persistence/
│   │   │   └── Services/
│   │   └── Settings/
│   ├── Domain/
│   │   ├── Domain.csproj
│   │   ├── Common/
│   │   ├── Entities/
│   │   └── Enums/
│   └── Infrastructure/
│       ├── Infrastructure.csproj
│       ├── Common/
│       ├── Persistence/
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Services/
│       └── Templates/
└── test/
    └── UnitTest/
```

File name must match class name. Avoid deviations like: `AuthExtension.cs` containing `AuthenticationExtension`, `IJwtGenerator.cs` containing `IJwtTokenGenerator`.

## Naming conventions

- Controller: `N + Controller`, eg: `AuthController`, `TarotReadingController`, `WalletController`.
- Service: `N + Service`, eg: `AuthService`, `TarotReadingService`. Interface: `I + N + Service` (eg `IAuthService`), placed in `Application/Contracts/Services/`.
- 1 controller uses 1 service; each service method uses exactly 1 request and 1 result with matching names.
  VD: `AuthController.GoogleLoginAsync(...)` -> `IAuthService.GoogleLoginAsync(GoogleLoginRequest) : GoogleLoginResult`.

## DTO conventions

- DTOs use `record`. Name: `FunctionName + Request` / `FunctionName + Result`, eg: `CreateDrawForAuthRequest`, `GetLastDrawnCardForAuthResult`.
- Request/Result are declared in the **same file as the service interface** in `Application/Contracts/Services/` (no separate DTO folder).
- List items inside a result use the `Item` suffix, eg: `GetAllReadingItem`, `GetAllReadingResult(List<GetAllReadingItem> Items)`.
- Use `ApiResponse<T>(bool Success, string? Message, T? Data)` + factories `ApiResponse.Success()/Success<T>()/Failure()/Failure<T>()` from `Application/Common/Models/ApiResponse.cs`.

## Controller

- Inject the service interface, never the implementation.
- Route as `api/<domain>`, eg `[Route("api/streak")]`.
- Attributes: `[ApiController]`, `[ProducesErrorResponseType(typeof(ApiResponse<object>))]`.
- `[ProducesResponseType(typeof(ApiResponse<X>), StatusCodes.Status200OK)]` — type always wrapped in `ApiResponse<T>`.
- POST/PUT/DELETE always return `Ok(ApiResponse.Success())` (data = null); GET returns `Ok(ApiResponse.Success(result))`. Reason: frontend uses TanStack Query to invalidate cache.
- Header `X-Device-Id` is the device fingerprint for both login and guest flows.
- Errors are handled by middleware; never return errors directly from the controller.

## Service

- Services live in `Infrastructure/Services/`, interfaces in `Application/Contracts/Services/`.
- Services always inject `IAppDbContext` (never `DbContext` directly).
- Read-only methods always use `AsNoTracking()` and `Select(...)` to fetch only needed fields, avoiding full-entity queries and EF tracking overhead.
  - Exception: when the entity must be updated after querying (eg `RefreshAsync` uses `.Include(User).IgnoreQueryFilters()` to rotate tokens).
- Soft delete by setting `DeletedAt`; no hard deletes.
- When a write flow spans multiple steps/services, wrap in a transaction:
  ```csharp
  await using var transaction = await _context.Database.BeginTransactionAsync(ct);
  // ... write steps ...
  await transaction.CommitAsync(ct);
  ```
- Non-critical async side effects (eg email) run fire-and-forget (`_ = SendWelcomeEmailAsync(...)`).
- Always use `async/await`; never `.Result`/`.Wait()` (avoids deadlocks).
- Always accept `CancellationToken cancellationToken = default` and pass it down.

## Domain & Persistence

- Entities inherit `BaseEntity` (`Id`, `CreatedAt`, `DeletedAt`). No EF Core in Domain.
- Relationships are configured at the **weak side**: `builder.HasOne(x => x.User).WithMany(x => x.Xs).HasForeignKey(x => x.UserId)` in the child's Configuration, not the parent's.
- Every Configuration adds the soft-delete global filter: `HasQueryFilter(x => x.DeletedAt == null)`.
- Enums stored as strings: `.HasConversion<string>().HasMaxLength(...)`.
- Add check constraints for invariants (eg `CK_Wallets_RedCoin_NonNegative`).
- Watch out for multiple cascade paths; use `DeleteBehavior.Restrict` for secondary FKs (eg `OrderDetail -> WhiteCoinBatch`).

## Settings

- Never read `appsettings.json` directly in services. Always use `IOptions<T>`.
- One settings class per domain in `Application/Settings/` (eg `JwtSetting`, `GoogleSetting`, `WalletSetting`, `StreakSetting`, `EmailSetting`, `AiTarotSetting`, `TokenCleanupSetting`).
- Bound in `Api/Extensions/SettingExtension.cs`.

## Exceptions & Error codes

- Throw HTTP errors via the exception hierarchy in `Application/Common/Exceptions/AppException.cs`: `BadRequestException` (400), `ValidationException` (400 w/ field errors), `UnauthorizedException` (401), `ForbiddenException` (403), `NotFoundException` (404), `ConflictException` (409), `TooManyRequestsException` (429), `InternalServerException` (500). All inherit `BaseException`.
- Never return errors directly from the controller; `GlobalExceptionMiddleware` maps `BaseException` → status + `ApiResponse.Failure(...)` (499 when the client cancels the request).
- Error codes live in `Application/Constants/Errors/*ErrorCode.cs`, i18n format `error.<domain>.<camelCase>` (eg `error.wallet.walletNotFound`).

## Validation

- Always use FluentValidation in `Application/Common/Validators/`; never validate directly in controllers.
- Call via `ValidationHelper.ValidateOrThrow(...)` (BadRequest) or `ValidateOrThrowForm(...)` (ValidationException w/ field errors).
- Register `IValidator<T>` in `Api/Extensions/DependencyInjectionExtension.cs`.

## Auth & Sessions

- JWT travels via **HttpOnly cookies** (`accessToken`, `refreshToken`), not headers. `Api/Extensions/AuthExtension.cs` reads the token from the cookie via `OnMessageReceived`.
- Device fingerprint comes from the `X-Device-Id` header, bound to the refresh token to detect theft (mismatch → revoke all tokens + throw Unauthorized).
- Guest flows use Redis (key `tarot:draw:{guestKey}`, cooldown TTL).
- Refresh tokens are always rotated (old one soft-deleted `DeletedAt = now`, new one issued).

## Comment conventions

- `<summary>` for classes, methods, properties — short, max 1 sentence.
- `<remarks>` for details longer than 1 sentence.
- `<param>` for important parameters.
- `<exception>` for exceptions a method may throw, eg:
  ```csharp
  /// <exception cref="NotFoundException">Thrown when the user is not found.</exception>
  ```
- `<returns>` briefly describing the result.
- Controller/Service classes may omit `<summary>`.

## Run / Migration

- Always use scripts in `scripts/` instead of `dotnet run` / `dotnet ef` directly (avoids environment-specific errors):
  - `./scripts/run-local.sh` — run the API locally (Windows: `run-local.cmd`).
  - `./scripts/add-migration.sh <MigrationName>` — create a new migration.
  - `./scripts/update-database.sh` — update the database.
  - `./scripts/clean-build.sh` — clean, restore, build.

## Quick checklist for new code

- [ ] Controller named `<N>Controller`, service `<N>Service` in `Infrastructure/Services/`, interface `I<N>Service` in `Application/Contracts/Services/`
- [ ] 1 controller uses 1 service; each service method has exactly 1 matching `<Fn>Request` / `<Fn>Result` record declared in the interface file
- [ ] DTOs are `record`s; list items use the `Item` suffix (`GetAllReadingItem`)
- [ ] Responses always wrapped in `ApiResponse<T>`; POST/PUT/DELETE return `Ok(ApiResponse.Success())` (data = null), GET returns `Ok(ApiResponse.Success(result))`
- [ ] `[ProducesResponseType(typeof(ApiResponse<X>), 200)]` + `[ProducesErrorResponseType(typeof(ApiResponse<object>))]` on controllers
- [ ] Controller injects the service interface; service injects `IAppDbContext` (never `DbContext` directly)
- [ ] Read-only methods use `AsNoTracking()` + `Select(...)` (exception: update-after-read like refresh-token rotation)
- [ ] Multi-step/multi-service writes wrapped in `BeginTransactionAsync`/`CommitAsync`
- [ ] Soft delete via `DeletedAt`; every Configuration has `HasQueryFilter(x => x.DeletedAt == null)`
- [ ] Relationships configured at the weak side (child Configuration); enum stored as string; `Restrict` where needed to avoid multiple cascade paths
- [ ] Settings via `IOptions<T>` (never read `appsettings.json` directly)
- [ ] Errors thrown through `BaseException` subtypes; error codes as i18n keys `error.<domain>.<camelCase>`; never returned directly from controllers
- [ ] Requests validated with FluentValidation via `ValidationHelper`, validators registered in DI
- [ ] `async`/`await` only (no `.Result`/`.Wait()`), `CancellationToken` threaded through
- [ ] XML docs: `<summary>` (1 sentence), `<remarks>`/`<param>`/`<exception>`/`<returns>` as needed
- [ ] File name matches class name; run via `scripts/*.{sh,cmd}`
