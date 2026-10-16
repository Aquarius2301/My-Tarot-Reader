---
name: backend-dotnet-testing
description: >-
  Unit testing conventions for Khang's .NET backend projects (xUnit + Moq + EF Core InMemory + FluentAssertions), built on top of the backend-dotnet-architecture Clean Architecture / CQRS conventions. MUST be used whenever writing, reviewing, or editing unit tests for .NET command/query handlers, validators, clients or background workers — including test project setup, test class/method naming, Arrange-Act-Assert structure, mocking dependencies, and asserting exceptions. Trigger on requests like "write a unit test for...", "add tests for this handler...", "test this command...", "test this validator...", even if the user doesn't explicitly mention xUnit, Moq, or FluentAssertions by name.
---

# Backend .NET Unit Testing

Standard unit testing conventions for Khang's .NET backend projects. Complements `backend-dotnet-architecture` — always check that skill too when the code under test involves commands/queries/handlers/validators, since the test must exercise the real conventions (eg `AsNoTracking`, `DeletedAt` filter, `BaseException` types, `FieldError`).

## 1. Stack

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
  <PackageReference Include="xunit" Version="2.9.3" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.2" />
  <PackageReference Include="coverlet.collector" Version="6.0.0">
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

- **xUnit** — test framework (`[Fact]`, `[Theory]`).
- **Moq** — mock infrastructure service ports (`IWalletService`, `IGeminiClientService`, `IJwtTokenGeneratorService`, ...) and `IOptions<T>`.
- **EF Core InMemory** — the default for any handler under test, because handlers always query/write through `IAppDbContext`. Moq cannot fake `IQueryable` translation reliably.
- **FluentAssertions** — all assertions (`result.Should().Be(...)`, not `Assert.Equal(...)`).
- **coverlet.collector** — code coverage collection (via `dotnet test --collect:"XPlat Code Coverage"`).

## 2. Test project structure & naming

```
test/
└── UnitTest/
    ├── UnitTest.csproj
    ├── Common/
    │   └── TestDbContextFactory.cs
    ├── Features/                 # mirrors Application/Features
    │   └── <Domain>/
    │       ├── <Fn>CommandHandlerTests.cs
    │       ├── <Fn>CommandValidatorTests.cs
    │       └── <Fn>QueryHandlerTests.cs
    └── Backgrounds/
        └── <Worker>Tests.cs
```

- Test project mirrors the folder layout of the code under test: `test/UnitTest/Features/<Domain>/`, namespace `MyTarotReader.UnitTest.Features.<Domain>`.
- Test class name: `<ClassUnderTest>Tests` (eg `CheckInCommandHandler` → `CheckInCommandHandlerTests`, `CreateDrawForAuthCommandValidator` → `CreateDrawForAuthCommandValidatorTests`).
- Test method name: `<UseCase>_<Scenario>_<ExpectedResult>` — the "method" is the **use case name**, not `Handle`.
  Examples: `CheckIn_NoStreak_CreatesStreakAndRewardsFirstDay`, `GetStreak_NoStreak_ReturnsDefaultValues`, `Validate_InvalidCard_IsInvalidWithInvalidCardCode`.
- One test class per class under test. One `[Fact]`/`[Theory]` per scenario — don't cram multiple unrelated assertions/scenarios into one test method.
- Structure every test body with `// Arrange`, `// Act`, `// Assert` comments, in that order.
- **There are no controller tests.** Do not write them; the route table and auth attributes are verified from Swagger.

## 3. InMemory vs Moq for `IAppDbContext`

**Use EF Core InMemory** whenever the handler runs an actual query against the context (filters, projections, includes, query filters like `DeletedAt == null`). Create a fresh, isolated database per test so tests never leak data into each other. The shared factory also suppresses the InMemory transaction warning, because handlers call `BeginTransactionAsync` (a no-op under InMemory):

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyTarotReader.Infrastructure.Persistence;

namespace MyTarotReader.UnitTest.Common;

