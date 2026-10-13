# CivicConnect - Milestone 3

Virtual Group K: Cameron Purdon 601963, Bianca Grobler 600537, Michael Ellis 600332.

Start with [PED v3.0 Word](docs/PED_v3.0.docx) or its [matching PDF](docs/PED_v3.0_Continued.pdf), continued from the final M2 document with the original layout and styles. Page 1 locates each member's M3 contributions. The current increment supports protected request reads and staff status updates using ASP.NET Core, EF Core and PostgreSQL. The central access policy separates read permission from status-change permission. State objects enforce the agreed lifecycle; the writer commits status, history and public feedback together.

Working login, request creation, assignment of an owner, lists/search and management reporting remain incomplete. The web executable still accepts Development only. A separate test executable runs the status API in simulated Staging with synthetic identities. Current team release assessment: **NOT READY**. See [release evidence](docs/release-evidence.md).

## Build and test

Use a .NET 8 SDK allowed by global.json and a disposable PostgreSQL database. M3 fault tests require a database name ending in `_test`. Supply its connection string through `ConnectionStrings__CivicConnect`; do not commit real credentials. Run from this repository:

```powershell
dotnet build tests/CivicConnect.Verification --configuration Release
dotnet build tests/CivicConnect.DatabaseTests --configuration Release
dotnet build tests/CivicConnect.M3Tests --configuration Release
dotnet run --project tests/CivicConnect.Verification --configuration Release --no-build
dotnet run --project tests/CivicConnect.DatabaseTests --configuration Release --no-build
dotnet run --project tests/CivicConnect.M3Tests --configuration Release --no-build -- --database
```

These are executable harnesses; `dotnet test` does not execute them. The recorded runs contain 29 legacy checks, 25 database checks and 19 new M3 cases. Treat assertion counts separately from meaningful test-case counts. New case specifications and results are in [test evidence](docs/testing-m3.md).

The M3 harness applies migrations, creates synthetic records, starts a loopback HTTPS host, runs the cases and removes its own records. Authentication tickets and a temporary certificate exist only inside the harness. The client trusts that specific certificate. There is no test-login endpoint in the application. The controlled fault test temporarily adds a database constraint and removes it in a finally block; run this suite serially on its own database.

## Development app

Set `ASPNETCORE_ENVIRONMENT=Development` and the configured database connection, then run `dotnet run --project src/CivicConnect.Web`. The landing page is public; protected endpoints return 401 while signed out. Real account sign-in remains pending.

## Navigation

- [Current API contract](docs/api-integration.md)
- [Architecture and integration flow](docs/design.md)
- [Continuing RTM](docs/CivicConnect_RTM_v2.0.xlsx): the established filename is retained; its current evidence is v3.0.
- [Risk register](docs/risk-register.md), [technical debt](docs/technical-debt.md), [defects](docs/defects.md)
- [Collaboration evidence and review steps](docs/github-evidence.md)
- [Submission information](docs/submission-information.md)
- [Team recording and Cameron's defence](docs/presentation-guide.md)
- [AI usage register](docs/ai-usage.md)
