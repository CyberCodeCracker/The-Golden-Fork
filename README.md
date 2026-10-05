# The Golden Fork

Restaurant management platform built on **.NET 8**. The heart of the project is a layered
ASP.NET Core Web API — JWT-secured, repository/unit-of-work backed, EF Core persisted —
with a Blazor WebAssembly client consuming it.

```
golden-fork-API/
├── golden-fork.API/             ASP.NET Core Web API — controllers, middleware, auth, Swagger
├── golden-fork.Core/            Business logic — services + service interfaces
├── golden-fork.Domain/          Entities, DTOs, enums, AutoMapper profiles
├── golden-fork.Infrastructure/  EF Core DbContext, repositories, unit of work, DI registration
├── golden_fork.Front/           Blazor WebAssembly client
├── golden_fork.Shared/          DTOs shared between API and client
└── MyProject.Tests/             xUnit test suite (EF Core InMemory, Moq, FluentAssertions, Bogus)
```

---

## Backend

### Architecture

Four layers, dependencies pointing inward:

| Layer | Responsibility |
|---|---|
| **API** | HTTP surface only. Controllers validate `ModelState`, call a service, map the result tuple to an `IActionResult`. No data access. |
| **Core** | All business rules — cart merging, order creation from a cart, status transitions, payment confirmation, password hashing, JWT issuance. Services return `(bool success, string message, T? payload)` tuples so controllers never throw for expected failures. |
| **Domain** | Entities (all deriving `BaseEntity`), request/response DTOs grouped by feature (`Kitchen`, `Cart`, `Order`, `Purchase`), the `OrderStatus` enum, and `ProfileMapper` AutoMapper profiles. |
| **Infrastructure** | `AppDbContext`, one repository per aggregate plus a generic repository, `UnitOfWork`, and an `infrastructureConfiguration()` extension that registers the whole data layer in one call. |

`Program.cs` wires every service as **scoped** (one instance per HTTP request) and delegates
the data layer to `builder.Services.infrastructureConfiguration(builder.Configuration)`.

### Data access: Repository + Unit of Work

`IGenericRepository<T>` provides the full CRUD and query surface used everywhere:

- `GetAllAsync()`, `GetAllAsyncWithFilter(filter, orderBy, noTracking)`, `GetQueryable()`
- `GetByIdAsync(id)` and `GetByIdAsync(params object[] keyValues)` for composite keys
- `GetFirstOrDefaultAsync(predicate, include)` — eager loading passed in as an `IIncludableQueryable` lambda
- `AddAsync` / `AddListAsync`, `UpdateAsync`, `UpdateGeneral(source, dest, fieldsToUpdate)` for partial updates
- `DeleteAsync`, `DeleteByIdAsync`, `DeleteListAsync`, `SaveChangesAsync`

`IUnitOfWork` exposes a typed repository per aggregate (`UserRepository`, `CartRepository`,
`OrderRepository`, `PaymentRepository`, `MenuRepository`, `ItemRepository`, …), a generic
`Repository<T>()` escape hatch, and **`ExecuteInTransactionAsync(action)`** — used for
multi-table writes such as turning a cart into an order and its order items atomically.

### Authentication & authorization

- **JWT bearer** tokens, HMAC-SHA256, 24-hour lifetime, validated on issuer, audience,
  lifetime and signing key with a 1-minute clock skew.
- Claims issued: `NameIdentifier`, `Name`, `Email`, `Role`, a numeric `RoleId`, plus `jti` and `iat`.
- **BCrypt** (`BCrypt.Net-Next`) password hashing on register, verification on login and on
  change-password.
- The API refuses to start if the JWT signing secret is missing — a deliberate fail-fast
  rather than falling back to a weak default.
- Two authorization styles coexist: the framework's `[Authorize(Roles = "Admin")]`, and a custom
  **`[AuthorizeRoles(UserRole.Admin, UserRole.Chef)]`** attribute (an `IAuthorizationFilter`) that
  reads the numeric `RoleId` claim, honours `[AllowAnonymous]`, and distinguishes `401` from `403`.
