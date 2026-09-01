---
name: backend-dotnet-architecture
description: Mandatory conventions for .NET backend code following a 4-layer Clean Architecture (Api, Application, Infrastructure, Domain). MUST use this skill whenever creating or editing a Controller, Service, Interface, DTO, Entity, EF Core Configuration, migration, or DI registration in a .NET project of this shape — even if the user only says "add a delete-history API", "create a service for X", "add entity Y" without mentioning the architecture explicitly. Applies to every CRUD operation, Controller/Service/DTO naming, response structure, exception handling, XML doc comments, performance rules, and DbContext/migration/DIExtension updates.
---

# .NET Clean Architecture — Backend Conventions

This skill describes the 4-layer architecture and the mandatory rules for generating .NET backend code. For any related task (adding an API, a service, an entity...), follow every rule below exactly — do not change the structure or naming on your own. All sections share one running example: creating a new `Product`.

## 1. Architecture & dependency flow

4 projects, dependencies flow one way only:

```
Api        -> Application -> Domain
Api        -> Infrastructure -> Application -> Domain
```

- **Api**: Controllers, background jobs, middleware, extensions (DI, Swagger, database...), helpers (JwtHelper...).
- **Application**: service interfaces (contracts) — `I{N}Service`, DTOs (Request/Response), settings. Application holds NO implementation.
- **Infrastructure**: implementations of the interfaces declared in Application (`{N}Service : I{N}Service`), persistence (DbContext, EF Core configurations, migrations).
- **Domain**: entities and enums only. No logic.

Hard rule: **Api never injects an Infrastructure service directly** — always inject through the interface declared in Application.

## 2. Naming rules

| Component | Rule                                                     | Example                                                            |
| --------- | -------------------------------------------------------- | ------------------------------------------------------------------ |
| Service   | `{N}Service`, N = Controller name (without "Controller") | `ProductController` → `ProductService`                             |
| Interface | `I` + service name                                       | `IProductService`                                                  |
| DTO       | method name + `Request` / `Response`                     | `CreateProductAsync` → `CreateProductRequest`, `ProductResponse`   |
| DTO file  | all DTOs of one service share a single file              | `ProductDtos.cs` holds every Request/Response of `IProductService` |

## 3. DTO — always a `record`

- Every DTO (Request/Response) is declared as a `record` with a primary constructor — never a `class` with `{ get; set; }` properties.
- Always include XML doc: one `<summary>` for the record, plus one `<param>` per field.

```csharp
/// <summary>
/// DTO returned with product info after a successful operation.
/// </summary>
/// <param name="Id">Unique identifier (GUID).</param>
/// <param name="Name">Product name.</param>
/// <param name="Price">Current selling price.</param>
/// <param name="CreatedAtUtc">Record creation time (UTC).</param>
public record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    DateTime CreatedAtUtc);
```

## 4. Domain (Entity)

- Entities are `class` (not `record`). Each property has its own XML `<summary>`, and the class has a `<summary>` describing the corresponding table.

```csharp
/// <summary>
/// Represents the [Products] table in the database.
/// Holds product info and its business status.
/// </summary>
public class Product
{
    /// <summary>
    /// Primary key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
```

When adding a **new entity**, update all of the following (no step skipped):

