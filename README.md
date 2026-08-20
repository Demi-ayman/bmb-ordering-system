# BMB Ordering System

A web-based ordering system built for the BMB software-engineering technical task. The solution provides customer registration and JWT authentication, secure order management, automatic monitoring of repeated same-day deletions, administrator reporting, and a responsive HTML/CSS/JavaScript client.

## Features

- Register and authenticate customers.
- Create orders containing one or more items.
- Retrieve the authenticated customer's active orders.
- Retrieve an owned order by ID.
- Soft-delete owned orders.
- Ban a customer from creating orders for six hours after three qualifying deletions on the same UTC date.
- Allow administrators to review all orders, customers, ban status, and customer-specific order history.
- Return consistent RFC-style Problem Details errors.
- Serve the frontend and API from the same ASP.NET Core application.

## Technology

| Area | Technology |
|---|---|
| Backend | ASP.NET Core Web API, .NET 6 |
| Frontend | HTML5, CSS3, vanilla JavaScript |
| Database | SQL Server, Entity Framework Core 6 |
| Security | JWT bearer authentication, password hashing, role-based authorization |
| Hosting target | IIS with HTTPS-only access |
| Testing | xUnit, ASP.NET Core test host, SQL Server LocalDB |

> .NET 6 is used because it is explicitly required by the task. It is out of Microsoft support and should be upgraded before real production use.

## Architecture

The solution follows Clean Architecture:

```text
Domain <- Application <- Infrastructure
                 ^             ^
                 |             |
                 +----- API ---+
```

- `Domain` contains entities and business invariants.
- `Application` contains use cases, validators, results, and abstractions.
- `Infrastructure` implements persistence, authentication, authorization, and time services.
- `Api` hosts controllers, middleware, JWT validation, static frontend files, and dependency composition.

The dependency rules are enforced by automated architecture tests. See [Architecture](docs/architecture.md) for the complete design.

## Repository structure

```text
.
|-- docs/
|-- src/
|   |-- BmbOrdering.Domain/
|   |-- BmbOrdering.Application/
|   |-- BmbOrdering.Infrastructure/
|   `-- BmbOrdering.Api/
|-- tests/
|   |-- BmbOrdering.UnitTests/
|   |-- BmbOrdering.IntegrationTests/
|   `-- BmbOrdering.ArchitectureTests/
|-- BmbOrdering.sln
|-- Directory.Build.props
`-- global.json
```

## Local setup

### Prerequisites

- .NET SDK `6.0.428`.
- SQL Server or SQL Server LocalDB.
- A trusted local ASP.NET Core HTTPS development certificate.

### 1. Restore dependencies and tools

```powershell
dotnet restore BmbOrdering.sln
dotnet tool restore
```

### 2. Configure development secrets

The connection string and JWT signing key are intentionally absent from `appsettings.json`.

```powershell
dotnet user-secrets set "ConnectionStrings:OrderingDatabase" "Server=(localdb)\MSSQLLocalDB;Database=BmbOrderingDb;Trusted_Connection=True;TrustServerCertificate=True" --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj

dotnet user-secrets set "Jwt:SigningKey" "replace-with-a-random-secret-containing-at-least-32-bytes" --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj
```

Detailed configuration guidance is available in [Configuration and secrets](docs/configuration.md).

### 3. Create or update the database

```powershell
dotnet tool run dotnet-ef database update `
  --project .\src\BmbOrdering.Infrastructure\BmbOrdering.Infrastructure.csproj `
  --startup-project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj
```

### 4. Run the application

```powershell
dotnet run --project .\src\BmbOrdering.Api\BmbOrdering.Api.csproj
```

Open:

- Web client: `https://localhost:7174`
- Swagger UI: `https://localhost:7174/swagger`

Swagger is enabled only in the Development environment.

## Administrator access

All registered accounts receive the `Customer` role. At login, an account also receives the `Administrator` role when its email is listed under `Authorization:AdministratorEmails`.

The development configuration currently contains:

```text
demiana.orders@example.com
```

Register that email normally, then log in to receive an administrator token. Administrator assignment must be configured securely for each environment; clients cannot select their own role.

## Business rule: repeated deletion ban

A deletion qualifies when an order is created and deleted on the same UTC calendar date. When a customer reaches three qualifying deletions on that date:

1. The deletion is retained as an audit event.
2. `BannedUntilUtc` is set to six hours after the third deletion.
3. New order creation returns `403 Forbidden` until the time expires.
4. Login, order viewing, and deletion remain available.

Order deletion and ban calculation run inside a serializable SQL transaction. The ban expires automatically; no background job is required.

## Tests

Run the entire suite:

```powershell
dotnet test BmbOrdering.sln
```

Current automated coverage:

| Suite | Tests | Purpose |
|---|---:|---|
| Unit | 46 | Domain rules, validators, handlers, authorization behavior |
| Integration | 10 | Real HTTP pipeline, JWT, SQL persistence, ownership, banning, admin access |
| Architecture | 15 | Dependency direction, encapsulation, conventions, controller security |
| **Total** | **71** | |

Integration tests create uniquely named LocalDB databases and delete them after execution. They do not modify the development database.

## API and deployment

- [API reference](docs/api-reference.md)
- [Architecture](docs/architecture.md)
- [Configuration and secrets](docs/configuration.md)
- [IIS HTTPS deployment](docs/deployment-iis.md)
- [Software requirements specification](docs/software-requirements-specification.md)

## Known limitations

- .NET 6 is end-of-support.
- JWT access tokens have no refresh-token or revocation mechanism.
- Administrator assignment is configuration-based rather than stored in a dedicated identity/role schema.
- List endpoints are not paginated because the task scope assumes a small dataset.
- The application does not include a product catalog, inventory, payment, shipping, email verification, or password recovery.

## License

This repository was created as a technical assessment and has no separate open-source license.