/// <summary>
/// Creates an isolated in-memory <see cref="AppDbContext"/> for each test, so tests
/// never leak data into one another. Transactions are logged as no-ops by the
/// InMemory provider; the warning is suppressed to keep handler tests clean.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}
```

> Note: EF Core InMemory does not enforce global query filters (`HasQueryFilter`) by default in older versions — if your `DeletedAt` filter must be verified, assert it explicitly in the test (seed one soft-deleted record and confirm it's excluded), don't rely on the provider to enforce it silently.

**Use Moq** for the handler's non-EF dependencies (infrastructure service ports, `IOptions<T>`). Always `Setup` the methods the handler calls and `Verify` where the side effect matters:

```csharp
var wallet = new Mock<IWalletService>();
wallet
    .Setup(x => x.AddCoinsAsync(
        It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(),
        It.IsAny<OrderType>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

## 4. Worked examples

### 4.1. Command handler test — `CreateSut()` helper returning `(Sut, Db, Mock<...>)`

```csharp
namespace MyTarotReader.UnitTest.Features.Streak;

public class CheckInCommandHandlerTests
{
    private static (CheckInCommandHandler Sut, AppDbContext Db, Mock<IWalletService> Wallet) CreateSut()
    {
        var db = TestDbContextFactory.Create();
        var wallet = new Mock<IWalletService>();
        wallet
            .Setup(x => x.AddCoinsAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<OrderType>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new CheckInCommandHandler(db, wallet.Object, Options.Create(new StreakSetting()));
        return (sut, db, wallet);
    }

    private static Streak SeedStreak(AppDbContext db, Guid userId, DateTime lastCheckIn, int current = 1)
    {
        var streak = new Streak { UserId = userId, CurrentStreak = current, LongestStreak = current, CycleDay = current, LastCheckIn = lastCheckIn };
        db.Streaks.Add(streak);
        db.SaveChanges();
        return streak;
    }

    [Fact]
    public async Task CheckIn_AlreadyCheckedInToday_ThrowsBadRequestAndRewardsNothing()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        SeedStreak(db, userId, StreakHelper.GetVietnamNow());

        // Act
        var act = () => sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(StreakErrorCode.AlreadyCheckedIn);
        wallet.Verify(
            x => x.AddCoinsAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<OrderType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
```

### 4.2. Query handler test — InMemory, assert on the projected result

```csharp
[Fact]
public async Task GetStreak_NoStreak_ReturnsDefaultValues()
{
    // Arrange
    var db = TestDbContextFactory.Create();
    var sut = new GetStreakQueryHandler(db, Options.Create(new StreakSetting()));

    // Act
    var result = await sut.Handle(new GetStreakQuery(Guid.NewGuid()), CancellationToken.None);

    // Assert
    result.CycleDay.Should().Be(0);
    result.CurrentStreak.Should().Be(0);
    result.IsSaverUsed.Should().BeFalse();
}
```

### 4.3. Validator test — direct `new`, assert `IsValid` + specific rule errors

```csharp
namespace MyTarotReader.UnitTest.Features.Tarot;

public class CreateDrawForAuthCommandValidatorTests
{
    private readonly CreateDrawForAuthCommandValidator _sut = new();

    [Theory]
    [InlineData("fake-card")]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_InvalidCard_IsInvalidWithInvalidCardCode(string cardCode)
    {
        // Arrange
        var command = new CreateDrawForAuthCommand(cardCode, false, Guid.NewGuid());

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should()
            .ContainSingle(e => e.PropertyName == nameof(CreateDrawForAuthCommand.CardCode))
            .Which.ErrorMessage.Should().Be(TarotReadingErrorCode.InvalidCardCode);
    }
}
```

## 5. FluentAssertions usage

- Prefer fluent chains over raw xUnit `Assert.*`: `result.Should().Be(expected)`, `result.Should().BeNull()`, `list.Should().HaveCount(3)`, `list.Should().ContainSingle(x => x.Id == id)`.
- For exceptions on async handlers, capture the call as a delegate first, then assert — including the domain error code:
  ```csharp
  var act = () => sut.Handle(new SomeCommand(...), CancellationToken.None);
  var ex = await act.Should().ThrowAsync<BadRequestException>();
  ex.Which.ErrorCode.Should().Be(SomeErrorCode.SomeError);
  ```
- For collections returned from `Select` projections, assert on shape, not just count: `result.Should().BeEquivalentTo(expected)`.
- Verify mocked side effects explicitly: `wallet.Verify(..., Times.Once)` / `Times.Never`.

## 6. What to cover per handler / validator

For every command/query handler, write at minimum:

- **Happy path** — valid input → expected result / expected side effect (entity added/updated, service port called).
- **Not-found / rejection path** — triggers the correct `BaseException` subtype, asserted via `ErrorCode`.
- **Soft-delete / query-filter edge case** — if the handler queries an entity that supports `DeletedAt`, verify soft-deleted records are excluded.
- **Side-effect verification** — when the handler calls an infrastructure port (`IWalletService`, `IEmailQueueService`, ...), `Verify` once/never with the expected arguments.
- **Boundary/edge input** — only when the handler's logic actually branches on it; don't pad tests with trivial cases that add no coverage.

For every validator, test each rule in its own `<Validator>Tests` class by constructing the validator directly and asserting `validator.Validate(request).IsValid` plus the specific `Errors` entry (property name + error code) — never inside a handler test.

## 7. Caution

- Only read `appsettings.json` for config; never read `appsettings.Development.json`.
- Handlers call `context.Database.BeginTransactionAsync(...)`; always go through `TestDbContextFactory.Create()` so the InMemory transaction warning is suppressed.
- Don't write controller tests — controller glue is verified manually from Swagger.

## Quick checklist for new tests

- [ ] Test class named `<ClassUnderTest>Tests`, one file under the mirrored `Features/<Domain>/` folder
- [ ] Test method named `<UseCase>_<Scenario>_<ExpectedResult>`
- [ ] Body has `// Arrange`, `// Act`, `// Assert` sections
- [ ] Handler tests: `CreateSut()` helper returning `(sut, db, mock...)`; EF via `TestDbContextFactory.Create()`, non-EF deps via Moq
- [ ] Validator tests: direct `new <Validator>()`, assert `IsValid` + specific `Errors`
- [ ] Assertions use FluentAssertions, not `Assert.*`
- [ ] Exception paths asserted via `act.Should().ThrowAsync<T>()` + `ex.Which.ErrorCode`
- [ ] Soft-deleted (`DeletedAt`) records covered when relevant
- [ ] Mocked infrastructure ports verified with `Times.Once`/`Times.Never` where the side effect matters
- [ ] No test depends on execution order or shares mutable state with another test