1. Create the entity in `Domain/Entities` (with XML doc as above).
2. Add `DbSet<TEntity>` to `IAppDbContext`.
3. Add `DbSet<TEntity>` to `AppDbContext`.
4. Add the matching `EntityTypeConfiguration` (declare relationships with other entities if any) in `Infrastructure/Persistence`.
5. Run add-migration / update-database using the **script already in the project** (don't run ad-hoc `dotnet ef` commands when the project already has a standard script — check the scripts folder first)

```
./scripts/add-migration.sh <name>
./scripts/update-database.sh
```

## 5. Controller rules

- RESTful route **with versioning**: `api/v{n}/{resource}` (e.g. `api/v1/products`).
- All controller files are in `Api/Controllers`, and the class name is `{N}Controller` (e.g. `ProductController`).
- Controller methods **always** end in `Async`, **always** take a `CancellationToken`.
- Always return `Ok(ApiResponse.Success())` or `Ok(ApiResponse.Success(data))` — never return a raw object.
- **Never** accept `userId` from client/route/query — always get it via `JwtHelper.GetUserId(HttpContext)`.
- Controllers only call Application interfaces (`I{N}Service`) — never call an Infrastructure class directly.
- Validate input **in the Controller** when validation doesn't need the DB (DB-dependent validation goes in the Service).
- Errors always come from the exceptions predefined in `AppException` — don't create ad-hoc exceptions.
- **Always** add an XML doc comment above every controller method: `<summary>`, `<param>` per parameter, `<returns>`, and one `<response code="...">` per `[ProducesResponseType]` declared below it.

```csharp
/// <summary>
/// Creates a new product in the system.
/// </summary>
/// <param name="request">The product info to create.</param>
/// <param name="cancellationToken">Cancellation token from the client.</param>
/// <returns>The newly created product.</returns>
/// <response code="201">Product created successfully.</response>
/// <response code="400">Invalid input (missing name, negative price...).</response>
/// <response code="409">A product with this name already exists.</response>
[HttpPost]
[ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<IActionResult> CreateAsync(
    [FromBody] CreateProductRequest request,
    CancellationToken cancellationToken)
{
    var result = await _productService.CreateProductAsync(request, cancellationToken);
    return Ok(ApiResponse.Success(result));
}
```

## 6. Application rules (interface)

- All service interfaces are declared in `Application/Contracts/Services` and named `I{N}Service`.
- Interface methods are also `Task`/`Task<T>` async, taking `CancellationToken` as the last parameter.
- **Read** methods (Get...) → always return a response (never `void`/`Task`).
- **Create / edit / delete** methods → return `Task` (void), no data.
- **Always** add full XML doc on the interface method: `<summary>`, `<param>` per parameter, `<returns>`, and `<exception>` when the method can throw a clearly identifiable exception (duplicate data, record not found...).

```csharp
/// <summary>
/// Handles creating a new product and persisting it to the database.
/// </summary>
/// <param name="request">DTO with the product's initial data.</param>
/// <param name="cancellationToken">Token to observe for task cancellation.</param>
/// <returns>The newly created <see cref="ProductResponse"/>.</returns>
/// <exception cref="InvalidOperationException">Thrown when the product name already exists.</exception>
Task<ProductResponse> CreateProductAsync(
    CreateProductRequest request,
    CancellationToken cancellationToken = default);
```

## 7. Infrastructure rules (implementation)

- All service implementations are in `Infrastructure/Services` and named `{N}Service : I{N}Service`.
- Always inject `IAppDbContext` (interface), never inject `AppDbContext` directly.
- Read queries (Get...) → always use `.AsNoTracking()`.
- No unnecessary tracking/transactions for read-only operations.
- Comments in the implementation are **optional** — add them only when the flow needs clarifying; not mandatory like Controller/Interface:
  - Start with `/// <inheritdoc />` (inherits the interface's doc).
  - If the logic has several steps, add a `<remarks>` block listing them.
  - Plain `//` comments inside the method body are fine when a step needs explaining.

```csharp
/// <inheritdoc />
/// <remarks>
/// Flow:
/// 1. Check for a duplicate product name in the DB (case-insensitive).
/// 2. Map <see cref="CreateProductRequest"/> to <see cref="Product"/>.
/// 3. Save and return the resulting <see cref="ProductResponse"/>.
/// </remarks>
public async Task<ProductResponse> CreateProductAsync(
    CreateProductRequest request,
    CancellationToken cancellationToken = default)
{
    // 1. Check for duplicate name (case-insensitive)
    var isDuplicate = await _context.Products
        .AsNoTracking()
        .AnyAsync(p => p.Name.ToLower() == request.Name.ToLower(), cancellationToken);
    if (isDuplicate)
        throw new InvalidOperationException("Product name already exists.");

    // 2. Map DTO -> Entity
    var product = new Product
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        Price = request.Price,
        CreatedAtUtc = DateTime.UtcNow
    };

    // 3. Save and return
    _context.Products.Add(product);
    await _context.SaveChangesAsync(cancellationToken);

    return new ProductResponse(product.Id, product.Name, product.Price, product.CreatedAtUtc);
}
```

Simple, read-only methods (e.g. `DeleteHistoryAsync` that just soft-deletes one record) do **not** need `<remarks>` or inline comments — `/// <inheritdoc />` alone is enough.

## 8. DI registration

When adding a **new service**, always register the interface ↔ implementation pair in `DIExtension` (Api layer). Never leave a service unregistered.

```csharp
services.AddScoped<IProductService, ProductService>();
```

## 9. Performance

### EF Core / Infrastructure

- **Project instead of Include for reads**: use `.Select(x => new XxxResponse(...))` directly on the `IQueryable` instead of `Include` + manual mapping — avoids loading unneeded columns/navigations.
- **Avoid N+1 queries**: when a navigation must be loaded, use explicit `Include`/`ThenInclude`; never enable lazy-loading proxies.
- **Pagination is mandatory** for every list-returning API (`Skip`/`Take` or keyset pagination for large tables) — never return an entire table.
- **Batch update/delete**: use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` (EF Core 7+) instead of loading entities then mutating/removing them one by one for bulk operations.
- **Index at the Configuration layer**: whenever an entity/column is used for frequent filtering or joins, declare `HasIndex` in its `EntityTypeConfiguration`.
- **Don't run multiple queries in parallel on the same `DbContext`** within one request (EF Core isn't thread-safe) — keep the Scoped lifetime intact.
- Read queries always use `.AsNoTracking()` (see section 7) — repeated here because it's also a performance rule, not just a convention.

```csharp
// Reading a list: direct projection + pagination, no unneeded Include
var items = await _context.Products
    .AsNoTracking()
    .Where(p => p.DeletedAt == null)
    .OrderByDescending(p => p.CreatedAtUtc)
    .Skip((request.Page - 1) * request.PageSize)
    .Take(request.PageSize)
    .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.CreatedAtUtc))
    .ToListAsync(cancellationToken);
