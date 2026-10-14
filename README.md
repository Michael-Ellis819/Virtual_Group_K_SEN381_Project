# CivicConnect

Community service request management for Virtual Group K.

Michael Ellis 600332 · Cameron Purdon 601963 · Bianca Grobler 600537

## Current implementation

The M3 increment provides protected request-details reads, central authorisation, filtered response data and authorised status updates. Requesters can read their own records and public notes. Staff can read and update records in authorised categories. PostgreSQL persistence, EF Core migrations and the six-state lifecycle are implemented. Status changes, history and public feedback are saved together in one transaction, with stale-record checks.

The application also includes a small Razor landing page. Real sign-in, request creation, owner assignment, lists/search and management reporting remain incomplete. The normal application starts only in Development; a separate harness exercises the integrated API in simulated Staging using HTTPS and synthetic identities. Full product acceptance is not claimed.

## Prerequisites and versions

- .NET 8 SDK, feature band 8.0.300 or later within the .NET 8 line, as permitted by `global.json`.
- Matching .NET 8 and ASP.NET Core 8 runtimes; C# 12 through the SDK.
- ASP.NET Core MVC, Razor, cookie authentication and dependency injection.
- EF Core and Npgsql provider packages 8.0.11, restored during the build.
- A running PostgreSQL instance and a disposable test database. The M3 suite requires its database name to end in `_test`.

Recorded Windows verification used SDK 8.0.425, runtime 8.0.31 and PostgreSQL 18.4. Michael's performance run and GitHub CI used PostgreSQL 17. Both affected test projects pin `System.Text.Json` to patched version 8.0.5.

## Build and verify

Run these commands from the repository root after configuring the test connection described below:

```powershell
dotnet --info
dotnet build tests/CivicConnect.Verification --configuration Release
dotnet build tests/CivicConnect.DatabaseTests --configuration Release
dotnet build tests/CivicConnect.M3Tests --configuration Release -p:AnalysisLevel=latest -p:EnforceCodeStyleInBuild=true
dotnet build tests/CivicConnect.PerformanceTests --configuration Release

dotnet run --project tests/CivicConnect.Verification --configuration Release --no-build
dotnet run --project tests/CivicConnect.DatabaseTests --configuration Release --no-build
dotnet run --project tests/CivicConnect.M3Tests --configuration Release --no-build -- --database
dotnet run --project tests/CivicConnect.PerformanceTests --configuration Release --no-build
```

Run the database suite before the performance exercise so the schema is migrated. Recorded correctness results are **29 regression checks, 25 database checks and 19 M3 cases**, with zero failures. The M3 cases comprise 6 component, 5 black-box, 6 API/integration and 2 system cases.

These are custom executable harnesses; use the commands above rather than `dotnet test`. A failed check produces a non-zero exit code. Run the database suites serially on their own disposable database: the rollback case temporarily adds a constraint and removes it in a `finally` block.

The M3 harness applies migrations, creates synthetic records, starts a temporary HTTPS server, runs the cases and removes its records. Test tickets and the certificate exist only inside the harness. The application has no test-login endpoint or client-selectable role.

## Run the development application

With the connection configured and migrations applied:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src/CivicConnect.Web --configuration Release --no-launch-profile --urls http://127.0.0.1:5080
```

Open `http://127.0.0.1:5080/` for the landing page. Protected endpoints return `401 Unauthorized` while signed out because real sign-in is not implemented. Use the harnesses to exercise authenticated behaviour. Press **Ctrl+C** to stop the application. The local HTTP landing page does not demonstrate production HTTPS acceptance.

## Established interface

| Endpoint | Behaviour |
| --- | --- |
| `GET /requests/{id}` | Returns permitted request details and notes as JSON. |
| `GET /requests/antiforgery` | Supplies an authenticated caller with the token/cookie needed for a status change. |
| `POST /requests/{id}/status` | Accepts `expectedStatus`, `targetStatus` and nonblank `feedback`; requires authorised Staff access and antiforgery. |

The request ID must be a non-empty UUID. Reads return `200` on success; status updates return `204`. Invalid input/transitions return `400`, signed-out requests `401`, missing/inaccessible records `404`, stale status/version conflicts `409`, and unexpected failures a generic `500`. Responses use `Cache-Control: no-store`.

The lifecycle is **New → Assigned → In Progress → Resolved → Closed**, or **New → Rejected**. Closed and Rejected are terminal. Moving to Assigned does not assign an actual owner. Feedback is limited to 2,000 .NET string code units. Identity and audit actor come from the protected server principal. See the [API and integration contract](docs/api-integration.md).

## Structure and responsibilities

