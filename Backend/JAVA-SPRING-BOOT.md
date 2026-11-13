# Học Java (Spring Boot) qua project My Tarot Reader

> Tài liệu cá nhân: chuyển tư duy từ **ASP.NET Core 8 (Clean Architecture + CQRS/MediatR)** sang **Java 21 + Spring Boot 3** bằng cách bám sát chính codebase này.
>
> Mục tiêu: đủ kiến thức nền + sản phẩm portfolio để apply vị trí **Java Backend / Spring Boot Developer**.
>
> Quy ước trong tài liệu:
> - 🟦 = code .NET thật trong repo (kèm đường dẫn file).
> - 🟨 = code Java/Spring Boot tương đương.
> - ⚠️ = điểm **không map 1:1**, dễ bị hỏi khi phỏng vấn.

---

## Mục lục

1. [Tổng quan project hiện tại](#1-tổng-quan-project-hiện-tại)
2. [Bản đồ kiến trúc .NET → Spring Boot](#2-bản-đồ-kiến-trúc-net--spring-boot)
3. [Bảng ánh xạ tổng quát](#3-bảng-ánh-xạ-tổng-quát)
4. [Deep-dive có code song song](#4-deep-dive-có-code-song-song)
5. [Các điểm KHÔNG map 1:1](#5-các-điểm-không-map-11)
6. [Lộ trình học 10–12 tuần](#6-lộ-trình-học-1012-tuần)
7. [Bài tập port project theo phase](#7-bài-tập-port-project-theo-phase)
8. [Câu hỏi phỏng vấn gắn với project](#8-câu-hỏi-phỏng-vấn-gắn-với-project)
9. [Tài nguyên & checklist năng lực](#9-tài-nguyên--checklist-năng-lực)

---

## 1. Tổng quan project hiện tại

Backend là **ASP.NET Core 8 Web API** theo Clean Architecture, dùng **CQRS** qua MediatR.

### 1.1. Luồng phụ thuộc

```
Api  ──►  Infrastructure  ──►  Application  ──►  Domain
 │                                  │
 └──────────────►  Application  ───┘
```

- **Domain**: entity + enum thuần C#, không phụ thuộc EF Core.
- **Application**: use case (CQRS handler), interface, DTO, validator, `IAppDbContext`.
- **Infrastructure**: `AppDbContext`, cấu hình EF Core, migration, service cụ thể (JWT, Gemini, PayOS...), template email.
- **Api**: controller mỏng, DI wiring, middleware, `Program.cs`.

### 1.2. Vertical slice

Mỗi use case nằm gọn trong một thư mục:

```
Features/<Domain>/
├── Commands/<Feature>/
│   ├── <Feature>Command.cs              # record request
│   ├── <Feature>CommandHandler.cs       # xử lý
│   └── <Feature>CommandValidator.cs     # FluentValidation
└── Queries/<Feature>/
    ├── <Feature>Query.cs
    └── <Feature>QueryHandler.cs
```

Ví dụ thật:
- `src/Application/Features/Tarot/Commands/CreateDrawForAuth/CreateDrawForAuthCommand.cs`
- `src/Application/Features/Tarot/Commands/CreateDrawForAuth/CreateDrawForAuthCommandHandler.cs`
- `src/Application/Features/Tarot/Commands/CreateDrawForAuth/CreateDrawForAuthCommandValidator.cs`

### 1.3. Pattern đang dùng (sẽ map sang Java)

| Pattern | Ở đâu trong repo |
| --- | --- |
| CQRS + MediatR | `Features/**`, `DependencyInjectionExtension.cs` |
| Validation pipeline | `Common/Behaviors/ValidationBehavior.cs` |
| Response envelope | `Common/Models/ApiResponse.cs` |
| Exception → HTTP | `Common/Exceptions/AppException.cs`, `Middlewares/GlobalExceptionMiddleware.cs` |
| DbContext seam | `Application/Persistence/IAppDbContext.cs` |
| Settings mạnh kiểu | `Application/Settings/*`, `Api/Extensions/SettingExtension.cs` |
| Background job | `Application/Backgrounds/*` |
| Redis | `Features/Tarot/Commands/CreateDrawForGuest/*` |
| JWT cookie auth | `Api/Extensions/AuthExtension.cs` |
| Soft delete | `BaseEntity.DeletedAt`, `UserConfiguration.HasQueryFilter` |
| Pagination | `Common/Models/PaginationResponse.cs` |
| Unit test | `test/UnitTest/**` (xUnit + Moq + FluentAssertions) |

---

## 2. Bản đồ kiến trúc .NET → Spring Boot

### 2.1. Cấu trúc project

**🟦 .NET hiện tại**

```
Backend/
├── src/
│   ├── Api/                # composition root
│   ├── Application/        # use cases
│   ├── Domain/             # entities
│   └── Infrastructure/     # EF Core + service impls
└── test/UnitTest/
```

**🟨 Java gợi ý (Maven multi-module hoặc 1 module + package)**

```
backend/
├── pom.xml
├── src/main/java/com/mytarot/
│   ├── api/                # @RestController, @RestControllerAdvice, security entrypoint
│   ├── application/        # @Service use case, port interface
│   ├── domain/             # @Entity hoặc POJO thuần
│   └── infrastructure/     # JpaRepository, config, client impl
├── src/main/resources/
│   ├── application.yml
│   └── db/migration/       # Flyway
└── src/test/java/com/mytarot/
```

> **Gợi ý thực dụng:** bắt đầu bằng **1 Maven module + package theo layer**, dùng **ArchUnit** để ép hướng phụ thuộc `api → infrastructure → application → domain`. Khi đã quen mới tách multi-module (domain, application, infrastructure, api) — tương đương 4 `.csproj`.

### 2.2. Vai trò từng layer

| Layer .NET | Trách nhiệm | Layer Java |
| --- | --- | --- |
| `Domain` | Entity, enum, quy tắc thuần | `domain` (JPA entity hoặc record/POJO) |
| `Application` | Use case, interface, DTO, validation | `application` (`@Service`, port interface, DTO record) |
| `Infrastructure` | EF Core, service cụ thể, HTTP client | `infrastructure` (`JpaRepository`, config, adapter) |
| `Api` | Controller, middleware, DI, Program | `api` (`@RestController`, `@RestControllerAdvice`, `@Configuration`) |

---

## 3. Bảng ánh xạ tổng quát

| 🟦 .NET (project này) | 🟨 Java / Spring Boot | Ghi chú |
| --- | --- | --- |
| ASP.NET Core Web API | Spring Boot `spring-boot-starter-web` | MVC + embedded Tomcat |
| `Program.cs` | `@SpringBootApplication` main class | composition root |
| `builder.Services.AddScoped<...>` | `@Service`/`@Repository`/`@Bean` | IoC container |
| Constructor injection (`class X(Dep d)`) | Constructor injection (`X(Dep d)`) | Spring khuyến nghị constructor, không cần `@Autowired` nếu 1 constructor |
| `record` C# | `record` Java 16+ | khác biệt ở ⚠️ mục 5 |
| `Guid` | `UUID` | |
| `DateTimeOffset` | `OffsetDateTime` / `Instant` | DB map `timestamptz` |
| `int?`, `string?` nullable | `Optional<T>` / `@Nullable` | Java không có nullable reference type |
| LINQ `Where/Select/OrderBy` | Stream API / JPQL / Spring Data | |
| `enum` + `HasConversion<string>()` | `@Enumerated(EnumType.STRING)` | lưu tên enum, không ordinal |
| Entity + navigation properties | JPA `@Entity` + `@OneToMany`/`@ManyToOne` | ⚠️ lazy loading + N+1 |
| `BaseEntity` | `@MappedSuperclass` abstract class | |
| `IEntityTypeConfiguration<T>` | annotation trên entity hoặc `@Embeddable` | |
| `AppDbContext` | `EntityManager` / `JpaRepository` | |
| `IAppDbContext` | port interface + `JpaRepository` | |
| `DbSet<T>.Add()` + `SaveChangesAsync` | `repository.save()` / `@Transactional` | |
| EF `AsNoTracking()` | projection DTO + `@Transactional(readOnly = true)` | |
| EF `Select(...)` DTO | JPQL constructor expression / interface projection | |
| EF migrations | Flyway / Liquibase | |
| `HasQueryFilter(DeletedAt == null)` | Hibernate `@SQLRestriction` / `@SoftDelete` | |
| MediatR `IRequest` + `Handler` | `@Service` use case / Spring Modulith / Axon | ⚠️ không chuẩn 1:1 |
| MediatR `IPipelineBehavior` | AOP `@Aspect` / `HandlerInterceptor` / method validation | |
| FluentValidation + `ValidationBehavior` | Bean Validation `@Valid` + `ConstraintValidator` | ⚠️ |
| `GlobalExceptionMiddleware` | `@RestControllerAdvice` + `@ExceptionHandler` | |
| `BaseException` | `RuntimeException` hierarchy | |
| `ApiResponse<T>` | `record ApiResponse<T>` | |
| `IOptions<T>` | `@ConfigurationProperties` | |
| `IConfiguration` | `Environment` / `@Value` / `@ConfigurationProperties` | |
| `appsettings.json` + env `A__B` | `application.yml` + profiles + relaxed binding | |
| `IHostedService` / `BackgroundService` | `@Scheduled` / `TaskScheduler` / Quartz | |
| `PeriodicTimer` | `@Scheduled(fixedDelay=...)` | |
| `IServiceScopeFactory` | Spring bean scope (singleton/prototype/request) | ⚠️ |
| `IConnectionMultiplexer` (Redis) | `StringRedisTemplate` / `RedisTemplate` | |
| `IHttpClientFactory` / `AddHttpClient` | `RestClient` / `WebClient` / OpenFeign | |
| `JwtBearer` + cookie | Spring Security filter chain + jjwt | |
| `[Authorize]` | `.authorizeHttpRequests(...)` / `@PreAuthorize` | |
| Google ID token validation | Google API Client / OAuth2 | |
| `[DevelopmentOnly]` + convention | `@Profile("dev")` | |
| Swashbuckle `/swagger` | springdoc-openapi `/swagger-ui.html` | |
| Health check `/health` | Spring Boot Actuator `/actuator/health` | |
| CORS policy | `CorsConfigurationSource` / `WebMvcConfigurer` | |
| `CancellationToken` | interrupt / reactive cancellation | ⚠️ |
| xUnit `[Fact]` | JUnit 5 `@Test` | |
| Moq | Mockito | |
| FluentAssertions | AssertJ | |
| EF Core InMemory | H2 + `@DataJpaTest` / Testcontainers | |
| `dotnet test` | `mvn test` | |
| Dockerfile | Dockerfile (temurin JRE) | tương đương |

---

## 4. Deep-dive có code song song

### 4.1. Entity & BaseEntity

**🟦 `src/Domain/Common/BaseEntity.cs`**

```csharp
public class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeletedAt { get; set; } = null;
}
```

**🟦 `src/Domain/Entities/User.cs`**

```csharp
public class User : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string ProviderKey { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Registered;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public Wallet Wallet { get; set; } = null!;
    public List<TarotReading> TarotReadings { get; set; } = [];
}
```

**🟨 Java tương đương**

```java
// domain/BaseEntity.java
@MappedSuperclass
public abstract class BaseEntity {

    @Id
    @Column(nullable = false, updatable = false)
    private UUID id = UUID.randomUUID();

    @Column(nullable = false, updatable = false)
    private OffsetDateTime createdAt = OffsetDateTime.now(ZoneOffset.UTC);

    private OffsetDateTime deletedAt;

    // getters/setters (hoặc Lombok @Getter @Setter)
}
```

```java
// domain/User.java
@Entity
@Table(name = "users")
public class User extends BaseEntity {

    @Column(nullable = false)
    private String fullName;

    @Column(nullable = false, unique = true)
    private String email;

    @Column(nullable = false, unique = true)
    private String providerKey;

    @Enumerated(EnumType.STRING)
    @Column(nullable = false, length = 10)
    private UserRole role = UserRole.REGISTERED;

    @OneToMany(mappedBy = "user", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<RefreshToken> refreshTokens = new ArrayList<>();

    @OneToOne(mappedBy = "user", cascade = CascadeType.ALL)
    private Wallet wallet;

    @OneToMany(mappedBy = "user")
    private List<TarotReading> tarotReadings = new ArrayList<>();
}
```

**Kiến thức Java cần nắm:** annotation, JPA mapping, `UUID`, `OffsetDateTime`, enum, quan hệ entity, `CascadeType`, `mappedBy` (phía không sở hữu khoá ngoại).

---

### 4.2. DbContext → Repository

**🟦 `src/Application/Persistence/IAppDbContext.cs`**

```csharp
public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<TarotReading> TarotReadings { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**🟦 `src/Infrastructure/Persistence/AppDbContext.cs`**

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<User> Users { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
```

**🟨 Java: Spring Data JPA repository**

```java
// infrastructure/persistence/UserRepository.java
public interface UserRepository extends JpaRepository<User, UUID> {

    Optional<User> findByEmail(String email);

    Optional<User> findByProviderKey(String providerKey);

    boolean existsByEmail(String email);

    @Query("""
        select u from User u
        where u.deletedAt is null and u.id = :id
        """)
    Optional<User> findActiveById(@Param("id") UUID id);
}
```

```java
// infrastructure/persistence/TarotReadingRepository.java
public interface TarotReadingRepository extends JpaRepository<TarotReading, UUID> {

    Page<TarotReading> findByUserId(UUID userId, Pageable pageable);
}
```

> Handler không gọi `SaveChangesAsync`; Spring Data JPA tự `flush` khi transaction commit. Thay vào đó đặt `@Transactional` ở service.

**Kiến thức Java cần nắm:** Spring Data JPA, derived query (`findBy...`), `@Query` JPQL, `JpaRepository`, `Pageable`, transaction.

---

### 4.3. Entity configuration & soft delete

**🟦 `src/Infrastructure/Persistence/Configurations/UserConfiguration.cs`**

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.ProviderKey).IsUnique();
        builder.Property(x => x.Role).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.HasQueryFilter(u => u.DeletedAt == null); // soft delete
    }
}
```

**🟨 Java tương đương**

```java
@Entity
@Table(
    name = "users",
    uniqueConstraints = {
        @UniqueConstraint(name = "uk_users_email", columnNames = "email"),
        @UniqueConstraint(name = "uk_users_provider_key", columnNames = "provider_key")
    }
)
@SQLRestriction("deleted_at is null") // Hibernate 6.3+, thay cho @Where
public class User extends BaseEntity {
    // ...
    @Enumerated(EnumType.STRING)
    @Column(nullable = false, length = 10)
    private UserRole role;
}
```

> ⚠️ `@SQLRestriction` luôn thêm điều kiện vào mọi query (kể cả `findById`). Khi cần bỏ qua (giống `IgnoreQueryFilters()` trong `TokenCleanupBackground`), dùng native query hoặc `@Query(nativeQuery = true)`.
>
> Hibernate 6.4+ có `@SoftDelete` — tự động chuyển `DELETE` thành `UPDATE deleted=true`. Nếu dùng cột `deleted_at` thì `@SQLRestriction` phù hợp hơn.

**Kiến thức Java cần nắm:** `@Table`, `@UniqueConstraint`, `@SQLRestriction`, lifecycle callback (`@PrePersist`, `@PreUpdate`).

---

### 4.4. Command + Handler + Validator (vertical slice)

**🟦 `CreateDrawForAuthCommand.cs`**

```csharp
public record CreateDrawForAuthCommand(string CardCode, bool IsReversed, Guid UserId) : IRequest;
```

**🟦 `CreateDrawForAuthCommandHandler.cs`**

```csharp
public class CreateDrawForAuthCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateDrawForAuthCommand>
{
    public async Task Handle(CreateDrawForAuthCommand request, CancellationToken ct)
    {
        var entity = new TarotReading
        {
            CardCode = request.CardCode,
            IsReversed = request.IsReversed,
            UserId = request.UserId,
        };
        context.TarotReadings.Add(entity);
        await context.SaveChangesAsync(ct);
    }
}
```

**🟦 `CreateDrawForAuthCommandValidator.cs`**

```csharp
public class CreateDrawForAuthCommandValidator : AbstractValidator<CreateDrawForAuthCommand>
{
    public CreateDrawForAuthCommandValidator()
    {
        RuleFor(x => x.CardCode)
            .Must(TarotConstant.IsValidCardCode)
            .WithMessage(TarotReadingErrorCode.InvalidCardCode);
    }
}
```

**🟨 Java: dùng application service + Bean Validation**

```java
// application/tarot/CreateDrawForAuthCommand.java
public record CreateDrawForAuthCommand(
        @NotBlank String cardCode,
        boolean isReversed,
        @NotNull UUID userId
) {}
```

```java
// application/tarot/TarotApplicationService.java
@Service
public class TarotApplicationService {

    private final TarotReadingRepository repository;

    public TarotApplicationService(TarotReadingRepository repository) {
        this.repository = repository;
    }

    @Transactional
    public void createDrawForAuth(CreateDrawForAuthCommand command) {
        if (!TarotConstant.isValidCardCode(command.cardCode())) {
            throw new BadRequestException(TarotReadingErrorCode.INVALID_CARD_CODE);
        }
        repository.save(new TarotReading(
                command.cardCode(),
                command.isReversed(),
                command.userId()
        ));
    }
}
```

```java
// api/TarotReadingController.java
@RestController
@RequestMapping("/api/tarot")
public class TarotReadingController {

    private final TarotApplicationService service;

    public TarotReadingController(TarotApplicationService service) {
        this.service = service;
    }

    @PostMapping("/draw")
    public ApiResponse<Void> createDrawForAuth(
            @Valid @RequestBody CreateDrawForAuthRequest request,
            @AuthenticationPrincipal UUID userId) {

        service.createDrawForAuth(
                new CreateDrawForAuthCommand(request.cardCode(), request.isReversed(), userId));
        return ApiResponse.success();
    }
}
```

**Custom validation kiểu FluentValidation (khi rule phức tạp):**

```java
@Component
public class CardCodeValidator implements ConstraintValidator<ValidCardCode, String> {
    @Override
    public boolean isValid(String value, ConstraintValidatorContext ctx) {
        return value != null && TarotConstant.isValidCardCode(value);
    }
}

@Target({ElementType.FIELD, ElementType.PARAMETER})
@Retention(RetentionPolicy.RUNTIME)
@Constraint(validatedBy = CardCodeValidator.class)
public @interface ValidCardCode {
    String message() default "error.tarot.invalidCardCode";
    Class<?>[] groups() default {};
    Class<? extends Payload>[] payload() default {};
}
```

> ⚠️ Đây là chỗ **khác biệt lớn nhất**: MediatR dispatch handler qua pipeline; Spring không có sẵn "mediator". Chọn 1 trong 3 hướng:
> 1. **Application service** (`@Service`) — đơn giản, phổ biến nhất, khuyến nghị khi mới học.
> 2. **Spring Modulith** — có event/publication, gần với modular monolith.
> 3. **Axon Framework** — CQRS/Event Sourcing thật, nặng hơn, dùng khi phỏng vấn nhắc CQRS.

---

### 4.5. Validation pipeline

**🟦 `src/Application/Common/Behaviors/ValidationBehavior.cs`**

```csharp
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(request, ct)));
        var errors = results.SelectMany(r => r.Errors)
                            .Select(e => new FieldError(e.PropertyName, e.ErrorMessage))
                            .ToList();
        if (errors.Count > 0) throw new BadRequestException(fieldErrors: errors);
        return await next(ct);
    }
}
```

**🟨 Java: `@RestControllerAdvice` bắt `MethodArgumentNotValidException`**

```java
@RestControllerAdvice
public class GlobalExceptionHandler {

    @ExceptionHandler(MethodArgumentNotValidException.class)
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    public ApiResponse<List<FieldError>> handleValidation(MethodArgumentNotValidException ex) {
        List<FieldError> errors = ex.getBindingResult().getFieldErrors().stream()
                .map(e -> new FieldError(e.getField(), e.getDefaultMessage()))
                .toList();
        return ApiResponse.failure(SystemErrorCode.BAD_REQUEST, errors);
    }

    @ExceptionHandler(BaseException.class)
    public ResponseEntity<ApiResponse<Object>> handleApp(BaseException ex) {
        return ResponseEntity.status(ex.getStatusCode())
                .body(ApiResponse.failure(ex.getErrorCode(), ex.getFieldErrors()));
    }

    @ExceptionHandler(Exception.class)
    @ResponseStatus(HttpStatus.INTERNAL_SERVER_ERROR)
    public ApiResponse<Void> handleUnknown(Exception ex) {
        return ApiResponse.failure(SystemErrorCode.INTERNAL_SERVER_ERROR);
    }
}
```

**Kiến thức Java cần nắm:** Bean Validation, `@Valid`, `@Validated`, `BindingResult`, `@RestControllerAdvice`, `@ExceptionHandler`, `ResponseEntity`.

---

### 4.6. Response envelope

**🟦 `src/Application/Common/Models/ApiResponse.cs`**

```csharp
public record ApiResponse<T>(bool Success, string? Message = null, T? Data = default);

public static class ApiResponse
{
    public static ApiResponse<object> Success(string? message = null) => new(true, message, null);
    public static ApiResponse<T> Success<T>(T data, string? message = null) => new(true, message, data);
    public static ApiResponse<object> Failure(string? message = null) => new(false, message, null);
    public static ApiResponse<T> Failure<T>(string? message, T? errors = default) => new(false, message, errors);
}
```

**🟨 Java**

```java
// application/common/ApiResponse.java
public record ApiResponse<T>(boolean success, String message, T data) {

    public static <T> ApiResponse<T> success(T data) {
        return new ApiResponse<>(true, null, data);
    }

    public static ApiResponse<Void> success() {
        return new ApiResponse<>(true, null, null);
    }

    public static <T> ApiResponse<T> failure(String message, T errors) {
        return new ApiResponse<>(false, message, errors);
    }
}
```

> Jackson serialize record Java thành JSON `{ "success": true, "message": null, "data": {...} }` — giống hệt .NET. Có thể bỏ field `null` bằng `@JsonInclude(JsonInclude.Include.NON_NULL)`.

**Kiến thức Java cần nắm:** generic, `record`, static factory method, Jackson annotations.

---

### 4.7. Pagination

**🟦 `PaginationResponse.cs`**

```csharp
public record PaginationResponse<T>(List<T> Items, int Page, int PageSize, int TotalItems)
{
    int TotalPages { get; init; } = ...;
}
```

**🟦 `GetAllReadingQueryHandler.cs`** dùng `Skip/Take`.

**🟨 Java**

```java
// application/common/PaginationResponse.java
public record PaginationResponse<T>(
        List<T> items, int page, int pageSize, int totalItems, int totalPages) {

    public static <T> PaginationResponse<T> of(Page<T> springPage) {
        return new PaginationResponse<>(
                springPage.getContent(),
                springPage.getNumber() + 1,
                springPage.getSize(),
                (int) springPage.getTotalElements(),
                springPage.getTotalPages());
    }
}
```

```java
// application/tarot/GetAllReadingQuery.java
@Transactional(readOnly = true)
public PaginationResponse<GetAllReadingItem> getAllReading(UUID userId, int page, int pageSize) {
    Pageable pageable = PageRequest.of(
            Math.max(page - 1, 0),
            Math.min(Math.max(pageSize, 1), 50),
            Sort.by(Sort.Direction.DESC, "createdAt", "id"));

    return PaginationResponse.of(
            readingRepository.findByUserId(userId, pageable)
                    .map(r -> new GetAllReadingItem(
                            r.getId(), r.getCardCode(), r.isReversed(), r.getCreatedAt())));
}
```

**Kiến thức Java cần nắm:** `Pageable`, `PageRequest`, `Page`, `Sort`, projection DTO.

---

### 4.8. Settings & cấu hình

**🟦 `Application/Settings/WalletSetting.cs`**

```csharp
public class WalletSetting
{
    public int InitialWhiteCoins { get; set; } = 5;
    public int ExpireDays { get; set; } = 30;
}
```

**🟦 `Api/Extensions/SettingExtension.cs`**

```csharp
services.Configure<WalletSetting>(configuration.GetSection("Wallet"));
// dùng: class WalletService(IOptions<WalletSetting> walletSetting) { ... walletSetting.Value.ExpireDays ... }
```

**🟨 `application.yml` + `@ConfigurationProperties`**

```yaml
# src/main/resources/application.yml
wallet:
  initial-white-coins: 5
  expire-days: 30

spring:
  datasource:
    url: ${DATABASE_URL:jdbc:postgresql://localhost:5432/MyTarotReader}
    username: ${DB_USER:postgres}
    password: ${DB_PASSWORD:postgres}
  data:
    redis:
      url: ${REDIS_URL:redis://localhost:6379}

jwt:
  secret-key: ${JWT_SECRET_KEY:dev-only-secret}
  issuer: ${JWT_ISSUER:http://localhost:8080}
  audience: ${JWT_AUDIENCE:http://localhost:5173}
```

```java
// application/settings/WalletProperties.java
@ConfigurationProperties(prefix = "wallet")
public record WalletProperties(int initialWhiteCoins, int expireDays) {}
```

```java
@SpringBootApplication
@ConfigurationPropertiesScan
public class MyTarotApplication {
    public static void main(String[] args) {
        SpringApplication.run(MyTarotApplication.class, args);
    }
}
```

```java
@Service
public class WalletService {
    private final WalletProperties wallet;
    public WalletService(WalletProperties wallet) { this.wallet = wallet; }
    // wallet.expireDays()
}
```

> Java relaxed binding: `WALLET_EXPIRE_DAYS`, `SPRING_DATASOURCE_URL`... tương đương `A__B` env của .NET. Profile: `application-dev.yml` ≈ `appsettings.Development.json`, chọn bằng `SPRING_PROFILES_ACTIVE=dev`.

**Kiến thức Java cần nắm:** `@ConfigurationProperties`, relaxed binding, profile, `@ConfigurationPropertiesScan`, external config priority.

---

### 4.9. Exception hierarchy

**🟦 `AppException.cs`** — `BaseException` giữ `StatusCode`, `ErrorCode`, `FieldErrors`; các subclass `BadRequestException`, `NotFoundException`, `ForbiddenException`...

**🟨 Java**

```java
public abstract class BaseException extends RuntimeException {

    private final int statusCode;
    private final String errorCode;
    private final List<FieldError> fieldErrors;

    protected BaseException(int statusCode, String errorCode,
                            List<FieldError> fieldErrors, Throwable cause) {
        super(errorCode, cause);
        this.statusCode = statusCode;
        this.errorCode = errorCode;
        this.fieldErrors = fieldErrors == null ? List.of() : fieldErrors;
    }
    // getters
}

public class BadRequestException extends BaseException {
    public BadRequestException(String errorCode) { super(400, errorCode, List.of(), null); }
    public BadRequestException(String errorCode, List<FieldError> errors) { super(400, errorCode, errors, null); }
}

public class NotFoundException extends BaseException {
    public NotFoundException(String errorCode) { super(404, errorCode, List.of(), null); }
}

public class TooManyRequestsException extends BaseException {
    public TooManyRequestsException(String errorCode) { super(429, errorCode, List.of(), null); }
}
```

> Bắt ở `GlobalExceptionHandler` như mục 4.5. Dùng `RuntimeException` để Spring rollback transaction mặc định (checked exception không rollback trừ khi khai báo).

---

### 4.10. Controller & lấy user hiện tại

**🟦 `TarotReadingController.cs`**

```csharp
var userId = JwtHelper.GetUserId(HttpContext);
var result = await mediator.Send(new GetLastDrawnCardForAuthQuery(userId), cancellationToken);
return Ok(ApiResponse.Success(result));
```

**🟨 Java**

```java
@RestController
@RequestMapping("/api/tarot")
public class TarotReadingController {

    @GetMapping("/draw")
    public ApiResponse<LastDrawnCard> getLastDrawn(@AuthenticationPrincipal UUID userId) {
        return ApiResponse.success(service.getLastDrawnForAuth(userId));
    }

    @GetMapping
    public ApiResponse<PaginationResponse<GetAllReadingItem>> getAll(
            @RequestParam(defaultValue = "1") int page,
            @RequestParam(defaultValue = "10") int pageSize,
            @AuthenticationPrincipal UUID userId) {
        return ApiResponse.success(service.getAllReading(userId, page, pageSize));
    }
}
```

> `@AuthenticationPrincipal` lấy principal từ SecurityContext. Với guest header `X-Device-Id` thì dùng `@RequestHeader("X-Device-Id") String deviceId`.

---

### 4.11. DI wiring

**🟦 `DependencyInjectionExtension.cs`**

```csharp
services.AddScoped<IAppDbContext, AppDbContext>();
services.AddScoped<IWalletService, WalletService>();
services.AddHttpClient<IGeminiClientService, GeminiClientService>();
services.AddSingleton<ITimeProvider, VietnamTimeProvider>();
services.AddHostedService<TokenCleanupBackground>();
services.AddMediatR(...);
services.AddValidatorsFromAssembly(...);
```

**🟨 Java**

```java
// Stereotype annotation, không cần đăng ký thủ công
@Service
public class WalletService implements IWalletService { /* ... */ }

@Repository
public interface UserRepository extends JpaRepository<User, UUID> {}

// Bean cho implementation không tự annotation (vd. client bên thứ ba)
@Configuration
public class HttpClientConfig {

    @Bean
    public RestClient geminiRestClient(RestClient.Builder builder) {
        return builder.baseUrl("https://generativelanguage.googleapis.com").build();
    }
}

// Đổi implementation theo profile
@Configuration
public class TimeConfig {
    @Bean
    public ITimeProvider timeProvider() {
        return new VietnamTimeProvider();
    }
}
```

> Spring bean mặc định **singleton**; `@Scope("prototype")` / `@RequestScope` khi cần. Không cần `IServiceScopeFactory` — repository được inject trực tiếp.

**Kiến thức Java cần nắm:** `@Component`, `@Service`, `@Repository`, `@Configuration`, `@Bean`, `@Qualifier`, bean scope, lifecycle.

---

### 4.12. Background job

**🟦 `TokenCleanupBackground.cs`** — kế thừa `BackgroundService`, dùng `PeriodicTimer`, tạo scope để lấy `IAppDbContext`, xoá token hết hạn.

**🟨 Java: `@Scheduled`**

```java
@Component
public class TokenCleanupJob {

    private static final Logger log = LoggerFactory.getLogger(TokenCleanupJob.class);

    private final RefreshTokenRepository repository;

    public TokenCleanupJob(RefreshTokenRepository repository) {
        this.repository = repository;
    }

    @Scheduled(fixedDelayString = "${token-cleanup.interval-minutes:60}", timeUnit = TimeUnit.MINUTES)
    @Transactional
    public void cleanup() {
        int deleted = repository.deleteExpiredOrRevoked(OffsetDateTime.now(ZoneOffset.UTC));
        if (deleted > 0) log.info("Deleted {} expired or revoked refresh tokens", deleted);
    }
}
```

```java
@SpringBootApplication
@EnableScheduling
public class MyTarotApplication { /* ... */ }
```

> Với job dài/parallel nên dùng `ThreadPoolTaskScheduler` hoặc Quartz. `@Scheduled` chạy trên scheduler thread pool, không chặn request.

---

### 4.13. Redis (guest cooldown)

**🟦 `CreateDrawForGuestCommandHandler.cs`**

```csharp
var db = redis.GetDatabase();
var key = TarotConstant.KeyPrefix + request.GuestKey;
var set = await db.StringSetAsync(key, value, TarotConstant.DrawCooldown, When.NotExists);
if (!set) throw new TooManyRequestsException(TarotReadingErrorCode.DrawnAlready);
await db.StringIncrementAsync(TarotConstant.GuestTotalKey);
```

**🟨 Java**

```java
@Service
public class GuestDrawService {

    private final StringRedisTemplate redis;
    private final ObjectMapper objectMapper;

    public GuestDrawService(StringRedisTemplate redis, ObjectMapper objectMapper) {
        this.redis = redis;
        this.objectMapper = objectMapper;
    }

    public void create(CreateDrawForGuestCommand command) throws JsonProcessingException {
        String key = TarotConstant.KEY_PREFIX + command.guestKey();
        DrawRecord record = new DrawRecord(
                Instant.now().getEpochSecond(), command.cardCode(), command.isReversed());

        Boolean set = redis.opsForValue().setIfAbsent(
                key,
                objectMapper.writeValueAsString(record),
                TarotConstant.DRAW_COOLDOWN);

        if (!Boolean.TRUE.equals(set)) {
            throw new TooManyRequestsException(TarotReadingErrorCode.DRAWN_ALREADY);
        }
        redis.opsForValue().increment(TarotConstant.GUEST_TOTAL_KEY);
    }
}
```

**Kiến thức Java cần nắm:** `StringRedisTemplate`, `opsForValue`, TTL, atomic `setIfAbsent` (≈ `SET NX EX`), Jackson.

---

### 4.14. JWT + cookie + Security

**🟦 `AuthExtension.cs`** — JWT bearer, fallback đọc cookie `accessToken` nếu không có `Authorization` header.

**🟨 Java: filter đọc header trước, cookie sau**

```java
@Component
public class JwtCookieAuthFilter extends OncePerRequestFilter {

    private final JwtService jwtService;

    public JwtCookieAuthFilter(JwtService jwtService) {
        this.jwtService = jwtService;
    }

    @Override
    protected void doFilterInternal(HttpServletRequest req, HttpServletResponse res,
                                    FilterChain chain) throws ServletException, IOException {
        String token = resolveToken(req);
        if (token != null && SecurityContextHolder.getContext().getAuthentication() == null) {
            jwtService.parse(token).ifPresent(claims -> {
                var auth = new UsernamePasswordAuthenticationToken(
                        UUID.fromString(claims.getSubject()), null, List.of());
                SecurityContextHolder.getContext().setAuthentication(auth);
            });
        }
        chain.doFilter(req, res);
    }

    private String resolveToken(HttpServletRequest req) {
        String header = req.getHeader(HttpHeaders.AUTHORIZATION);
        if (header != null && header.startsWith("Bearer ")) {
            return header.substring(7); // header thắng cookie
        }
        if (req.getCookies() == null) return null;
        for (Cookie c : req.getCookies()) {
            if ("accessToken".equals(c.getName())) return c.getValue();
        }
        return null;
    }
}
```

```java
@Configuration
@EnableWebSecurity
public class SecurityConfig {

    @Bean
    public SecurityFilterChain filterChain(HttpSecurity http, JwtCookieAuthFilter jwtFilter)
            throws Exception {
        return http
                .csrf(csrf -> csrf.disable())
                .cors(Customizer.withDefaults())
                .sessionManagement(s -> s.sessionCreationPolicy(SessionCreationPolicy.STATELESS))
                .authorizeHttpRequests(auth -> auth
                        .requestMatchers("/api/auth/**", "/api/tarot/guest-draw", "/health").permitAll()
                        .anyRequest().authenticated())
                .addFilterBefore(jwtFilter, UsernamePasswordAuthenticationFilter.class)
                .build();
    }

    @Bean
    public PasswordEncoder passwordEncoder() { return new BCryptPasswordEncoder(); }
}
```

```java
// set cookie (≈ CookieHelper Append)
ResponseCookie cookie = ResponseCookie.from("accessToken", token)
        .httpOnly(true)
        .secure(true)
        .sameSite("Strict")
        .path("/")
        .maxAge(Duration.ofMinutes(480))
        .build();
response.addHeader(HttpHeaders.SET_COOKIE, cookie.toString());
```

**Kiến thức Java cần nắm:** Spring Security filter chain, `OncePerRequestFilter`, `SecurityContextHolder`, `UsernamePasswordAuthenticationToken`, `@AuthenticationPrincipal`, `ResponseCookie`, `@PreAuthorize`, CSRF, session stateless.

---

### 4.15. HTTP client (Gemini / PayOS / Resend)

**🟦 `services.AddHttpClient<IGeminiClientService, GeminiClientService>()`**

**🟨 Java với `RestClient` (Spring 6.1+, khuyến nghị)**

```java
@Service
public class GeminiClientService implements IGeminiClientService {

    private final RestClient restClient;
    private final GeminiProperties properties;

    public GeminiClientService(RestClient.Builder builder, GeminiProperties properties) {
        this.restClient = builder
                .baseUrl("https://generativelanguage.googleapis.com")
                .build();
        this.properties = properties;
    }

    public String generate(String prompt) {
        return restClient.post()
                .uri("/v1beta/models/{model}:generateContent?key={key}",
                        properties.models().get(0).model(), properties.apis().get(0))
                .body(Map.of("contents", List.of(Map.of("parts", List.of(Map.of("text", prompt))))))
                .retrieve()
                .body(String.class);
    }
}
```

**Kiến thức Java cần nắm:** `RestClient`, `WebClient` (reactive), retry/backoff (`Retryable`, Resilience4j), timeout, Jackson mapping, `@HttpExchange`/OpenFeign.

---

### 4.16. Testing

**🟦 `CreateDrawForAuthCommandHandlerTests.cs`**

```csharp
[Fact]
public async Task CreateDrawForAuth_Valid_StoresForUser()
{
    var db = TestDbContextFactory.Create();      // EF InMemory
    var userId = Guid.NewGuid();
    SeedUser(db, userId);
    var sut = new CreateDrawForAuthCommandHandler(db);

    await sut.Handle(new CreateDrawForAuthCommand(ValidCard, true, userId), CancellationToken.None);

    var entity = db.TarotReadings.Should().ContainSingle().Subject;
    entity.UserId.Should().Be(userId);
}
```

**🟨 Java: JUnit 5 + Mockito + AssertJ**

```java
@ExtendWith(MockitoExtension.class)
class TarotApplicationServiceTest {

    @Mock TarotReadingRepository repository;
    @InjectMocks TarotApplicationService service;

    @Test
    void createDrawForAuth_valid_storesForUser() {
        UUID userId = UUID.randomUUID();
        var command = new CreateDrawForAuthCommand("maj-00", true, userId);

        service.createDrawForAuth(command);

        ArgumentCaptor<TarotReading> captor = ArgumentCaptor.forClass(TarotReading.class);
        verify(repository).save(captor.capture());
        assertThat(captor.getValue().getUserId()).isEqualTo(userId);
        assertThat(captor.getValue().getCardCode()).isEqualTo("maj-00");
    }
}
```

**Repository test với DB thật (Testcontainers):**

```java
@DataJpaTest
@Testcontainers
class TarotReadingRepositoryTest {

    @Container
    static PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:16");

    @DynamicPropertySource
    static void props(DynamicPropertyRegistry registry) {
        registry.add("spring.datasource.url", postgres::getJdbcUrl);
        registry.add("spring.datasource.username", postgres::getUsername);
        registry.add("spring.datasource.password", postgres::getPassword);
    }

    @Autowired TarotReadingRepository repository;

    @Test
    void save_persistsReading() {
        TarotReading saved = repository.save(new TarotReading("maj-00", true, UUID.randomUUID()));
        assertThat(repository.findById(saved.getId())).isPresent();
    }
}
```

**Kiến thức Java cần nắm:** JUnit 5, `@Test`, `@ExtendWith`, Mockito `@Mock`/`@InjectMocks`/`verify`/`ArgumentCaptor`, AssertJ, `@DataJpaTest`, Testcontainers, ArchUnit.

---

### 4.17. Migration

**🟦 EF Core:** `scripts/add-migration`, `scripts/update-database`.

**🟨 Flyway**

```
src/main/resources/db/migration/
├── V1__init.sql
├── V2__add_white_coin_batch.sql
└── V3__add_index_users_email.sql
```

```yaml
spring:
  flyway:
    enabled: true
    locations: classpath:db/migration
```

> Tương đương `add-migration` = tạo file `V{n}__mô_tả.sql`; `update-database` = chạy app (Flyway tự migrate lúc startup). Có thể dùng `flyway-maven-plugin` để migrate riêng.

---

### 4.18. Swagger, health, CORS

| .NET | Spring Boot |
| --- | --- |
| `/swagger` (Swashbuckle) | `/swagger-ui.html` (springdoc) |
| `/health` | `/actuator/health` |
| `CorsExtension` | `CorsConfigurationSource` |
| `[ProducesResponseType]` | springdoc annotation (optional) |

```xml
<dependency>
    <groupId>org.springdoc</groupId>
    <artifactId>springdoc-openapi-starter-webmvc-ui</artifactId>
    <version>2.6.0</version>
</dependency>
<dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-actuator</artifactId>
</dependency>
```

```java
@Configuration
public class CorsConfig {
    @Bean
    public CorsConfigurationSource corsConfigurationSource(@Value("${cors.frontend-url}") String origin) {
        var config = new CorsConfiguration();
        config.setAllowedOrigins(List.of(origin));
        config.setAllowedMethods(List.of("GET", "POST", "PUT", "DELETE", "OPTIONS"));
        config.setAllowCredentials(true);
        var source = new UrlBasedCorsConfigurationSource();
        source.registerCorsConfiguration("/**", config);
        return source;
    }
}
```

---

## 5. Các điểm KHÔNG map 1:1

### 5.1. MediatR vs Spring

- **.NET:** `mediator.Send(command)` → pipeline behaviors → handler.
- **Java:** không có mediator built-in. Spring **khuyến nghị application service**. Nếu thích dispatch kiểu command bus, tự viết interface `CommandHandler<C>` + registry, hoặc dùng Spring Modulith/Axon.
- **Trả lời phỏng vấn:** "CQRS không đồng nghĩa MediatR. CQRS là tách Command/Query về mặt mô hình; MediatR chỉ là cơ chế dispatch. Trong Spring thường dùng application service hoặc tách `XxxCommandService` / `XxxQueryService`."

### 5.2. FluentValidation vs Bean Validation

- Bean Validation đặt rule ngay trên DTO/record (`@NotBlank`, `@Size`, custom `@Constraint`).
- Rule phụ thuộc DB hoặc logic phức tạp → viết `ConstraintValidator` có inject repository, hoặc validate trong service và ném `BadRequestException`.
- Không có "pipeline behavior" tự động; `@Valid` chạy nhờ `MethodValidationPostProcessor`.

### 5.3. EF Core vs JPA/Hibernate

- **Navigation property + lazy loading** dễ gây **N+1**. Trong .NET project dùng `AsNoTracking()` + `Select(...)`; Java tương đương **DTO projection** (constructor expression) hoặc `@EntityGraph`.
- EF `HasQueryFilter` ≈ `@SQLRestriction`; nhưng `@SQLRestriction` áp cho mọi query và không tắt dễ như `IgnoreQueryFilters`.
- `SaveChangesAsync()` gom nhiều thay đổi trong 1 transaction; Java dùng `@Transactional` + dirty checking.
- EF change tracker khá giống persistence context của JPA.

### 5.4. `IServiceScopeFactory`

- .NET background service sống singleton nên phải tự tạo scope để lấy scoped `DbContext`.
- Spring: `@Scheduled` bean là singleton nhưng repository là proxy an toàn; không cần scope factory. Nếu cần scope riêng, dùng `ObjectProvider` hoặc `@Scope`.

### 5.5. `CancellationToken`

- .NET truyền token xuyên suốt để huỷ request.
- Java servlet truyền thống không có cơ chế tương đương; với `CompletableFuture`/WebFlux có cancellation, hoặc dùng timeout. Đừng cố mô phỏng y hệt.

### 5.6. Nullable reference types

- C# `string?`, `null!` là cảnh báo compile-time.
- Java: `Optional<T>` cho return, `@Nullable`/`@NonNull` (JSpecify, Lombok), hoặc Bean Validation. **Không có** kiểm tra nullable mặc định.

### 5.7. `record` C# vs `record` Java

| | C# record | Java record |
| --- | --- | --- |
| Mutability | init-only, có `with` | immutable hoàn toàn |
| Kế thừa | record kế thừa record | không kế thừa class khác |
| Vị trí | thường top-level trong file | top-level hoặc nested |
| Dùng làm entity JPA | — | **không** (JPA cần no-arg constructor) |

> Vì vậy entity Java nên là **class** (có getter/setter hoặc Lombok), còn DTO/Command/Response dùng **record**.

---

## 6. Lộ trình học 10–12 tuần

> Giả định bạn đã biết lập trình cơ bản (C#/TypeScript) và có 10–15h/tuần.

### Tuần 1–2 — Java core + build tool

- Cú pháp Java, OOP, interface, abstract class, generic, `record`.
- Collections: `List`, `Map`, `Set`; `Stream` API (`map`, `filter`, `collect`, `groupingBy`).
- Exception checked/unchecked; `Optional`; `var`; `switch` expression.
- Concurrency cơ bản: `Thread`, `ExecutorService`, `CompletableFuture`; Java 21 virtual threads.
- **Maven**: `pom.xml`, dependency, plugin, `mvnw`. (Gradle nếu công ty dùng.)
- **Bài tập:** viết lại `TarotConstant`, `PaginationHelper`, `StreakHelper` bằng Java thuần + unit test.

### Tuần 3 — Spring Boot fundamentals

- `@SpringBootApplication`, auto-configuration, `application.yml`, profile.
- IoC/DI: `@Component`, `@Service`, `@Repository`, `@Configuration`, `@Bean`, constructor injection, bean scope.
- REST: `@RestController`, `@RequestMapping`, `@GetMapping`, `@RequestBody`, `@PathVariable`, `@RequestParam`, `ResponseEntity`.
- **Bài tập:** dựng `/api/tarot` trả `ApiResponse` tĩnh, chưa cần DB.

### Tuần 4 — JPA/Hibernate + PostgreSQL + Flyway

- `@Entity`, `@Id`, `@GeneratedValue`, `@Column`, `@Table`, quan hệ `@OneToMany`/`@ManyToOne`.
- Spring Data JPA repository, derived query, `@Query`.
- Transaction, `@Transactional(readOnly = true)`, dirty checking, N+1.
- Flyway migration.
- **Bài tập:** port `User`, `TarotReading`, `Wallet`, `WhiteCoinBatch`; viết repository + migration; test với Testcontainers.

### Tuần 5 — Validation + exception + envelope

- Bean Validation + custom `ConstraintValidator`.
- `@RestControllerAdvice`, `@ExceptionHandler`, `ResponseEntity`.
- `ApiResponse<T>`, `PaginationResponse<T>`.
- **Bài tập:** port toàn bộ `Common/` (ApiResponse, exception hierarchy, pagination, validation) + endpoint list có phân trang.

### Tuần 6 — Spring Security + JWT + OAuth Google

- Security filter chain, stateless, CORS/CSRF.
- JWT: tạo/verify (`jjwt`), `OncePerRequestFilter`, `SecurityContextHolder`.
- HttpOnly cookie, refresh token rotation, device binding.
- Google ID token (Google API Client) hoặc OAuth2 login.
- **Bài tập:** port `AuthController` + `AuthExtension` + `[Authorize]` tương đương.

### Tuần 7 — Redis + scheduled jobs

- `StringRedisTemplate`, TTL, `setIfAbsent` atomic, counter.
- `@Scheduled`, `@EnableScheduling`, `fixedDelay`/`cron`.
- **Bài tập:** port guest draw cooldown + token cleanup job.

### Tuần 8 — Testing chuyên sâu

- JUnit 5, Mockito, AssertJ.
- `@WebMvcTest` (controller slice), `@DataJpaTest` (repository slice), Testcontainers.
- ArchUnit: ép layer dependency.
- **Bài tập:** port 15–20 test hiện có, đặt tên `<Method>_<Scenario>_<ExpectedResult>`.

### Tuần 9–10 — Port feature end-to-end

- Tarot draw (auth + guest), history + pagination + soft delete.
- Wallet: add/deduct coin FIFO, `@Transactional`, pessimistic lock khi cần.
- Streak check-in.
- **Bài tập:** xem mục 7.

### Tuần 11 — AI integration + Docker + CI

- `RestClient` gọi Gemini, retry/backoff, timeout.
- Dockerfile `eclipse-temurin:21-jre`, multi-stage build.
- GitHub Actions: `mvn -B verify`, build image, deploy Render (Docker).
- **Bài tập:** hoàn thiện endpoint AI + deploy thử.

### Tuần 12 — Ôn phỏng vấn + portfolio

- Ôn câu hỏi mục 8.
- Viết README Java project, sơ đồ kiến trúc, so sánh .NET vs Java.
- Luyện giải thích trade-off.

---

## 7. Bài tập port project theo phase

> Mục tiêu: có một repo `my-tarot-reader-java` chạy được, không cần giống 100% nhưng cùng API contract.

### Phase 1 — Domain + entity

- Port `BaseEntity`, `User`, `TarotReading`, `Wallet`, `WhiteCoinBatch`, `Streak`, `Order`, `RefreshToken`, enum `UserRole`, `OrderType`...
- Chuyển `Guid` → `UUID`, `DateTimeOffset` → `OffsetDateTime`.
- ✅ Tiêu chí: `mvn compile` pass, có test tạo entity.

### Phase 2 — Repository + DB + migration

- Viết `JpaRepository` cho từng entity.
- Viết Flyway `V1__init.sql` khớp schema (unique index, enum string).
- Soft delete bằng `deleted_at`.
- ✅ Tiêu chí: `@DataJpaTest` với Testcontainers pass; CRUD cơ bản.

### Phase 3 — Auth

- Google login (có thể mock bằng dev endpoint trước).
- JWT + refresh token rotation, cookie HttpOnly, `X-Device-Id`.
- Security filter chain.
- ✅ Tiêu chí: `GET /api/auth/me` trả user từ JWT; refresh xoay token; sai device → revoke.

### Phase 4 — Tarot draw + Redis

- Auth draw: lưu `TarotReading`.
- Guest draw: `StringRedisTemplate.setIfAbsent` + TTL, trả 429 khi còn cooldown.
- History: pagination + soft delete.
- ✅ Tiêu chí: hành vi giống bảng API trong `Backend/README.md`.

### Phase 5 — Wallet + Streak (transaction)

- `@Transactional` cho add/deduct coin.
- FIFO white coin batch, kiểm tra hết hạn.
- Check-in streak theo chu kỳ 7 ngày.
- Cân nhắc `@Lock(PESSIMISTIC_WRITE)` hoặc optimistic `@Version` để tránh race condition.
- ✅ Tiêu chí: test đồng thời 2 request trừ coin không âm.

### Phase 6 — AI integration

- Gọi Gemini bằng `RestClient`, retry/backoff, failover nhiều key/model.
- Lưu kết quả đọc bài, trừ white/red coin đúng thời điểm.
- ✅ Tiêu chí: mock server test + timeout/retry test.

### Phase 7 — Test + CI + deploy

- Unit test handler/service, slice test controller/repository.
- ArchUnit cho layer.
- Dockerfile + GitHub Actions + deploy Render.
- ✅ Tiêu chí: pipeline xanh, có URL public + Swagger.

### Checklist hoàn thành portfolio

- [ ] Repo Java public, README có kiến trúc + sơ đồ.
- [ ] `docker compose up` chạy được (API + Postgres + Redis).
- [ ] Swagger public.
- [ ] CI pass (`mvn verify`).
- [ ] Có ít nhất 30 test.
- [ ] README so sánh ngắn .NET vs Java (thể hiện hiểu biết).

---

## 8. Câu hỏi phỏng vấn gắn với project

### Java core
1. `record` vs class thường? Vì sao không dùng record làm JPA entity?
2. `Optional` dùng khi nào, khi nào không nên?
3. `equals`/`hashCode` contract là gì? Ảnh hưởng khi dùng entity trong `HashSet`?
4. Checked vs unchecked exception; vì sao `BaseException` nên là `RuntimeException`?
5. Stream API: `map` vs `flatMap`, `collect` vs `reduce`, lazy evaluation.
6. `HashMap` hoạt động thế nào? `ConcurrentHashMap` khác gì?

### Spring core / DI
7. Bean scope: singleton, prototype, request. Bean singleton có thread-safe không?
8. Constructor injection vs field injection; vì sao khuyến nghị constructor?
9. `@Component` vs `@Service` vs `@Repository` vs `@Controller`.
10. Auto-configuration hoạt động thế nào? `@ConditionalOn...`.
11. `@Configuration` + `@Bean` vs `@Component`.
12. Vòng đời bean (lifecycle callback).

### JPA / Hibernate
13. N+1 là gì? Cách phát hiện và xử lý (fetch join, `@EntityGraph`, projection).
14. `@Transactional` hoạt động nhờ đâu? Vì sao gọi trong cùng class có thể mất transaction (self-invocation)?
15. `@Transactional(readOnly = true)` có tác dụng gì?
16. Optimistic vs pessimistic locking, khi nào dùng? (Liên hệ wallet coin deduction.)
17. `orphanRemoval` vs `CascadeType.REMOVE`.
18. `LazyInitializationException` nguyên nhân và cách tránh.
19. Soft delete: `@SQLRestriction` vs `@SoftDelete`, ưu nhược điểm.

### Security
20. Spring Security filter chain chạy thế nào? Filter của mình đặt ở đâu?
21. JWT stateless vs session; ưu nhược điểm.
22. Vì sao lưu JWT trong HttpOnly cookie? CSRF liên quan gì?
23. Refresh token rotation để làm gì? Bị đánh cắp thì phát hiện thế nào?
24. `@PreAuthorize` hoạt động nhờ đâu?

### Redis / concurrency / system design
25. Vì sao guest cooldown dùng Redis thay vì DB? TTL + atomic `SET NX` giải quyết gì?
26. Race condition khi trừ coin đồng thời — xử lý cách nào?
27. Idempotency webhook PayOS: làm sao tránh cộng coin 2 lần?
28. Retry/backoff khi gọi AI: exponential backoff + jitter, failover key/model.
29. Background job chạy multi-instance: làm sao tránh chạy trùng? (distributed lock Redis/ShedLock.)

### Testing / vận hành
30. Unit test vs integration test; dùng Testcontainers vì sao tốt hơn H2?
31. Làm sao test `@Transactional` thật sự commit/rollback?
32. ArchUnit bảo vệ kiến trúc thế nào?

---

## 9. Tài nguyên & checklist năng lực

### Tài liệu chính thức
- Java: https://dev.java/learn/ , https://docs.oracle.com/en/java/
- Spring Boot: https://docs.spring.io/spring-boot/
- Spring Data JPA: https://docs.spring.io/spring-data/jpa/
- Spring Security: https://docs.spring.io/spring-security/
- Spring Modulith: https://docs.spring.io/spring-modulith/
- Flyway: https://documentation.red-gate.com/flyway
- Testcontainers: https://java.testcontainers.org/
- ArchUnit: https://www.archunit.org/
- springdoc: https://springdoc.org/

### Sách / khóa (chọn 1–2)
- *Spring in Action* (Craig Walls)
- *High-Performance Java Persistence* (Vlad Mihalcea)
- *Effective Java* (Joshua Bloch) — đọc chọn lọc
- Baeldung cho ví dụ nhanh (đọc có chọn lọc, đối chiếu docs chính thức)

### Checklist năng lực trước khi apply

**Java core**
- [ ] Viết được generic class/method, hiểu wildcard.
- [ ] Dùng thành thạo Stream + Optional.
- [ ] Hiểu JVM memory, GC cơ bản, `equals/hashCode`.
- [ ] Dùng `ExecutorService`, `CompletableFuture`; biết virtual threads.

**Spring**
- [ ] Giải thích IoC/DI, bean lifecycle, scope.
- [ ] Viết REST API + validation + exception handler.
- [ ] Cấu hình `application.yml`, profile, `@ConfigurationProperties`.
- [ ] Viết test slice (`@WebMvcTest`, `@DataJpaTest`).

**JPA/DB**
- [ ] Mapping quan hệ, cascade, orphanRemoval.
- [ ] Nhận diện và sửa N+1.
- [ ] Transaction propagation + isolation cơ bản.
- [ ] Viết migration Flyway.

**Security**
- [ ] Dựng JWT stateless + refresh rotation.
- [ ] Hiểu filter chain, CORS, CSRF, cookie flags.

**Vận hành**
- [ ] Dockerize app + compose Postgres/Redis.
- [ ] CI GitHub Actions chạy test + build.
- [ ] Logging, health check (Actuator), cấu hình production.

**Phỏng vấn**
- [ ] Trả lời trôi chảy 10 câu ở mục 8 mà không cần nhìn.
- [ ] Giải thích được trade-off .NET vs Java trong project này.

---

## Phụ lục: package tree Java gợi ý

```
com.mytarot
├── MyTarotApplication.java
├── api
│   ├── controller
│   │   ├── AuthController.java
│   │   ├── TarotReadingController.java
│   │   ├── WalletController.java
│   │   └── ShopController.java
│   ├── advice
│   │   └── GlobalExceptionHandler.java
│   ├── security
│   │   ├── SecurityConfig.java
│   │   ├── JwtCookieAuthFilter.java
│   │   └── JwtService.java
│   └── dto
│       └── CreateDrawForAuthRequest.java
├── application
│   ├── common
│   │   ├── ApiResponse.java
│   │   ├── PaginationResponse.java
│   │   └── FieldError.java
│   ├── exception
│   │   ├── BaseException.java
│   │   ├── BadRequestException.java
│   │   └── NotFoundException.java
│   ├── settings
│   │   ├── WalletProperties.java
│   │   ├── JwtProperties.java
│   │   └── GeminiProperties.java
│   ├── tarot
│   │   ├── TarotApplicationService.java
│   │   └── TarotReadingRepository.java   (port)
│   ├── wallet
│   │   └── WalletService.java
│   └── auth
│       └── AuthService.java
├── domain
│   ├── BaseEntity.java
│   ├── entity
│   │   ├── User.java
│   │   ├── TarotReading.java
│   │   ├── Wallet.java
│   │   └── WhiteCoinBatch.java
│   └── enums
│       ├── UserRole.java
│       └── OrderType.java
└── infrastructure
    ├── persistence
    │   ├── UserRepository.java
    │   ├── TarotReadingRepositoryImpl.java
    │   └── JpaConfig.java
    ├── redis
    │   └── GuestDrawService.java
    ├── client
    │   ├── GeminiClient.java
    │   └── PayOsClient.java
    ├── scheduler
    │   └── TokenCleanupJob.java
    └── config
        └── CorsConfig.java
```

---

> **Ghi chú cuối:** tài liệu này cố ý bám vào code thật của `Backend/` để việc học có ngữ cảnh. Khi port, ưu tiên **hành vi API** (contract trong `Backend/README.md`) hơn là sao chép y nguyên cách tổ chức .NET — vì một số pattern như MediatR không phải idiom của Spring.