- Seeded roles: `Admin`, `Customer`, `Kitchen`, `Delivery`.

### Cross-cutting concerns

- **`ExceptionMiddleware`** — registered first in the pipeline, catches any unhandled exception
  and returns a consistent `{ success, message, detail }` JSON body with `500` instead of a
  stack-trace page.
- **Pagination** — `PagedResult<T>` carries `Items`, `PageNumber`, `PageSize`, `TotalItems` and
  computes `TotalPages`, `HasPreviousPage`, `HasNextPage`. `PaginationRequest` adds `SearchTerm`,
  `SortBy`, `Ascending` and `Skip`/`Take`. Every list endpoint is paged, searchable and sortable.
- **AutoMapper** — entity ⇄ DTO mapping registered by assembly scan from `ProfileMapper`.
- **Swagger / OpenAPI** — Swashbuckle with a `Bearer` security definition, so the whole API is
  callable from the Swagger UI after pasting a token.
- **CORS** — a named `AllowBlazor` policy restricted to the known client origins.
- **Static files** — uploaded item and category photos are served from `wwwroot/images`.

### Domain model

`AppDbContext` configures the relationships explicitly rather than relying on convention:

| Relationship | Shape | Delete behaviour |
|---|---|---|
| `User` → `UserRole` | many-to-one | `Restrict` |
| `User` → `Token` | one-to-many | `Cascade` |
| `User` → `Cart` | one-to-one | `Cascade` |
| `Cart` → `CartItem` → `Item` | one-to-many / many-to-one | `Cascade` / `Restrict` |
| `Category` → `Item` | one-to-many | `Restrict` |
| `Item` → `ItemImage` | one-to-many | `Cascade` |
| `Menu` ↔ `Item` | many-to-many via `MenuItem` (composite key) | `Cascade` |
| `Order` → `OrderItem` → `Item` | one-to-many / many-to-one | `Cascade` / `Restrict` |
| `Order` → `Payment` | one-to-one | `Cascade` |

`Restrict` on `Item` references is what keeps historical orders and carts intact when a
menu item is removed.

Also configured at the model level: required fields and max lengths on every string column,
unique indexes on `User.Email`, `UserRole.Name` and `Category.Name`, `decimal(18,2)` for all
money columns, `GETUTCDATE()` defaults for `Order.OrderDate` and `Cart.CreatedAt`,
a `"Pending"` default for `Payment.Status`, and seed data for roles, categories, menus,
sample items and an initial admin user.

### Order lifecycle

`OrderStatus`: `Pending → Confirmed → Preparing → Ready → Delivered`, with `Cancelled` and
`Paid` as terminal/branch states. `OrderService.CanCancelOrder(status)` gates cancellation so
an order already in preparation or delivered cannot be withdrawn, and
`UpdateOrderStatusAdminAsync` is the single entry point for admin-driven transitions.

### Payments

`PaymentService` handles cash payments (`CreateCashPaymentAsync`), staff confirmation
(`MarkAsPaidAsync`), and an anonymous **gateway callback** (`ProcessPaymentCallbackAsync`)
that accepts a `PaymentCallbackDto` and records `TransactionId`, `IsPaid` and `PaidAt` —
the hook for an external provider such as CinetPay or Stripe.

### API endpoints

**`api/user`** — `POST register`, `POST login` (returns the JWT), `POST logout`,
`GET me`, `GET profile`, `PUT profile`, `POST change-password`;
`GET all`, `GET count`, `PUT update-role`, `DELETE {userId}` *(Admin)*.

**`api/category`** — `GET` (paged, anonymous), `GET {id}` with items (anonymous),
`POST`, `PUT {id}`, `DELETE {id}`, `POST {id}/upload-photo`,
`POST {categoryId}/items/{itemId}`, `DELETE {categoryId}/items/{itemId}` *(Admin/Chef)*.

