# LessonFlow – AI Agent Instructions

## Architecture

LessonFlow is a monorepo with two separate apps:

- `src/server` — ASP.NET Core 10 Web API (written by the human developer — **agent must not modify this**)
- `src/client/` — React frontend (the agent's domain)

They communicate over HTTPS + JSON. The backend uses cookie-based authentication (no JWT).

## Agent Scope

**Only modify files inside `src/client/`. and tests/client** Never edit anything in `src/backend`, `tests/server`, migrations, `.csproj`, `.sln`, or any .NET file. If a new API endpoint or DTO is needed, note it as a requirement for the developer — do not implement it yourself.

## Frontend Stack

| Concern       | Library                                                       |
| ------------- | ------------------------------------------------------------- |
| Language      | TypeScript (strict mode)                                      |
| Framework     | React 19                                                      |
| Routing       | React Router v7                                               |
| Server state  | TanStack Query (React Query v5)                               |
| UI components | shadcn/ui                                                     |
| Styling       | Tailwind CSS v4                                               |
| HTTP          | native `fetch` with `credentials: 'include'` on every request |

## First Session — Project Scaffold

If `src/client/` contains no `package.json`, scaffolding is the first task. Complete all of the following before writing any feature code:

1. Vite + React 19 + TypeScript (`strict: true` in `tsconfig.json`)
2. Tailwind CSS v4
3. shadcn/ui — initialise with the **neutral** colour palette and default border radius
4. React Router v7 — root layout, an `<AuthGuard>` component, and placeholder routes for all 8 features
5. TanStack Query v5 — `QueryClientProvider` wrapping the router at the app root
6. `src/client/src/api/client.ts` — base fetch wrapper with `credentials: 'include'` on every request; on 401 redirect to `/login`
7. Vitest + React Testing Library + `@testing-library/user-event` — configured via `vite.config.ts`
8. MSW — `src/client/src/mocks/` with a `server.ts` for integration tests and `browser.ts` for dev-mode mocking
9. Vite proxy: forward all `/api` requests to the .NET API (`target` read from `.env.development`)
10. Create `tests/Client/LessonFlow.Client.UnitTests/` and `tests/Client/LessonFlow.Client.IntegrationTests/` directory structures

## API Conventions

- Base URL: `https://localhost:{port}/api/v1/`
- All endpoints follow `/api/v{version}/{feature}/`
- **Every request must include `credentials: 'include'`** (cookie auth — no Authorization header)
- Do **not** add CSRF/antiforgery token handling — Minimal API endpoints do not require antiforgery tokens by default; the middleware is present for Blazor forms only
- Responses use standard HTTP status codes; errors return RFC 7807 Problem Details
- On 401, redirect to `/login`

### Working against unimplemented endpoints

If a required backend endpoint does not exist yet, **do not block** — write the UI and tests against MSW handlers, then add a comment in the relevant `api/` file:

```typescript
// BACKEND DEPENDENCY: POST /api/v1/auth/login not yet implemented
// Tracked: implement Login endpoint in src/server/Api/Features/Auth/
```

The developer will implement the endpoint separately. Do not modify any .NET files.

## Authentication Flow

```
POST /api/v1/auth/login        { email, password }  → sets auth cookie
POST /api/v1/auth/register     { email, password, confirmedPassword }
GET  /api/v1/auth/me           → { id, hasCompletedAccountSetup }
```

After login, check `hasCompletedAccountSetup`:

- `false` → redirect to `/account-setup`
- `true` → redirect to `/week-planner`

## Features & UX References

For each feature, read the referenced Blazor component(s) **before** building the React equivalent — they define the intended behaviour and data flow.

| Route                 | Feature                       | Blazor UX Reference                                                                              |
| --------------------- | ----------------------------- | ------------------------------------------------------------------------------------------------ |
| `/login` `/register`  | Login / Register              | `src/Components/Account/Pages/Login.razor`, `Register.razor`                                     |
| `/account-setup`      | Account Setup (4-step wizard) | `src/Components/Pages/AccountSetup.razor`, `src/Components/Pages/AccountSetup/`                  |
| `/week-planner`       | Week Planner (home screen)    | `src/Components/Pages/WeekPlannerPage.razor`, `WeekPlannerColumn.razor`, `WeekPlannerCell.razor` |
| `/lesson-planner/:id` | Lesson Planner                | `src/Components/Pages/LessonPlanner.razor`                                                       |
| `/term-planner`       | Term Planner                  | `src/Components/Pages/TermPlannerPage.razor`                                                     |
| `/resources`          | Resource Manager              | _(no Blazor reference — new feature)_                                                            |
| `/students`           | Student Records               | _(no Blazor reference — new feature)_                                                            |
| `/settings`           | Account Settings              | _(no Blazor reference — new feature)_                                                            |

### Account Setup Wizard Steps

1. **Basic Info** — school name, calendar year, year levels taught, working days
2. **Subjects** — multi-select from available curriculum subjects
3. **Timing** — school day start/end time, define lesson and break periods
4. **Schedule** — set the weekly template (which subject goes in which period per day)

Submits all data in one request: `POST /api/v1/users/account-setup`

## Key Domain Concepts

The frontend must understand these to render UI correctly:

**Planning hierarchy:** `YearPlan → WeekPlanner → DayPlan → LessonPlan`

**WeekPlannerTemplate:** the teacher's reusable weekly schedule, set during account setup. Each day has ordered periods; each period is a `Lesson`, `Break`, or `Nit` (non-instructional time).

**PeriodType enum:** `Lesson | Break | Nit`

**DayType enum:** `Working | NonWorking | StudentFree`

**YearLevel enum** (South Australian): `Reception | Year1 | Year2 | ... | Year10 | Years1To2 | Years3To4 | Years5To6 | Years7To8 | Years9To10`

**Week navigation:** weeks are identified by `{ year, termNumber, weekNumber }` — the API uses these to create/retrieve week planners, not date strings.

## Key API Shapes

```typescript
// Auth
type LoginRequest = { email: string; password: string };
type AuthResponse = { hasCompletedAccountSetup: boolean };

// Account setup (single submission)
type AccountSetupRequest = {
  firstName: string;
  schoolName: string;
  calendarYear: number;
  subjectsTaught: string[];
  yearLevelsTaught: string[];
  workingDays: string[];
  weekPlannerTemplate: WeekPlannerTemplateDto;
};

// Week Planner
type WeekPlannerDto = { dayPlans: DayPlanDto[]; weekPlannerTemplate: WeekPlannerTemplateDto; weekStart: string; weekNumber: number };
type DayPlanDto = { date: string; lessonPlans: LessonPlanDto[]; schoolEvents: SchoolEventDto[]; breakDutyOverrides?: Record<number, string> };
type LessonPlanDto = {
  lessonPlanId: string;
  subject: CurriculumSubjectDto;
  planningNotesHtml: string;
  resources: ResourceDto[];
  startPeriod: number;
  numberOfPeriods: number;
};

// Template
type WeekPlannerTemplateDto = { periods: TemplatePeriodDto[]; dayTemplates: DayTemplateDto[] };
type TemplatePeriodDto = { name?: string; startPeriod: number; startTime: string; endTime: string; isBreak: boolean };
type DayTemplateDto = { dayOfWeek: string; type: string; templates: LessonTemplateDto[] };

// Resources
type ResourceDto = { id: string; name: string; url: string; resourceType: string; yearLevels: string[] };
```

## Testing (TDD)

Development follows **test-driven development** — write the test first, then implement. This mirrors the server-side testing approach.

**Test projects** (follow the same naming convention as the server):

| Project                              | Location                                           | Purpose                                   |
| ------------------------------------ | -------------------------------------------------- | ----------------------------------------- |
| `LessonFlow.Client.UnitTests`        | `tests/Client/LessonFlow.Client.UnitTests/`        | Component logic, hooks, utility functions |
| `LessonFlow.Client.IntegrationTests` | `tests/Client/LessonFlow.Client.IntegrationTests/` | Full page/feature tests with mocked API   |

**Libraries:**
| Library | Purpose |
|---|---|
| Vitest | Test runner (pairs with Vite) |
| React Testing Library | Component rendering and interaction |
| MSW (Mock Service Worker) | API mocking for integration tests |
| `@testing-library/user-event` | Simulating user interactions |

**Naming conventions** mirror the server exactly:

- Test files: `{ComponentName}.test.tsx` / `{HookName}.test.ts`
- Test names: `MethodOrComponent_Scenario_ExpectedResult`
  - e.g. `WeekPlannerGrid_WhenNoLessonPlans_RendersEmptyCells`
  - e.g. `useWeekPlanner_WhenFetchFails_ReturnsError`

**Unit tests** — component logic and hooks in isolation, no real API calls, no routing:

```typescript
// tests/Client/LessonFlow.Client.UnitTests/features/weekPlanner/WeekPlannerGrid.test.tsx
describe('WeekPlannerGrid', () => {
  it('WeekPlannerGrid_WhenNoLessonPlans_RendersEmptyCells', () => {
    render(<WeekPlannerGrid dayPlans={[]} template={mockTemplate} />)
    expect(screen.getAllByTestId('empty-cell')).toHaveLength(5)
  })
})
```

**Integration tests** — full feature with MSW handlers intercepting real API calls:

```typescript
// tests/Client/LessonFlow.Client.IntegrationTests/features/weekPlanner/WeekPlannerPage.test.tsx
describe('WeekPlannerPage', () => {
  it('WeekPlannerPage_WhenLoaded_DisplaysCurrentWeekLessons', async () => {
    server.use(http.get('/api/v1/weekplanners', () => HttpResponse.json(mockWeekPlanner)))
    render(<WeekPlannerPage />, { wrapper: AppProviders })
    await screen.findByText('Mathematics')
  })
})
```

MSW handlers live in `tests/Client/LessonFlow.Client.IntegrationTests/mocks/handlers/`.

## Client Project Structure

```
src/client/
├── src/
│   ├── api/          # TanStack Query hooks + fetch wrappers, one file per feature
│   ├── components/   # shadcn/ui extensions and shared components
│   ├── features/     # one folder per route feature (co-locate components + hooks)
│   ├── routes/       # React Router route definitions
│   ├── types/        # TypeScript types mirroring API contracts
│   └── main.tsx
├── public/
├── package.json
└── vite.config.ts

tests/Client/
├── LessonFlow.Client.UnitTests/
│   └── features/     # mirrors src/client/src/features structure
└── LessonFlow.Client.IntegrationTests/
    ├── features/
    └── mocks/
        └── handlers/  # MSW request handlers per feature
```

## Dev Workflow

```bash
# Start .NET API (from repo root)
dotnet run --project src/server --launch-profile https

# Start React dev server
cd src/client && npm install && npm run dev

# Run frontend tests
cd src/client && npm test                      # unit tests (watch)
cd src/client && npm run test:integration      # integration tests
dotnet test tests/Server                       # server tests
```

The Vite dev server proxies `/api` to the .NET API — never hardcode the backend port in source files.

---

## Backend Context (read-only reference)

The .NET API uses:

- **Vertical Slice Architecture**: each feature is a `static class` with a nested `Request`, `Response` record and `Endpoint` method in `src/Api/Features/{Feature}/`
- \*\*domain events published inside `SaveChangesAsync`
- **EF Core 10 + PostgreSQL**; repository interfaces in `src/Shared/Interfaces/Persistence/`
- **Strongly typed IDs**: all entity IDs are `record` wrappers over `Guid` (e.g. `UserId`, `WeekPlannerId`) — they serialise as plain GUIDs in JSON
- **Cookie-based ASP.NET Identity** — no JWT issued to clients
- New routes registered in `src/Api/RouteMapper.cs`