| Path | Architecture responsibility |
| --- | --- |
| `src/CivicConnect.Web/Views/` | Razor presentation and landing page |
| `src/CivicConnect.Web/Controllers/RequestsController.cs` | HTTP authentication, validation, antiforgery and response mapping |
| `src/CivicConnect.Web/Application/RequestQueryService.cs` | Protected read coordination and filtered response projection |
| `src/CivicConnect.Web/Application/RequestStatusService.cs` | Authorised status-change coordination |
| `src/CivicConnect.Web/Application/StatusTransitionService.cs` | State classes and legal transitions |
| `src/CivicConnect.Web/Security/RequestAccessPolicy.cs` | Central read and status-change policy |
| `src/CivicConnect.Web/Data/` and `src/CivicConnect.Web/Migrations/` | EF Core adapters, entities, transactions and migrations |
| `src/CivicConnect.Web/Composition.cs` | Dependency registration, database configuration and safe error handling |
| `tests/` | Regression, database, M3 functional/system and performance harnesses |
| `.github/workflows/verify.yml` | Build, automated checks, code analysis and dependency gates |
| `docs/` | PED, RTM, decisions, registers and execution evidence |

## Component and design responsibilities

`RequestsController` handles the HTTP boundary. `RequestQueryService` reads through `IRequestReader`, applies `RequestAccessPolicy` and produces a filtered DTO. `RequestStatusService` checks action permission and delegates transition legality to `StatusTransitionService`. `EfRequestStatusWriter` commits the status, history and public feedback atomically, rejecting a changed record version.

ADR-M2-01 records the layered application, ADR-M2-02 the State lifecycle, ADR-M2-05 central authorisation and ADR-M2-06 explicit service coordination. Bianca's [completed conformance review](docs/reviews/Bianca_M3_Final_Review_Updated.docx) records their implementation consequences.

## Configuration and database continuation

The application and database harnesses consume `ConnectionStrings__CivicConnect`. Supply it through an environment variable or User Secrets. For PowerShell, replace the role, password and connection details below with those for your disposable test database:

```powershell
$env:ConnectionStrings__CivicConnect = 'Host=127.0.0.1;Port=5432;Database=civicconnect_test;Username=YOUR_TEST_ROLE;Password=YOUR_TEST_PASSWORD'
```

The database and M3 harnesses apply migrations. The performance harness requires an already migrated schema, creates one temporary request and deletes it afterwards. Keep Michael's saved report intact; store new recordings separately.

Real credentials, cookie keys and certificate private keys must stay outside committed files. Development may create local data-protection keys; test hosts use ephemeral keys. Persistent deployed cookie keys, trusted accounts and permission refresh remain deployment concerns.

## Testing and known limitations

Tests cover ownership/category access, note filtering, legal/prohibited transitions, feedback boundaries, antiforgery, stale permissions/version conflicts, safe errors, rollback and two complete API/database status journeys. Black-box evidence uses Equivalence Partitioning and Boundary Value Analysis. See [test specifications and results](docs/testing-m3.md).

Michael's separate performance exercise recorded **200 successful reads, zero failures, 1.62 ms mean, 8.69 ms p95, 0.12 seconds total duration and 1,696.16 operations/second**. This short database-read benchmark has no separate database target and does not establish full NFR-002 acceptance.

Remaining limitations include real account/permission management, request submission and assignment, lists/search/reporting, browser usability, full pilot-load and timed recovery acceptance, and normal Staging/production startup. These remain visible in the [RTM](docs/CivicConnect_RTM_v2.0.xlsx), [defect register](docs/defects.md) and [technical debt register](docs/technical-debt.md).

## Engineering evidence

Cameron personally completed the recorded correctness verification on **13 October 2026**. Michael supplied the separate performance run on **14 October**. Bianca and Michael reviewed the combined work on **14 October**. Original human reports retain their execution dates and operators.

Michael's PR16 is merged at `cb588d4`; [main CI run 38579869423](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38579869423) passed its regression, database, performance and dependency-report steps. Cameron's 19-case suite and strict combined gates are configured on this feature branch; their remote execution is assessed separately. The recorded release classification is **NOT READY**, with reasons in the release evidence.

Supporting evidence:

- [PED v3.0 PDF](docs/PED_v3.0_Updated.pdf), [editable Word document](docs/PED_v3.0.docx) and [Markdown copy](docs/PED_v3.0.md)
- [Continuing RTM workbook](docs/CivicConnect_RTM_v2.0.xlsx) and [Markdown copy](docs/CivicConnect_RTM_v2.0.md) — 27 requirements and 54 acceptance criteria
- [Decision log and ADRs](docs/decisions/README.md)
- [Design views](docs/design.md) and [traceability demonstration](docs/traceability-demo.md)
- [GitHub evidence](docs/github-evidence.md)
- [Cameron's personal verification](docs/evidence/m3/personal-20261013-181846/summary.md)
- [Michael's performance report](docs/evidence/m3-performance-results.txt)
- [Risk register](docs/risk-register.md) and [release evidence](docs/release-evidence.md)
- [AI usage record](docs/ai-usage.md)
- [Submission information](docs/submission-information.md) and [presentation guide](docs/presentation-guide.md)
