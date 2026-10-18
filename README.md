# CivicConnect

Service request management · Virtual Group K · SEN381

Michael Ellis 600332 · Cameron Purdon 601963 · Bianca Grobler 600537

## Final assessed product — 18 October 2026

The application integrates real sign-in and server-owned roles, request submission, requester status/history/public feedback, scoped staff lists/search, assignment and claiming, notes, priority/due dates, resolution/closure and management counts/references. EF Core/PostgreSQL persists the required effects transactionally. Categories, ownership and lifecycle legality are enforced server-side.

The declared environment is **controlled local staging**. Public production is a **NO-GO** for the recorded deployment/security and operating gaps. M3 remains **CONDITIONALLY READY for its agreed protected read/status increment only**. The unchanged 54 original acceptance criteria score **48.5/54 = 89.8%**; the score does not establish production readiness.

[Final PED PDF](docs/PED_v4.0_Final.pdf) · [Word source](docs/PED_v4.0.docx) · [Continuing RTM](docs/CivicConnect_RTM_v2.0.xlsx) · [Assessed version/access](docs/submission-information.md) · [Final evidence](docs/evidence/m4-final-20261018/summary.md)

## Prerequisites

.NET 8 SDK (global.json permits later .NET 8 feature bands), ASP.NET Core 8 and PostgreSQL. Recorded local verification used SDK 8.0.425, runtime 8.0.31 and PostgreSQL 18.4; CI uses PostgreSQL 17. EF Core/Npgsql packages are 8.0.11. Review the runtime support window before extending operation.

## Controlled local setup

Supply `ConnectionStrings__CivicConnect` through the environment or User Secrets; keep actual credentials outside Git. For example, configure an existing disposable database with a name ending `_test`, then run the database harness below to apply migrations and verify storage. It creates and removes synthetic fixtures. Account setup requires the migrated schema and prompts for the username, display name, role, staff category grants and a password of at least 12 characters.

```powershell
dotnet run --project tests/CivicConnect.DatabaseTests --configuration Release
dotnet run --project tools/CivicConnect.AccountSetup --configuration Release
$env:ASPNETCORE_ENVIRONMENT = 'Staging'
dotnet run --project src/CivicConnect.Web --configuration Release --no-launch-profile --urls https://localhost:5081
```

Configure the ASP.NET Core development HTTPS certificate on the demonstration computer (`dotnet dev-certs https --trust` on Windows) before opening the local HTTPS URL. Open `/account/sign-in`; authorised users receive their role-specific landing page. Normal cookies require HTTPS. This local development certificate does not establish public deployed TLS acceptance. `Production` startup is deliberately blocked pending release approval.

The isolated participant preview at `http://127.0.0.1:5089` uses temporary accounts and an explicit fixture-only cookie override. It is a usability environment, not the normal deployed configuration. Its access sheet remains outside the repository.

## Verification

Set `ConnectionStrings__CivicConnect` to an explicit disposable database ending `_test`. Run the suites serially because fault cases temporarily add database constraints and clean them afterwards.

```powershell
dotnet run --project tests/CivicConnect.Verification --configuration Release
dotnet run --project tests/CivicConnect.DatabaseTests --configuration Release
dotnet run --project tests/CivicConnect.M3Tests --configuration Release -- --database
dotnet run --project tests/CivicConnect.M4Tests --configuration Release
dotnet run --project tests/CivicConnect.M4Tests --configuration Release -- --database
```

These executable harnesses contain 73/43/19/30/36 passing checks: 201 total. The last suite uses real MVC forms, password hashes, PostgreSQL adapters and a pinned generated HTTPS certificate. It verifies the complete submit → claim → progress → resolve → close journey, notes/privacy, scheduling, rollback, competing claims, durable duplicate protection, sorting, restart and session revocation. Exit codes report failures; `dotnet test` does not discover these custom harnesses.

[Remote CI run 39145430277](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/39145430277) passed on code commit `625e4e99882150f79ad3e00bae327b14e63e5d9c`, including static analysis and secrets/dependency gates. The separate paced database-read exercise is not the full NFR-002 user-visible performance workload. Final documentation revisions have their own repository history.

## Architecture

Razor controllers coordinate `IPortalAccounts`, `IPortalRequestStore`, `RequestQueryService` and `RequestStatusService`. EF adapters own storage. Central access policy restricts request ownership and staff categories. Bianca’s six-state lifecycle is reused by supported writes; first assignment must use the atomic ownership command, which validates one eligible stored owner. Status, history and public status feedback commit together. Notes retain visibility/author/time; scheduling has separate audit. Account role/category/active state and security stamp are rechecked for browser sessions.

The lifecycle is New → Assigned → In Progress → Resolved → Closed, or New → Rejected. Closed and Rejected are terminal. The legacy status-only JSON endpoint rejects ownerless New → Assigned. See [API boundaries](docs/api-integration.md), [design history](docs/design.md) and [decision register](docs/decisions/README.md).

## Final limitations and evidence

Five real participants were observed by Cameron. All reported submission and finding status, but only three completed without coaching; P2/P3 also chose categories other than IT Support. The four-of-five threshold failed. [Usability observations](docs/evidence/m4-final-20261018/usability.md) retain timings and explanations. A local timed restore recovered matching request, owner, history, notes and scheduling audit; automated/off-site/hosted recovery is not established.

Remaining acceptance gaps include the full performance workload, complete Chrome/Edge and keyboard-error matrix, trusted deployed HTTPS/protected operations, and a per-note status snapshot for general public notes. No hosting account/environment or complete effort/spend ledger was available. Final team review and Moodle submission are pending. Test reports identify how each check was performed.

The existing PED, RTM, risk, defect, debt, decision and AI registers retain their history. AI assistance is explicitly disclosed in [the continuing register](docs/ai-usage.md). Personal handover instructions and the raw contribution archive are outside the assessed repository. Health endpoints `/health` and `/health/database` report application/database availability; logs omit custom exception detail, and production sensitive-data logging must remain disabled.
