# Cameron's M4 presentation integration

Checkpoint: 18 October 2026. Michael and Bianca are continuing their implementation. These contracts are proposed for integration, not recorded as an already agreed team interface.

## Implemented presentation

Razor pages provide sign-in, submission, requester/staff lists, details, feedback, assignment, status changes, notes, priority/due dates and management category/status views. Forms use server validation, antiforgery and normal HTML encoding. The details and status actions call the established `IRequestReader`, `RequestQueryService` and `RequestStatusService`. The UI requires owner assignment through its own command rather than offering status-only New-to-Assigned.

`PortalActor.From` rejects ambiguous, inactive or unknown principals. Assignment uses Bianca's `IOwnershipRule` and `IStatusTransitionService`; management uses `IServiceActivitySummaryCalculator`. The API's existing status contract is retained. Owner enforcement is not extended to the legacy API by this presentation change; that remains part of the shared assignment/integrity integration.

## Required adapters from Michael

Implement `IPortalAccounts` and `IPortalRequestStore` in `Application/PortalContracts.cs`, then register one scoped implementation of each in `AddCivicConnect`. No production fallback, synthetic accounts or in-memory request store are registered. The running product returns unavailable responses for dependent operations until adapters are connected.

| Contract | Required behaviour |
| --- | --- |
| `AuthenticateAsync` / `FindAsync` | Verify hashed credentials; return enabled account, server-owned role/category grants and a security stamp. Increment the stamp after credential/permission changes. No client-selected roles. The cookie validator rereads the account on each portal session request. |
| `ListAsync` | Apply requester ownership or staff-category permission in the database before search, combined filters, counts or ordering. Date bounds are inclusive UTC submission dates. Priority order is Low, Normal, High, not alphabetic. |
| `MetadataAsync` | Return only an authorised record's requester, priority, owner, due date, dated notes and history. The portal additionally removes private notes and staff audit entries for requesters. A reviewed public history projection is a later integration item. |
| `SubmitAsync` | Validate category and lengths again; default priority Normal; create New request, unique reference, creation history and initial public feedback atomically. Scope the durable submission key to the actor and original payload. Repeated matching submissions return the same record; changed payload/key reuse must conflict. |
| `AssignAsync` | Revalidate current actor/category, eligible owner, unowned New state and version inside one transaction. Save owner, Assigned status, actor/time history and public feedback together. At most one competing claim wins. Validate the approved change rather than accepting arbitrary status data. |
| `AddNoteAsync` | Validate authorisation/version/content; save actor/time and explicit public/internal visibility. Append corrections with audit links rather than replacing earlier text. |
| `SetScheduleAsync` | Validate actor/version and controlled priority; preserve UTC due time or null; record each priority/due-date change with actor/time. |
| `ActivityAsync` | Management only; read saved category/status/due data. Return authorised reporting data to Bianca's calculator, including empty datasets. |

The current `ServiceRequestEntity` lacks some of these fields. This work does not modify Michael's entities, migrations, transactional writer or operational startup guard. His reader must continue to project the same saved requests through `IRequestReader` for details/status. Returning a successful submission without making that request readable is an integration failure.

## Verification and handover

`dotnet run --project tests/CivicConnect.M4Tests --configuration Release` runs black-box HTTP/form checks against synthetic adapters compiled only into the test executable. It covers validation boundaries, permissions, privacy, antiforgery, stale conflicts and a complete screen-contract journey. It does not prove production authentication, durable idempotency, simultaneous claims, PostgreSQL-backed portal workflows, pilot performance or deployed acceptance.

The existing regression/database/M3 suites must also pass after adapter integration. Then rerun the submit → assign → progress → resolve → close journey against the actual final staging candidate. Record the deployed URL/environment, revision, operator, outcomes and any defects before marking the corresponding RTM acceptance criteria Met.

For visual inspection only, `dotnet run --project tests/CivicConnect.M4Tests --configuration Release -- --preview` starts a loopback HTTP host with disposable synthetic data. It is not the normal product or deployment. Fixture usernames are requester, staff and manager; the test-only password is printed by the harness. No preview result is evidence of production HTTPS.
