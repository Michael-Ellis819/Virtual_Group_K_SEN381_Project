# Initial interface and integration contract

Owner: Cameron. Contract date: 30 September 2026. Decisions: ADR-M2-05 and ADR-M2-06.

## Established HTTP boundary

GET `/requests/{id}` returns JSON from the same ASP.NET Core application. `{id}` is a non-empty UUID. There is no request body and no write action. Unknown query parameters are ignored and cannot override the authenticated subject. Signed-out requests are rejected before controller validation. GET `/` is a public Razor development landing page.

Success fields are `requestId`, `referenceNumber`, `categoryId`, `description`, `location`, `status`, `updatedAt` and `notes` (array of strings). Times use DateTimeOffset JSON representation. The fixture category identifier is the string `IT Support`; it is not a final database key design. Map Michael's Category primary key through the adapter when that schema is implemented, and update the contract/test fixtures together if the public identifier changes.

Example owner response for the synthetic request:

```json
{
  "requestId": "11111111-1111-1111-1111-111111111111",
  "referenceNumber": "CC-DEMO-001",
  "categoryId": "IT Support",
  "description": "Demo network fault",
  "location": "Demo lab",
  "status": "New",
  "updatedAt": "2026-09-30T08:00:00+00:00",
  "notes": ["Request received."]
}
```

The actual fields contain strings, not executable markup. A future Razor details view must keep normal HTML encoding. This slice has no free-text write endpoint or database query; it does not establish SQL injection or stored-XSS acceptance.

## Authentication and authorisation

Cookie authentication validates the framework-protected ticket. The server-issued principal requires one identity and exactly one `NameIdentifier`, one role and `active=true`. Staff need a category claim matching the record. These trusted claims must eventually come from server-owned user/permission data. Clients must never choose their role, active state or categories. Login, revocation, inactive-user refresh and account provisioning remain unimplemented. Short-lived/revalidated tickets are required before real access can rely on mutable permissions.

| Caller | Detail permission | Notes |
| --- | --- | --- |
| Signed out or invalid cookie | 401 | No record returned |
| Active Requester, own record | 200 | Public notes only |
| Requester, another owner's record | 404 | No record returned |
| Active Staff, permitted category | 200 | Public and internal notes |
| Staff outside category | 404 | No record returned |
| Management, unknown/inactive/ambiguous user | 404 after authentication | Detail access denied; reporting is a separate future operation |

The application service repeats resource checks for non-HTTP callers. Response DTOs exclude requester ID and raw visibility metadata. Missing and inaccessible records share a 404 title; this reduces obvious enumeration but does not prove constant response timing.

## Errors and failure handling

| Result | HTTP behaviour | Retry or consequence |
| --- | --- | --- |
| Malformed/empty UUID, authenticated caller | 400 ProblemDetails, explicit UUID error | Correct input |
| Unauthenticated/invalid ticket | 401, no redirect or response body promised | Obtain a valid session after login exists |
| Absent or inaccessible resource | 404 ProblemDetails, `Request not found.` | Do not disclose which condition occurred |
| Unexpected store/service failure | 500 ProblemDetails, generic title, no exception message | No automatic application retry; read can be retried deliberately |
| Client cancellation | Cancellation forwarded to reader | Stop work; no completed HTTP response is promised |

ProblemDetails may include framework metadata such as `type`, `status` and `traceId`; consumers must not depend on exact trace values. Responses set Cache-Control: no-store. The slice intentionally records no private payload or exception detail in custom logs. Production observability remains to be designed. TLS is required for real authentication; the current loopback checks do not validate NFR-006.

## Internal contracts and future work

`RequestQueryService.ReadAsync(principal, id, cancellationToken)` returns a filtered DTO or null for invalid/denied/missing reads; storage faults propagate to the safe HTTP boundary. `IRequestReader.FindAsync` returns a snapshot/null and observes cancellation. `IRequestAccessPolicy.EvaluateRead` returns read permission and whether internal notes may be included.

The reader implementation is currently synthetic. No cross-process module integration, network DB call, POST/PUT/PATCH route, event broker, notification delivery or public API is implemented. Future list queries must constrain the query by permission before pagination/counting; filtering only a detail DTO is insufficient for FR-006/FR-007.

Before write routes are built: decide DTO bounds, category key mapping, permission freshness, antiforgery for cookie-authenticated mutations, idempotency keys, transaction ownership and optimistic concurrency. Return validation/conflict errors only once those operations exist. Read access never permits assignment/status writes. ADR-M2-06 specifies the required orchestration and transaction direction without inventing endpoints.