```

### Api / Controller

- **No sync-over-async**: never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on a `Task`, anywhere in Api/Application/Infrastructure.
- **`CancellationToken` must flow through** from the Controller all the way to the final EF Core call (never "cut off" partway with `default`) so work stops early on client disconnect and DB resources aren't wasted.
- **Response caching / output caching** for read APIs that change rarely (catalogs, config, static data...).
- **Enable response compression** (Gzip/Brotli) in middleware for the whole Api.
- **Stream large payloads**: use `IAsyncEnumerable<T>` for very large result sets (exports, reports) instead of buffering everything into a `List<T>` first.
- **Keep response DTOs minimal**: map only the fields the frontend needs — never return the raw Entity or extra fields that bloat the payload.

## 10. Checklist for a new API

- [ ] Route is `api/v{n}/{resource}`, correct RESTful verb.
- [ ] Controller method: `Async`, takes `CancellationToken`, returns `Ok(ApiResponse.Success(...))`.
- [ ] Controller has full XML doc (`summary`, `param`, `returns`, `response code` matching each `ProducesResponseType`).
- [ ] `userId` comes from `JwtHelper.GetUserId`, never from an external parameter.
- [ ] Validation in the Controller when it doesn't need the DB.
- [ ] `I{N}Service` interface declared in Application, async + `CancellationToken`, full XML doc (`summary`, `param`, `returns`, `exception` if any).
- [ ] DTO is a `record` with XML doc (`summary` + `param` per field), named per method (`{Method}Request`/`{Method}Response`), grouped into one DTO file per service.
- [ ] Infrastructure implementation injects `IAppDbContext`, uses `AsNoTracking()` for reads, has `/// <inheritdoc />`, adds `<remarks>`/inline comments only if the logic needs clarifying.
- [ ] Create/Edit/Delete return `void` (Task, no data); Get returns a response.
- [ ] Errors use exceptions from `AppException`.
- [ ] New entity: XML doc on the entity + DbSet + IAppDbContext + AppDbContext + Configuration + migration.
- [ ] New service registered in `DIExtension`.
- [ ] Read queries use projection (`Select` straight into a Response) + pagination, no unneeded `Include`, no N+1.
- [ ] No sync-over-async (`.Result`/`.Wait()`); `CancellationToken` flows all the way to EF Core.
