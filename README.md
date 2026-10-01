# CivicConnect

Community service request management for Virtual Group K.

Michael Ellis 600332 · Cameron Purdon 601963 · Bianca Grobler 600537

## Current implementation

The current M2 implementation provides a protected request-details read path, a central authorisation policy and filtered response data. Requesters can read their own records and public notes. Staff can read records in authorised categories, including internal notes. The application also includes a small Razor landing page. The verification executable contains 29 passing service and HTTP checks.

The current data is synthetic and held in a development fixture. The application does not yet include working sign-in, request creation, status transitions, database integration, migrations, reporting or production deployment. It refuses to start outside Development because production identity and persistence have not been implemented. A technology selected in the PED is not treated as implemented unless corresponding application evidence exists.

## Prerequisites and versions

- .NET 8 SDK, feature band 8.0.300 or later in the .NET 8 line.
- Matching, currently serviced .NET 8 and ASP.NET Core 8 runtimes.
- C# 12 through the .NET 8 SDK.
- ASP.NET Core MVC, Razor, cookie authentication and dependency injection.

The repository's `global.json` requests SDK 8.0.300 and permits later stable .NET 8 feature bands. Local verification used SDK 8.0.300 with .NET and ASP.NET Core runtime 8.0.31. The current application has no external NuGet package dependencies.

EF Core and PostgreSQL are selected in the M2 technology and persistence decisions but have not yet been added. Their package and server versions remain to be established. A conventional test framework is also planned. Review the target before M3 to ensure that the selected version remains supported. See the [Microsoft .NET release and support policy](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support).

## Build and verify

Run these commands from the repository root:

```powershell
dotnet --info
dotnet build tests/CivicConnect.Verification/CivicConnect.Verification.csproj
dotnet run --project tests/CivicConnect.Verification --no-build
```

The final command starts a temporary server on a random loopback port, uses protected test authentication tickets, checks the actual controller and application service, and then stops the server. It exits with a non-zero result if a check fails. A successful run ends with:

```text
RESULT 29 checks passed; 0 failed.
```

Use this command instead of `dotnet test`: the current verification executable is a custom harness and has no external test-framework package. The recorded result is in `docs/evidence/local-verification.txt`.

Test authentication tickets exist only inside the verification executable. The web application has no test-login endpoint, role selector or request header that grants access. Loopback HTTP verification does not demonstrate production HTTPS compliance.

## Run the development application

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project src/CivicConnect.Web --urls http://127.0.0.1:5080
```

Open `http://127.0.0.1:5080/` for the landing page. A signed-out request to `http://127.0.0.1:5080/requests/11111111-1111-1111-1111-111111111111` returns `401 Unauthorized`. This is expected because real sign-in has not been implemented. Use the verification executable to exercise authenticated reads. Press `Ctrl+C` to stop the application. The development application uses synthetic data only.

## Established interface

The current application establishes one protected read endpoint:

```text
GET /requests/{id}
```

The request ID must be a valid UUID. There is no request body. A successful request returns permitted request details as JSON. Requesters receive only their own records and public notes. Authorised staff may receive public and internal notes for permitted categories.

The endpoint returns:

- `200 OK` for an authorised request;
- `400 Bad Request` for a malformed or empty UUID;
- `401 Unauthorized` when the caller is not authenticated;
- `404 Not Found` when the record is missing or inaccessible; and
- `500 Internal Server Error` for an unexpected application or reader failure.

Missing and inaccessible records both return `404 Not Found`. Responses use `Cache-Control: no-store`. No POST, PUT, PATCH or DELETE endpoint is implemented. The full contract is in `docs/api-integration.md`.

## Structure and responsibilities

