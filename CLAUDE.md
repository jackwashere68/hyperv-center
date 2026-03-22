# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

### Backend (.NET 9)
```bash
dotnet build src/backend/HyperVCenter.sln
dotnet run --project src/backend/src/HyperVCenter.Web        # HTTPS :7001, HTTP :5000
dotnet test src/backend/HyperVCenter.sln                     # xUnit tests
dotnet ef database update --project src/backend/src/HyperVCenter.Infrastructure  # migrations
```

### Frontend (Angular 19)
```bash
cd src/frontend
npm install
npm start                                    # dev server :4200, proxies /api to g3:5000
npx ng build --configuration production      # prod build → dist/hyperv-center/browser/
npm test                                     # Karma + Jasmine
```

### Docker (dev infrastructure)
```bash
docker compose -f docker/docker-compose.yml up -d   # PostgreSQL, pgAdmin, Redis, Seq
```

## Architecture

### Backend — Clean Architecture + CQRS
Four layers, each a separate .csproj:

- **Domain** — Entities (`VirtualMachine`, `HyperVHost`, `Credential`, `Cluster`), value objects, interfaces, enums
- **Application** — CQRS features via MediatR. Each feature under `Features/{Domain}/Commands/` or `Queries/` with paired Handler. FluentValidation pipeline behavior for request validation.
- **Infrastructure** — EF Core (`ApplicationDbContext`), PostgreSQL via Npgsql, DPAPI encryption, PowerShell remoting services, background sync service
- **Web** — API controllers, SignalR hub, middleware, DI composition root (`Program.cs`)

Each layer has a `DependencyInjection.cs` that registers its own services.

### Frontend — Angular 19 standalone components
- **core/** — HTTP services, shared models/DTOs, type definitions
- **features/** — Lazy-loaded routes: dashboard, clusters, credentials, hosts, virtual-machines
- **shared/** — Reusable components, directives, pipes
- State managed with **NgRx Signal Store** (`features/*/store/`)
- Path aliases: `@core/*`, `@shared/*`, `@features/*`, `@env/*` (defined in tsconfig.json)
- Styling: Tailwind CSS 4 + Angular Material 19 + SCSS
- Proxy: `proxy.conf.json` routes `/api/*` → `http://g3:5000` (with WebSocket support)

### VM Remote Console
- **Guacamole** (guacamole-common-js 1.5.0) for RDP/VMConnect display in browser
- Flow: Browser → WebSocket (:5000) → Express → guacd (:4822) → VM (RDP 3389 or VMConnect 2179)
- **PowerShell Direct** terminal as alternative text-only access via node-pty + xterm.js

## Database
- PostgreSQL — default connection in `appsettings.json`: `Host=localhost;Port=5432;Database=hypervcenteer;Username=hyperv;Password=hyperv`
- Migrations in `Infrastructure/Migrations/`
- `BaseEntity` has automatic `UpdatedAt` tracking in `SaveChangesAsync`

## Key Patterns
- **Stub service**: `UseStubService: true` in appsettings enables `StubHyperVManagementService` for local dev without Hyper-V
- **PowerShell remoting**: `IHyperVManagementService` wraps `System.Management.Automation` SDK; results serialized via `ConvertTo-Json` to avoid PSObject nested array loss over remoting
- **VM action cmdlets** (Start-VM, Stop-VM, etc.) don't accept `-Id` — must pipe from `Get-VM -Id '{guid}'`
- **Data Protection**: DPAPI with machine-level keys in shared directory; do NOT call `SetApplicationName()`
- **SignalR**: Real-time VM/host state updates pushed to frontend

## Testing
- **Backend**: xUnit + FluentAssertions + NSubstitute + EF Core InMemory (`HyperVCenter.Application.Tests`)
- **Frontend**: Karma + Jasmine

## Formatting
- `.editorconfig`: 4 spaces default, 2 spaces for JSON/YAML/HTML/TS/SCSS
- `.prettierrc.json`: single quotes, trailing commas (frontend)
