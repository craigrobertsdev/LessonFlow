# LessonFlow – A digital planner for classroom teachers

## Motivation

Classroom teachers spend a significant amount of time on lesson planning, recording assessment grades, and writing report notes. They are also required to create or source resources to supplement learning.

LessonFlow reduces time spent on repetitive tasks and creates lesson structures that can be easily modified and reused from year to year.

## Features

- **Account Setup** – guided wizard to configure year levels, subjects, timetable structure, and a default weekly template
- **Week Planner** – home screen showing the current week's lessons, a todo list, and navigation to adjacent weeks
- **Lesson Planner** – write lesson plans, link resources, export plans for relief teachers, and adjust subject/duration from the template
- **Term Planner** – high-level topic overview per term/subject to inform lesson planning and surface relevant resources
- **Resource Manager** – cloud file store organised by subject; supports custom folder hierarchies, hiding/archiving, and tagging
- **Student Records** – per-student notes, report comments, and assessment grades
- **Account Settings** – modify timetable, subjects, and other account preferences
- **Login / Register** – email or third-party authentication

## Architecture

LessonFlow is a monorepo with two separate applications:

```
src/
├── server/   ASP.NET Core 10 Web API  (backend — human authored)
└── client/   React 19 SPA             (frontend — agent authored)

tests/
├── Server/
│   ├── LessonFlow.UnitTests/
│   ├── LessonFlow.IntegrationTests/
│   └── TestComponents/
└── Client/
    ├── LessonFlow.Client.UnitTests/
    └── LessonFlow.Client.IntegrationTests/
```

The frontend and backend communicate over HTTPS + JSON. Authentication uses cookies (no JWT).

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10, EF Core 10, PostgreSQL, MediatR |
| Frontend | React 19, TypeScript, React Router v7, TanStack Query v5, shadcn/ui, Tailwind CSS v4 |
| Testing (server) | xUnit, Moq, bunit, Testcontainers (PostgreSQL) |
| Testing (client) | Vitest, React Testing Library, MSW |
| File storage | Azure Blob Storage (Azurite in dev) |

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker (for Azurite and PostgreSQL)

### 1 — Start backing services

```bash
./src/server/start-services.sh   # starts Azurite (Azure Storage emulator)
```

PostgreSQL must be running on `localhost:5432`. Adjust the connection string in `src/server/appsettings.json` if needed.

### 2 — Apply database migrations and seed

```bash
cd src/server
dotnet ef database update
bash SeedDb.sh
```

### 3 — Start the API

```bash
dotnet run --project src/server --launch-profile https
```

### 4 — Start the frontend

```bash
cd src/client
npm install
npm run dev
```

The Vite dev server proxies `/api` requests to the .NET API.

## Testing

Development follows TDD — write the test first, then implement.

```bash
# Server tests
dotnet test tests/Server

# Client unit tests
cd src/client && npm test

# Client integration tests
cd src/client && npm run test:integration
```

Test naming convention (both server and client): `MethodOrComponent_Scenario_ExpectedResult`