| Path | Architecture responsibility |
| --- | --- |
| `src/CivicConnect.Web/Views` | Razor presentation; currently contains the landing page |
| `src/CivicConnect.Web/Controllers/RequestsController.cs` | HTTP authentication gate, UUID validation and response mapping |
| `src/CivicConnect.Web/Application/RequestQueryService.cs` | Protected read use case, reader contract and safe response projection |
| `src/CivicConnect.Web/Security/RequestAccessPolicy.cs` | Central resource policy injected into the application service |
| `src/CivicConnect.Web/Domain/RequestSnapshot.cs` | Request read model and public/private note distinction |
| `src/CivicConnect.Web/Data/DevelopmentRequestReader.cs` | Synthetic reader; a future persistence adapter will implement `IRequestReader` |
| `src/CivicConnect.Web/Composition.cs` | Dependency registration, cookie authentication and safe error handling |
| `tests/CivicConnect.Verification` | Direct-service and loopback HTTP checks |
| `docs/PED_v2.0_FINAL.docx` | Reviewed PED preserving M1 and consolidating the M2 decisions |
| `docs/CivicConnect_RTM_v2.0.xlsx` | Requirements, acceptance criteria, traceability and current evidence |
| `docs/decisions` | Shared decision index and M2 ADRs |
| `docs/api-integration.md` | Established read contract and planned write integration |
| `docs/traceability-demo.md` | Selected requirement-to-implementation trace |
| `docs/evidence/local-verification.txt` | Recorded local verification result |
| `docs/ai-usage.md` | AI assistance and human verification record |

## Component and design responsibilities

`RequestsController` handles the HTTP boundary, validates the identifier and maps the application result to a safe response. `RequestQueryService` coordinates the read operation, obtains a record through `IRequestReader`, applies the access policy and produces a filtered DTO. `RequestAccessPolicy` checks requester ownership, staff category permission, account status and supported roles. `DevelopmentRequestReader` supplies synthetic development data and will later be replaced by a database-backed adapter.

ADR-M2-05 records the central authorisation policy. ADR-M2-06 records explicit application-service coordination and the integration boundary. ADR-M2-02 defines legal request-status transitions, which remain planned.

## Configuration and database continuation

The current read slice needs no application secret or database connection string. ASP.NET Core may create local data-protection keys during development; these files must not be committed. The verification harness uses temporary keys. A deployed application will require protected persistent cookie keys.

A future EF Core/PostgreSQL adapter will implement `IRequestReader`, together with database migrations, persisted users, categories and permissions, verified connectivity, and transaction and concurrency handling.

The planned database configuration name is:

```text
ConnectionStrings__CivicConnect
```

Supply it through a local environment variable or the deployment platform's secret store. The current application does not consume it, and no migration command is documented as working. Real credentials must never be placed in source code, `appsettings*.json`, the PED or committed documentation.

Future write operations must update request state, history and required feedback in one transaction. They must enforce concurrency and add idempotency protection where duplicate creation requests could occur.

## Testing and known limitations

The verification harness checks requester ownership, public/internal note filtering, staff category permission, invalid identities, malformed IDs, inaccessible records, cookie tampering, status codes, safe failures, cache prevention, cancellation and the absence of write endpoints.

Full FR/NFR acceptance is not claimed. Current evidence covers only the protected request-details read path. The following remain unimplemented or unverified:

- Real registration, sign-in and permission storage
- Request creation, assignment and status updates
- State-pattern implementation
- PostgreSQL, EF Core and migrations
- Transactional writes and concurrent-claim protection
- Reporting and notifications
- Load, backup and recovery testing
- Production HTTPS and security assessment
- Production deployment and remote CI results

## Engineering evidence

The local verification harness completed 29 service and HTTP checks successfully. Bianca Grobler, Michael Ellis and Cameron Purdon reviewed the consolidated PED v2.0 and supporting M2 material for submission on 30 September 2026.

Supporting evidence:

- [PED v2.0](docs/PED_v2.0_FINAL.docx)
- [RTM v2.0](docs/CivicConnect_RTM_v2.0.xlsx)
- [Decision log and ADRs](docs/decisions/README.md)
- [API and integration contract](docs/api-integration.md)
- [Design views](docs/design.md)
- [Traceability demonstration](docs/traceability-demo.md)
- [GitHub evidence](docs/github-evidence.md)
- [Local verification result](docs/evidence/local-verification.txt)
- [AI usage record](docs/ai-usage.md)
