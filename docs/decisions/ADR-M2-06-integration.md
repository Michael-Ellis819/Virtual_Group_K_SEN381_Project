# ADR M2 06 Same application HTTP and explicit coordination

Owner: Cameron Purdon (601963). Date: 30 September 2026.
Status: Accepted for the M2 baseline. The read contract is implemented; write coordination remains planned.

## Context and alternatives
ADR-M2-01 selects a layered application; ADR-M2-03 selects PostgreSQL; ADR-M2-04 selects ASP.NET Core MVC and Razor. The established boundary is a details controller calling an application service. Public third-party APIs, email and a messaging broker are outside initial scope.

Assignment 2 Task 3 recommends REST-style browser/backend interaction and in-process Observer for feedback. Task 1 Problem 1 instead recommends explicit coordination initially. Week 3 Day 3 SO16 distinguishes API contracts from network services and requires explicit failure behaviour.

| Approach | Judgement |
| --- | --- |
| Explicit service calls in one application | Selected: visible call order and a future single transaction for required state/history/feedback. |
| In-process Observer | Deferred: required subscribers complicate ordering and atomic failure handling for few consumers. |
| Broker or separate services | Rejected for M2: delivery and deployment complexity without an approved need. |

## Decision
Use MVC within one application. Establish GET /requests/{id} as a same-application JSON read contract. It is not approval of SR-FUT-02 public integration. Razor remains the presentation choice; a landing view exists, while sign-in and request-details views remain to be built.

Use controller -> RequestQueryService -> IRequestReader and IRequestAccessPolicy -> filtered response. The current reader is a development fixture. EF Core/PostgreSQL is selected but no adapter/migration is claimed. Exact contract and errors: docs/api-integration.md.

For future status mutations, choose explicit orchestration over Task 3's Observer recommendation: current scope has few consumers and history/feedback are required effects. Logging an observer failure would not satisfy FR-005 or NFR-003. State remains Bianca's transition mechanism, not a transaction guarantee.

## Future writes and failures
Before adding a write endpoint: validate input and action permission; delegate legal transitions to StatusTransitionService; persist request/history/requester feedback in one PostgreSQL transaction; enforce competing updates in the database. Any required write failure rolls back. Michael must settle concurrency tokens, category permissions and the adapter. Creation also needs durable idempotency for duplicate submission. No POST route or atomicity test is claimed here.

## Contract evolution and trade-offs
Version the current contract with its consumer in the same repository. No /v1 boundary is needed for external clients that do not yet exist. Breaking changes require an impact review, updated tests, ADR and RTM. Future public integrations need their own version/deprecation policy. Explicit calls couple the coordinator to known interfaces; revisit events when independent optional reactions exist. R-027, R-034, R-036 and R-038 apply. Timing leakage, database retries and production identity remain future checks.

## Sources
Virtual Group K (2026), Assignment 2 Virtual Group K.docx, Task 1 Problem 1, Task 3 and Task 5. SEN381 Teaching Team (2026), Week 3 Day 3 SO16 APIs and Integration Engineering, sections 3 and 4 and failure/contract guidance. Supplied project material.