**`api/item`** — `GET` (paged, anonymous), `GET {id}` (anonymous), `POST`, `PUT {id}`,
`DELETE {id}`, `POST {id}/upload-photo?isMain=` *(Admin/Chef)*.

**`api/menu`** — `GET` (paged), `GET {id}` with items, `POST Create`, `PUT Update/{id}`,
`DELETE Delete/{id}` *(Admin/Chef)*; plus `POST api/menu/{menuId}/items` and
`DELETE api/menu/{menuId}/items/{itemId}` *(Admin/Chef)*.

**`api/cart`** *(authenticated; the user is resolved from the token, never from the request body)* —
`GET`, `POST add`, `PUT item/{itemId}`, `DELETE item/{itemId}`, `DELETE clear`.

**`api/order`** — `POST create-from-cart`, `GET {id}`, `POST {id}/cancel` *(authenticated)*;
`GET`, `GET admin`, `GET count-pending`, `PUT update-status`, `PUT {id}/status` *(Admin)*.

**`api/payment`** — `POST cash/{orderId}` *(authenticated)*, `POST {orderId}/confirm`
*(Admin/Kitchen)*, `POST callback` *(anonymous, gateway webhook)*.

---

## Frontend

Blazor WebAssembly (`golden_fork.Front`) talking to the API over a named `HttpClient`:

- `JwtAuthHandler` — a `DelegatingHandler` that attaches the bearer token to every outgoing request.
- `CustomAuthenticationStateProvider` — reads the token from Blazored.LocalStorage and exposes
  the authentication state to `AuthorizeView`.
- Pages: `/login`, `/register`, `/menu`, `/menu/{id}`, `/categories`, `/category/{id}`, `/items`,
  `/orders/checkout`, `/admin/clients`, `/admin/order-history`.
- Shared components: `CardItem`, `CartPanel`, `ClientsTable`; layouts `MainLayout` and `PublicLayout`.

---

## Tests

xUnit suite in `MyProject.Tests`, running against **EF Core InMemory** with Moq, FluentAssertions
and Bogus for fake data:

- `GenericRepositoryTests` — CRUD, filtering and partial updates on the generic repository.
- `MenuRepositoryTests` — repository inheritance behaviour.
- `EntityRelationshipsTests` — navigation properties and cascade/restrict behaviour.
- `EntityValidationTests` — required fields, lengths, value constraints.

```bash
dotnet test golden-fork-API/MyProject.Tests
```

`MyProject.Tests/run_all_tests.bat` runs the suite with TRX + HTML loggers, collects coverage
via coverlet, and generates a ReportGenerator HTML coverage report under `TestResults/`.

---

## Getting started

**Prerequisites:** .NET 8 SDK, SQL Server LocalDB (or any SQL Server instance).

```bash
git clone https://github.com/CyberCodeCracker/The-Golden-Fork
cd The-Golden-Fork/golden-fork-API
```

Set the JWT signing secret — the API will not start without it:

```powershell
$env:GOLDENFORK_JWT_SECRET = "a-long-random-secret-key-here"
```

Point `ConnectionStrings:GoldenForkDatabase` in `golden-fork.API/appsettings.json` at your
SQL Server instance, then apply the schema:

```bash
dotnet ef database update --project golden-fork.Infrastructure --startup-project golden-fork.API
```

Run the API and the client:

```bash
dotnet run --project golden-fork.API      # https://localhost:7256 / http://localhost:5128 — Swagger at /swagger
dotnet run --project golden_fork.Front    # http://localhost:5181
```

The client reads the API base URL from `golden_fork.Front/wwwroot/appsettings.json`.

---

## Tech stack

**Backend** — .NET 8, ASP.NET Core Web API, EF Core 8 (SQL Server), AutoMapper 12,
BCrypt.Net-Next, JWT bearer authentication, Swashbuckle/Swagger.

**Frontend** — Blazor WebAssembly, Blazored.LocalStorage, Bootstrap.

**Testing** — xUnit, Moq, FluentAssertions, Bogus, EF Core InMemory, coverlet, ReportGenerator.
