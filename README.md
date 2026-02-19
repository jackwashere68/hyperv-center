# Hyper-V Center

A virtualization management appliance providing vCenter-like management for Microsoft Hyper-V environments.

## Architecture

- **Backend**: .NET 9 (ASP.NET Core) with Clean Architecture, CQRS (MediatR), EF Core + PostgreSQL
- **Frontend**: Angular 19 with Angular Material, NgRx SignalStore
- **Real-time**: SignalR
- **Auth**: JWT + Active Directory integration

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/) with npm
- [Docker](https://www.docker.com/) and Docker Compose
- [Angular CLI](https://angular.dev/) (`npm install -g @angular/cli`)

## Quick Start

### 1. Start Development Infrastructure

```bash
docker compose -f docker/docker-compose.yml up -d
```

This starts PostgreSQL, pgAdmin, Redis, and Seq.

| Service  | URL                    | Credentials          |
|----------|------------------------|----------------------|
| pgAdmin  | http://localhost:5050  | admin@hyperv.local / admin |
| Seq      | http://localhost:5341  | (no auth)            |
| Redis    | localhost:6379         | -                    |
| Postgres | localhost:5432         | hyperv / hyperv      |

### 2. Run the Backend

```bash
cd src/backend
dotnet run --project src/HyperVCenter.Web
```

API: https://localhost:7001 | Swagger: https://localhost:7001/swagger

### 3. Run the Frontend

```bash
cd src/frontend
npm install
ng serve
```

App: http://localhost:4200 (proxies API calls to backend)

## Project Structure

```
hyperv-center/
├── docker/                     # Docker Compose dev environment
├── src/
│   ├── backend/
│   │   ├── src/
│   │   │   ├── HyperVCenter.Domain/          # Entities, value objects, interfaces
│   │   │   ├── HyperVCenter.Application/     # CQRS features, DTOs, validators
│   │   │   ├── HyperVCenter.Infrastructure/  # EF Core, external services
│   │   │   └── HyperVCenter.Web/             # API controllers, SignalR, DI
│   │   └── tests/
│   └── frontend/               # Angular 19 app
├── .editorconfig
├── .gitignore
└── README.md
```

## Running Tests

```bash
dotnet test src/backend/HyperVCenter.sln
```
