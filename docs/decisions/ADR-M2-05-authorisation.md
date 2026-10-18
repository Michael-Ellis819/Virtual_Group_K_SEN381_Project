# ADR M2 05 Central authorisation through an injected policy

Owner: Cameron Purdon (601963). Date: 30 September 2026.
Status: Accepted for the M2 baseline and implemented in the protected request read slice.

## Problem and evidence
FR-003 and FR-008 require access to individual requests; FR-016 and NFR-001 require identity and record-level restrictions. A role check alone would expose other requesters' records or staff categories. Duplicated controller/view checks can drift. This is separate from transition legality, owned by Bianca's StatusTransitionService in ADR-M2-02.

Assignment 2 Virtual Group K.docx, Task 1, 'CivicConnect Design Problem 2: Role-Based Access and Changing Authorisation Rules', compares Strategy per role with a central service behind an abstraction. It recommends the central service for the current three-role model; Task 5 repeats the research link. This ADR makes the project choice.

| Alternative | Benefit | Cost and decision |
| --- | --- | --- |
| Inline controller and view checks | Few classes | Duplicates rules; alternate service callers can bypass checks. Rejected. |
| Strategy objects per role | Independent extension | Extra dispatch/classes for three small rules. Deferred until complexity justifies it. |
| Central policy with dependency injection | One auditable rule set and isolated tests | An extra interface/DTO; policy may grow too large. Selected. |

Microsoft's resource-based authorisation guidance explains why an authentication attribute alone cannot decide access to a loaded resource. OWASP recommends deny-by-default checks. These support an application-service check after loading a resource and before returning data.

## Decision and boundaries
Inject IRequestAccessPolicy, implemented by RequestAccessPolicy, into RequestQueryService. The controller handles HTTP. The service always calls the policy, including for direct non-HTTP callers. IRequestReader supplies a read snapshot; only a filtered RequestDetailsDto reaches presentation. Framework cookie authentication supplies the principal. Request parameters never supply trusted identity, roles or categories.

The implemented operation is read details. Require one authenticated identity, exactly one non-empty user ID, one recognised role and active=true. Requesters can read only their own record, with public notes. Staff can read authorised categories, with internal notes. Management's reporting remit grants no permission to this detail endpoint. Unknown, inactive, missing or ambiguous claims deny access. Read permission grants no write permission.

Future mutation services must check action permission and then delegate transition legality to Bianca's service. Michael owns persistent permission storage and identity integration. This slice's claim contract does not settle his database model or the login flow.

## Benefits and consequences
The application service protects HTTP and direct calls. DTO projection removes internal notes from requester responses. Injection makes isolated failure and permission tests possible. Additional mapping costs are accepted; split the policy into strategies when independently varying rules justify it. Callers must never return raw persistence entities.

Implementation: src/CivicConnect.Web/Security/RequestAccessPolicy.cs; Application/RequestQueryService.cs; Controllers/RequestsController.cs. Verification: tests/CivicConnect.Verification/Program.cs and docs/evidence/local-verification.txt. These are initial checks for FR-003, FR-008, FR-016, NFR-001, NFR-008 and NFR-009. Login, lists, writes and durable storage remain unimplemented; no full requirement is marked Verified.

Changes to role, category or ownership rules affect this ADR, DTOs, contract, tests, RTM and risks R-013/R-017/R-026/R-032. Superseded decisions must remain traceable through change control.

## Sources
- Virtual Group K (2026), Assignment 2 Virtual Group K.docx, Task 1 Problem 2 and Task 5, supplied main snapshot.
- Microsoft (n.d.), Resource-based authorization in ASP.NET Core MVC: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/resource-based (accessed 30 September 2026).
- OWASP Foundation (n.d.), Authorization Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html (accessed 30 September 2026).

## M3 later consequence - 13 October 2026

The central policy is retained and extended with CanChangeStatus. Active category-authorised Staff may mutate; requester read ownership grants no mutation rights. RequestStatusService applies the action check for direct and HTTP callers. C03/BB05/A06 passed; E01 verifies owner note filtering after writes. Version transfer closes the stale-category gap between read permission and saving. Live account provisioning/freshness is still unverified. Change impact: CR-M3-C01. Owner: Cameron; combined work reviewed by Bianca and Michael on 14 October; PR17 has both approvals, is merged and passes combined main CI 38594557024.

## M4 observed consequence 18 October 2026

The portal reuses the central record policy for details and staff writes. Actor parsing rejects ambiguous/unknown/inactive principals, and browser sessions re-read server-owned account state and security stamps when the account adapter is connected. UI checks verify role-injection denial, category denial and disabled-session rejection using test-only accounts. The real account store and permission-scoped collection queries remain integration dependencies; these results are not production identity acceptance.

## M4 final closure 18 October 2026

The final product connects stored accounts and permissions, record-scoped PostgreSQL queries, real MVC forms and transactional commands. Lifecycle legality remains in Bianca’s rules; authorisation decides who may act. Assignment saves one eligible owner with its status/history/feedback, while generic status routes cannot create ownerless assignment. The persistence boundary now rechecks legality, reducing the earlier writer-bypass debt.

Repeated local checks cover the persistent journey, races, injected rollback, safe errors and revocation. The custom harness remains an explicit CI responsibility. Hosted TLS and the full pilot performance measurements remain outstanding. Final teammate reviews must also be recorded before merge.
