---
name: backend-dotnet-testing
description: >-
  Unit testing conventions for Khang's .NET backend projects (xUnit + Moq + EF Core InMemory + FluentAssertions), built on top of the backend-dotnet-architecture Clean Architecture conventions. MUST be used whenever writing, reviewing, or editing unit tests for .NET Services, Controllers, or Helpers — including test project setup, test class/method naming, Arrange-Act-Assert structure, mocking dependencies, and asserting exceptions. Trigger on requests like "write a unit test for...", "add tests for this service...", "test this controller...", "cover this method with tests...", even if the user doesn't explicitly mention xUnit, Moq, or FluentAssertions by name.
---

# Backend .NET Unit Testing

Standard unit testing conventions for Khang's .NET backend projects. Complements `backend-dotnet-architecture` — always check that skill too when the code under test involves Controllers/Services/DTOs, since the test must exercise the real conventions (e.g. `AsNoTracking`, `DeletedAt` filter, `AppException` types).

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
- **Moq** — mock plain dependencies/interfaces that don't involve LINQ-to-Entities query translation (e.g. `IEmailService`, `ICurrentUserService`, `IOptions<T>`).
- **EF Core InMemory** — use when the method under test builds an EF Core query (`Where`, `Select`, `AsNoTracking`, navigation properties, `HasQueryFilter`) — Moq cannot fake `IQueryable` translation reliably, so InMemory is the default for anything touching `IAppDbContext`.
- **FluentAssertions** — all assertions (`result.Should().Be(...)`, not `Assert.Equal(...)`).
- **coverlet.collector** — code coverage collection (via `dotnet test --collect:"XPlat Code Coverage"`, run through the project's script, not called directly).

## 2. Test project structure & naming

```
tests/
└── <ProjectName>.Tests
    ├── <ProjectName>.Tests.csproj
    ├── Services/
    │   ├── HistoryServiceTests.cs
    │   └── UserServiceTests.cs
    ├── Controllers/
    │   └── HistoryControllerTests.cs
    └── Common/
        └── TestDbContextFactory.cs
```

- Test project mirrors the folder layout of the code under test (`Services/`, `Controllers/`, ...).
- Test class name: `<ClassUnderTest>Tests` (e.g. `HistoryService` → `HistoryServiceTests`).
- Test method name: `<MethodUnderTest>_<Scenario>_<ExpectedResult>`
  Examples: `GetHistoryAsync_HistoryExists_ReturnsResult`, `GetHistoryAsync_HistoryNotFound_ThrowsNotFoundException`, `CreateHistoryAsync_ValidRequest_AddsEntityToDb`.
- One test class per class under test. One `[Fact]`/`[Theory]` per scenario — don't cram multiple unrelated assertions/scenarios into one test method.
- Structure every test body with `// Arrange`, `// Act`, `// Assert` comments, in that order.

## 3. Choosing InMemory vs Moq for `IAppDbContext`

**Use EF Core InMemory** when the service method runs an actual query against `DbSet<T>` (filters, projections, includes, query filters like `DeletedAt == null`). Create a fresh, isolated database per test using a random name so tests never leak data into each other:

```csharp
namespace ProjectName.Tests.Common;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
```

> Note: EF Core InMemory does not enforce global query filters (`HasQueryFilter`) by default in older versions — if your `DeletedAt` filter must be verified, assert it explicitly in the test (seed one soft-deleted record and confirm it's excluded), don't rely on the provider to enforce it silently.

**Use Moq** when `IAppDbContext` isn't the focus of the test (e.g. testing a method that primarily calls another injected service), or when mocking dependencies that aren't EF Core (`IEmailService`, `IDateTimeProvider`, `IOptions<AppSettings>`).

```csharp
var emailServiceMock = new Mock<IEmailService>();
emailServiceMock
    .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

## 4. Worked example: testing `HistoryService` (from backend-dotnet-architecture example)

### 4.1. `GetHistoryAsync` — query scenario → InMemory

```csharp
namespace ProjectName.Tests.Services;

public class HistoryServiceTests
{
    [Fact]
    public async Task GetHistoryAsync_HistoryExists_ReturnsResult()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var history = new History
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Question = "What does my future hold?",
            Result = "The Sun",
            CreatedAt = DateTime.UtcNow,
        };
        dbContext.Histories.Add(history);
        await dbContext.SaveChangesAsync();

        var sut = new HistoryService(dbContext);

        // Act
        var result = await sut.GetHistoryAsync(new GetHistoryRequest(history.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(history.Id);
        result.Question.Should().Be(history.Question);
    }

    [Fact]
    public async Task GetHistoryAsync_HistoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var sut = new HistoryService(dbContext);

        // Act
        var act = () => sut.GetHistoryAsync(new GetHistoryRequest(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetHistoryAsync_HistorySoftDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var dbContext = TestDbContextFactory.Create();
        var history = new History
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Question = "Deleted question",
            Result = "The Moon",
            CreatedAt = DateTime.UtcNow,
            DeletedAt = DateTime.UtcNow,
        };
        dbContext.Histories.Add(history);
        await dbContext.SaveChangesAsync();

        var sut = new HistoryService(dbContext);

        // Act
        var act = () => sut.GetHistoryAsync(new GetHistoryRequest(history.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
```

### 4.2. `CreateHistoryAsync` — write scenario → InMemory

```csharp
[Fact]
public async Task CreateHistoryAsync_ValidRequest_AddsEntityToDb()
{
    // Arrange
    var dbContext = TestDbContextFactory.Create();
    var sut = new HistoryService(dbContext);
    var request = new CreateHistoryRequest(Guid.NewGuid(), "Will I find love?", "The Lovers");

    // Act
    await sut.CreateHistoryAsync(request, CancellationToken.None);

    // Assert
    var saved = await dbContext.Histories.SingleOrDefaultAsync(x => x.UserId == request.UserId);
    saved.Should().NotBeNull();
    saved!.Question.Should().Be(request.Question);
    saved.Result.Should().Be(request.Result);
}
```

### 4.3. Example with Moq — non-EF dependency

```csharp
[Fact]
public async Task SendHistoryReportAsync_ValidRequest_CallsEmailServiceOnce()
{
    // Arrange
    var dbContext = TestDbContextFactory.Create();
    var emailServiceMock = new Mock<IEmailService>();
    emailServiceMock
        .Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    var sut = new HistoryService(dbContext, emailServiceMock.Object);

    // Act
    await sut.SendHistoryReportAsync(new SendHistoryReportRequest(Guid.NewGuid()), CancellationToken.None);

    // Assert
    emailServiceMock.Verify(
        x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## 5. FluentAssertions usage

- Prefer fluent chains over raw xUnit `Assert.*`: `result.Should().Be(expected)`, `result.Should().BeNull()`, `list.Should().HaveCount(3)`, `list.Should().ContainSingle(x => x.Id == id)`.
- For exceptions on async methods, capture the call as a delegate first, then assert:
  ```csharp
  var act = () => sut.SomeAsync(...);
  await act.Should().ThrowAsync<NotFoundException>()
      .WithMessage("*not found*");
  ```
- For collections returned from `Select` projections, assert on shape, not just count: `result.Should().BeEquivalentTo(expected)`.

## 6. What to cover per service method

For every public service method, write at minimum:

- **Happy path** — valid input → expected result / expected side effect (entity added/updated, dependency called).
- **Not-found / validation failure path** — triggers the correct `AppException` subtype.
- **Soft-delete / query-filter edge case** — if the method queries an entity that supports `DeletedAt`, verify soft-deleted records are excluded.
- **Boundary/edge input** — e.g. empty collections, `Guid.Empty`, max-length strings — only when the method's logic actually branches on them; don't pad tests with trivial cases that add no coverage.

## 7. Caution

- Only read `appsettings.json` for config; never read `appsettings.Development.json`.

Do not write tests for FluentValidation rules inside service tests — validator tests belong in their own `<Validator>Tests` class, asserting `validator.Validate(request).IsValid` and specific `Errors` per rule.

## Quick checklist for new tests

- [ ] Test class named `<ClassUnderTest>Tests`, one file under the mirrored folder
- [ ] Test method named `<Method>_<Scenario>_<ExpectedResult>`
- [ ] Body has `// Arrange`, `// Act`, `// Assert` sections
- [ ] EF Core query logic → InMemory DB via `TestDbContextFactory`, unique DB name per test
- [ ] Non-EF dependencies → Moq, with explicit `Setup` and `Verify` where behavior matters
- [ ] Assertions use FluentAssertions, not `Assert.*`
- [ ] Exception paths asserted via `act.Should().ThrowAsync<T>()`
- [ ] Soft-deleted (`DeletedAt`) records covered when relevant
- [ ] No test depends on execution order or shares mutable state with another test
