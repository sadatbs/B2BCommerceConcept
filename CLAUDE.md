# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Environment

- **.NET 9 SDK** is required. It is installed at `C:\Program Files\dotnet\` and must be on PATH:
  ```bash
  export PATH="$PATH:/c/Program Files/dotnet:$HOME/.dotnet/tools"
  ```
- **PostgreSQL 16** runs via Docker. Start it with `docker compose up -d` before running migrations or integration tests.
- **dotnet-ef** is installed as a global tool (`dotnet-ef` 9.0.4). All `ef` commands target the Infrastructure project.

## Common Commands

```bash
# Build
dotnet build B2B.Commerce.sln

# Run all tests
dotnet test B2B.Commerce.sln

# Run a single test class
dotnet test tests/B2B.Commerce.Tests/ --filter "FullyQualifiedName~ProductTests"

# Run a single test by name
dotnet test tests/B2B.Commerce.Tests/ --filter "DisplayName~Create_WithValidData"

# Start the database
docker compose up -d

# Add an EF migration
dotnet ef migrations add <MigrationName> --project src/B2B.Commerce.Infrastructure --startup-project src/B2B.Commerce.Infrastructure

# Apply migrations
dotnet ef database update --project src/B2B.Commerce.Infrastructure --startup-project src/B2B.Commerce.Infrastructure
```

## Architecture

This is a **Clean Architecture** solution with a strict dependency rule: Domain ← Infrastructure ← Api. Tests depend only on Domain.

```
B2B.Commerce.Domain         # No external dependencies
  Entities/                 # Product, Catalog, CatalogProduct
  Interfaces/               # IProductRepository (repository contracts)

B2B.Commerce.Contracts      # No external dependencies — DTOs and request/response records
  Products/                 # ProductDto, CreateProductRequest, UpdateProductRequest, UpdateProductPriceRequest
  Catalogs/                 # CatalogDto, CreateCatalogRequest
  Common/                   # PagedRequest, PagedResponse<T>, ErrorResponse (with static factory methods)

B2B.Commerce.Infrastructure # Depends on Domain
  Data/CommerceDbContext          # EF Core DbContext with Fluent API config
  Data/CommerceDbContextFactory   # IDesignTimeDbContextFactory for ef CLI
  Migrations/                     # EF Core migrations
  Repositories/ProductRepository  # IProductRepository implementation

B2B.Commerce.Api            # Depends on Domain + Infrastructure + Contracts
  Endpoints/ProductEndpoints      # 7 Minimal API endpoints, TypedResults, OpenAPI metadata
  Mapping/ProductMappingExtensions # Domain ↔ DTO extension methods
  Program.cs                      # DI registration, Swagger, endpoint mapping

tests/B2B.Commerce.Tests    # Depends on Domain + Contracts + Infrastructure + Api
  Domain/                   # Unit tests for domain entities
  Infrastructure/           # Repository unit tests (EF InMemory, isolated per test)
  Integration/              # API integration tests (TestContainers PostgreSQL, per-test container)
```

## Domain Design Conventions

- **Entities use factory methods** (`Product.Create(...)`) — constructors are private. EF Core accesses them via the private parameterless constructor.
- **Private setters** on all entity properties — mutations go through dedicated methods (`UpdatePrice`, `UpdateDetails`).
- **SKU is normalized** to `UPPER_INVARIANT` on creation. It is the unique business key; `Id` (Guid) is the persistence key.
- **CatalogProduct** is an explicit junction entity (not an EF skip navigation) because it carries metadata (`AddedAt`).
- `DateTime.UtcNow` is used everywhere — no `DateTime.Now`.

## Package Versions (pinned to 9.x — latest jumped to .NET 10)

| Package | Version |
|---|---|
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 |
| Microsoft.EntityFrameworkCore.Design | 9.0.4 |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.4 |
| Microsoft.EntityFrameworkCore | 9.0.4 |
| FluentAssertions | 6.12.2 |

When adding new EF-related packages, pin to `9.0.x` — do **not** use the latest floating version, which resolves to 10.x.

## Database

- Connection string (local dev): `Host=localhost;Port=5432;Database=b2b_commerce;Username=b2b_user;Password=b2b_dev_password`
- The `CommerceDbContextFactory` hardcodes this for `dotnet ef` design-time use.
- The API's `appsettings.json` will need this connection string wired up when endpoints are added (Brief #02).

## Git Workflow

Feature branches follow `feature/NN-short-description`. Each implementation brief is a separate branch merged to `main` and tagged `vN.N.0`.

## Brief Logs

After completing each implementation brief, create a log at:

```
.claude/briefs/brief-NN-YYYY-MM-DD.md
```

Each log documents: scope, what was done, decisions made (with rationale), verification checklist results, and a pointer to the next brief. See `.claude/briefs/brief-01-2026-03-26.md` as the reference example.

| Brief | Date | Tag | Summary |
|---|---|---|---|
| [#01](/.claude/briefs/brief-01-2026-03-26.md) | 2026-03-26 | v0.1.0 | Project setup, domain entities, EF migrations |
| [#02](/.claude/briefs/brief-02-2026-03-26.md) | 2026-03-26 | v0.2.0 | Contracts project, ProductRepository, 7 REST endpoints, TestContainers integration tests |
| [#03](/.claude/briefs/brief-03-2026-03-26.md) | 2026-03-26 | v0.3.0 | Catalog endpoints, FluentValidation filters, ExceptionHandlingMiddleware |
