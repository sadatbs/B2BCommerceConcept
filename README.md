# B2B Commerce Concept

A reference implementation of a B2B e-commerce REST API built with .NET 9, demonstrating Clean Architecture, domain-driven design patterns, and contract pricing. The project evolves through a series of implementation briefs, each adding a distinct capability on top of a stable foundation.

## Features

- **Product catalogue** — CRUD with SKU-based lookup and price management
- **Catalogs** — group products into named collections with pagination
- **Shopping cart** — add/update/remove items with per-customer pricing applied at read time
- **Order checkout** — create orders from carts with a confirm → complete lifecycle
- **Customer management** — company accounts with activate/deactivate and price-tier assignment
- **User accounts** — per-customer users with roles (Buyer, Admin, Approver)
- **Price tiers** — named tiers with per-product price overrides; falls back to list price when no tier price exists

## Architecture

Clean Architecture with a strict inward dependency rule:

```
Domain  ←  Infrastructure  ←  Api
```

| Project | Responsibility |
|---------|---------------|
| `B2B.Commerce.Domain` | Entities, repository interfaces, domain services. Zero external dependencies. |
| `B2B.Commerce.Contracts` | DTOs and request/response records. Zero external dependencies. |
| `B2B.Commerce.Infrastructure` | EF Core DbContext, Npgsql provider, migrations, repository implementations, `PricingService`. |
| `B2B.Commerce.Api` | ASP.NET Core 9 minimal API, endpoint mapping, FluentValidation filters, exception-handling middleware, Swagger/OpenAPI. |
| `B2B.Commerce.Tests` | xUnit unit tests (domain entities) and integration tests (TestContainers PostgreSQL, one container per test class). |

### Domain conventions

- Entities expose **factory methods** (`Product.Create(...)`) — constructors are private; EF Core uses the private parameterless constructor.
- All properties have **private setters**; mutations go through dedicated methods (`UpdatePrice`, `Deactivate`, etc.).
- **SKU** is normalised to `UPPER_INVARIANT`. **Email** is normalised to `lower_invariant`.
- `DateTime.UtcNow` everywhere — no `DateTime.Now`.

## API Reference

All routes are prefixed with `/api`.

<details>
<summary><strong>Products</strong> — <code>/api/products</code></summary>

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/` | List products (paginated) |
| `GET` | `/{id}` | Get product by ID |
| `GET` | `/sku/{sku}` | Get product by SKU |
| `POST` | `/` | Create product |
| `PUT` | `/{id}` | Update product details |
| `PATCH` | `/{id}/price` | Update product price |
| `DELETE` | `/{id}` | Delete product |

</details>

<details>
<summary><strong>Catalogs</strong> — <code>/api/catalogs</code></summary>

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/` | List catalogs (paginated) |
| `GET` | `/{id}` | Get catalog with products |
| `POST` | `/` | Create catalog |
| `PUT` | `/{id}` | Update catalog |
| `DELETE` | `/{id}` | Delete catalog |
| `GET` | `/{id}/products` | List products in catalog (paginated) |
| `POST` | `/{id}/products` | Add products to catalog |
| `DELETE` | `/{id}/products/{productId}` | Remove product from catalog |

</details>

<details>
<summary><strong>Carts</strong> — <code>/api/carts</code></summary>

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/` | Create cart (optionally with `customerId`) |
| `GET` | `/{id}` | Get cart with tier-resolved prices |
| `POST` | `/{id}/items` | Add item to cart |
| `PUT` | `/{id}/items/{productId}` | Update item quantity |
| `DELETE` | `/{id}/items/{productId}` | Remove item |
| `DELETE` | `/{id}` | Clear cart |

</details>

<details>
<summary><strong>Orders</strong> — <code>/api/orders</code></summary>

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/` | Checkout — create order from cart |
| `GET` | `/` | List orders (paginated) |
| `GET` | `/{id}` | Get order by ID |
| `POST` | `/{id}/confirm` | Confirm order |
| `POST` | `/{id}/complete` | Complete order |

</details>

<details>
<summary><strong>Customers</strong> — <code>/api/customers</code></summary>

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/` | List customers (paginated) |
| `GET` | `/{id}` | Get customer with users |
| `GET` | `/code/{code}` | Get customer by code |
| `POST` | `/` | Create customer |
| `PUT` | `/{id}` | Update customer details |
| `PUT` | `/{id}/price-tier` | Assign or clear price tier |
| `POST` | `/{id}/activate` | Activate customer |
| `POST` | `/{id}/deactivate` | Deactivate customer |
| `GET` | `/{id}/users` | List users (paginated) |
| `POST` | `/{id}/users` | Create user |

</details>

<details>
<summary><strong>Price Tiers</strong> — <code>/api/price-tiers</code></summary>

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/` | List all tiers |
| `GET` | `/{id}` | Get tier with prices |
| `POST` | `/` | Create tier |
| `PUT` | `/{id}` | Update tier |
| `DELETE` | `/{id}` | Delete tier |
| `GET` | `/{id}/prices` | List tier prices |
| `PUT` | `/{id}/prices` | Set (upsert) tier price for a product |
| `DELETE` | `/{id}/prices/{productId}` | Remove tier price |

</details>

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (for PostgreSQL)

### 1. Start the database

```bash
docker compose up -d
```

This starts a PostgreSQL 16 container on `localhost:5432` with database `b2b_commerce`.

### 2. Apply migrations

```bash
dotnet ef database update \
  --project src/B2B.Commerce.Infrastructure \
  --startup-project src/B2B.Commerce.Infrastructure
```

### 3. Run the API

```bash
dotnet run --project src/B2B.Commerce.Api
```

Swagger UI is available at `http://localhost:<port>/swagger`.

## Running Tests

Integration tests spin up their own isolated PostgreSQL container via TestContainers — no manual setup required.

```bash
# All tests
dotnet test B2B.Commerce.sln

# Single test class
dotnet test tests/B2B.Commerce.Tests/ --filter "FullyQualifiedName~CustomerEndpointsTests"

# Single test by name
dotnet test tests/B2B.Commerce.Tests/ --filter "DisplayName~Cart_CustomerWithTier_ShowsTierPrice"
```

Current test count: **131** (all passing).

## Tech Stack

| Concern | Library | Version |
|---------|---------|---------|
| Framework | ASP.NET Core (minimal API) | 9.0 |
| ORM | Entity Framework Core + Npgsql | 9.0.4 |
| Validation | FluentValidation | 11.x |
| API docs | Swagger / Swashbuckle | 7.x |
| Testing | xUnit + FluentAssertions | — |
| Test DB | Testcontainers.PostgreSql | 4.x |

## Project History

| Version | Brief | Summary |
|---------|-------|---------|
| v0.1.0 | #01 | Project setup, domain entities, EF migrations |
| v0.2.0 | #02 | Contracts, ProductRepository, 7 REST endpoints, integration tests |
| v0.3.0 | #03 | Catalog endpoints, FluentValidation, ExceptionHandlingMiddleware |
| v0.4.0 | #04 | Cart, Order, domain events, Unit of Work, checkout flow |
| v0.5.0 | #05 | Customer, User, PriceTier entities, PricingService, contract pricing in cart |

## License

MIT
