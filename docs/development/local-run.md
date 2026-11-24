# Local Run Guide

This guide is for running the solution locally from Visual Studio with repeatable Docker-hosted infrastructure dependencies.

## Current Local Strategy

Run application code from Visual Studio:

- `LS.Api`
- `LS.UI.Blazor`
- EF migrations and debugging

Run PostgreSQL 17, RabbitMQ, Redis, Mailpit, and Azurite through `ops/local/docker-compose.yml`. PostgreSQL is a core service on host port 5436; Seq remains an optional Compose profile because developers may already have it installed.

See [Local Platform](local-platform.md) and run:

```powershell
./scripts/setup-local-platform.ps1
```

## Development Toggles

The Development environment disables infrastructure that should not block basic local API/UI work:

- `Messaging:Enabled = true`
- `BackgroundJobs:Enabled = false`
- `Observability:Enabled = false`
- `DevelopmentSeed:Enabled = true`

Production defaults keep these capabilities enabled unless explicitly configured otherwise.

## Messaging Transport

Development uses RabbitMQ. Azure production uses Azure Service Bus. Both use the same MassTransit consumers and EF outbox registration. LlanSacco pins Apache-2.0-licensed MassTransit `8.5.10`, which targets .NET 10; the commercial v9 dependency was intentionally removed from the reusable template.

## Required User Secrets

Set local secrets on the API project:

```powershell
dotnet user-secrets set "JwtSettings:SecretKey" "LOCAL-DEVELOPMENT-ONLY-CHANGE-ME-TO-A-LONG-RANDOM-SECRET-AT-LEAST-32-CHARS" --project src\Backend\Api\LS.Api\LS.Api.csproj
```

`./scripts/setup-local-platform.ps1` sets `ConnectionStrings:DefaultConnection` to `Host=localhost;Port=5436;Database=llansacco;Username=...;Password=...` using the generated values in ignored `ops/local/.env`.

## Apply Local Database Schema

Apply every context's migrations, in the order listed in `ops/migrations/contexts.txt`, after setting the connection string. `scripts/test-app-startup.ps1` applies them all in that order; to run them by hand, use one command per context:

```powershell
dotnet ef database update --context ControlPlanePostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context IamPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context SharedPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context HrPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context MembershipPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context BankingPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context LoansPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context AccountingPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context DividendsPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
dotnet ef database update --context CheckOffPostgreSqlDBContext --project src\Backend\Persistence\LS.Persistence\LS.Persistence.csproj --startup-project src\Backend\Api\LS.Api\LS.Api.csproj
```

The bounded contexts share the same local database, but each context has its own migration history table.

## Development Admin

When `LS.Api` starts in Development, it seeds Identity roles and this local admin user:

- Username: `aamodhiambo@gmail.com`
- Password: `Admin@12345`
- Role: `System Administrator`

This seed is disabled outside Development.

## Run From Visual Studio

1. Open the solution.
2. Configure multiple startup projects: `LS.Api` and `LS.UI.Blazor`.
3. Start both projects with the `https` profiles.
4. Browse API documentation at `https://localhost:7129/scalar/v1`.
5. Browse the Blazor UI at `https://localhost:7049`.
6. Sign in through `/iam/sign-in` with the development admin account.
7. Use `/iam/security` to view the current user and configure authenticator-app two-factor authentication.

The Blazor app calls the API through `BackendApi:BaseUrl`. In Development this is configured as:

```json
{
  "BackendApi": {
    "BaseUrl": "https://localhost:7129/"
  }
}
```

The UI must not reference backend Application, Domain, MediatR handlers, EF Core contexts, or repositories directly. Feature UI flows should call API endpoints through typed HTTP client services and shared DTO contracts.

See `docs/architecture/ui-to-backend-flow.md` for the rule of thumb and the tradeoff behind this decision.

## Current Known Local Gaps

- Quartz tables are not created yet, so background jobs stay disabled in Development.
- The MediatR package currently emits a development/test license warning. This is not blocking local development, but production licensing or replacement must be decided later.
