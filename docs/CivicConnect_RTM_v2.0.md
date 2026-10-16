# CivicConnect Requirements Traceability Matrix

Virtual Group K · Milestone 3

Markdown copy of [CivicConnect_RTM_v2.0.xlsx](CivicConnect_RTM_v2.0.xlsx). The established workbook filename is retained as the matrix continues into M3.

Both workbook sheets are included below. Priority and current requirement wording are resolved from the Requirements sheet; the Excel workbook retains the live lookup formulas.

## Requirements

| ID | Requirement | Priority | Acceptance criterion 1 | Acceptance criterion 2 |
| --- | --- | --- | --- | --- |
| FR-001 | Allow requesters to submit a request with a category, description and location. | High | AC-FR-001-1: A valid submission saves one request with a unique reference, date and New status. | AC-FR-001-2: Missing required fields or an invalid category are rejected without saving a request. |
| FR-002 | Use a predefined list of request categories. | High | AC-FR-002-1: A requester can select and save a category from the agreed list. | AC-FR-002-2: An unlisted category is rejected; existing requests retain their saved category. |
| FR-003 | Allow requesters to view the current status of their own requests. | High | AC-FR-003-1: After an authorised update, the requester sees the saved status and update date. | AC-FR-003-2: Entering another requester’s reference does not reveal its status or details. |
| FR-004 | Allow requesters to view their previous requests. | Medium | AC-FR-004-1: The list shows each own request’s reference, category, date and current status, including completed requests. | AC-FR-004-2: Other users’ requests are excluded; an empty list displays a clear message. |
| FR-005 | Provide requester feedback when a request is accepted, rejected, updated or completed. | Medium | AC-FR-005-1: Each event produces feedback showing the event, date and current status; rejection and completion include a reason or summary. | AC-FR-005-2: Feedback remains available later and excludes private staff notes and other users’ information. |
| FR-006 | Allow staff to view requests permitted by their role and assigned categories. | High | AC-FR-006-1: Staff see all and only the requests allowed by their permissions. | AC-FR-006-2: Direct access to a request outside these permissions is denied. |
| FR-007 | Allow staff to search requests and filter by status, category, date and priority, and sort by date or priority. | Medium | AC-FR-007-1: Searches and combined filters return only matching authorised records, including both date-range boundaries. | AC-FR-007-2: Sorting gives the expected order; clearing filters restores the list and no matches displays an empty result. |
| FR-008 | Allow authorised staff to view full request details. | High | AC-FR-008-1: Details show the reference, requester, category, description, location, date, status, priority, owner, due date and notes. | AC-FR-008-2: Unauthorised users cannot open the same details; missing owner or due date is clearly shown. |
| FR-009 | Allow authorised staff to assign a request or accept responsibility for it. | High | AC-FR-009-1: Assignment records one eligible owner, the person making the change and the time. | AC-FR-009-2: Ineligible owners are rejected; simultaneous attempts to claim a request cannot create two owners. |
| FR-010 | Allow request status changes only through the agreed lifecycle. | High | AC-FR-010-1: Allowed changes save the new status, previous status, actor and time. | AC-FR-010-2: Invalid transitions and changes by unauthorised users are rejected without changing the request. |
| FR-011 | Allow staff to record comments, actions and resolution notes against a request. | High | AC-FR-011-1: A nonblank entry is saved with its author and time; blank entries are rejected. | AC-FR-011-2: Requester-visible updates reach the requester; internal notes remain private and corrections preserve earlier entries. |
| FR-012 | Allow authorised staff to resolve and close requests. | High | AC-FR-012-1: Resolving an In Progress request requires a resolution summary and records the actor and time. | AC-FR-012-2: Only an authorised user can close a Resolved request; other closure attempts are rejected. |
| FR-013 | Allow management to view counts of open, overdue, resolved and closed requests. | Medium | AC-FR-013-1: Counts match a known dataset using the stated lifecycle and overdue rules. | AC-FR-013-2: Refreshing shows saved changes, including zero counts; requesters cannot access organisation-wide totals. |
| FR-014 | Allow management to view requests and counts by category and status. | Medium | AC-FR-014-1: Category and status selections show only matching requests and correct counts. | AC-FR-014-2: Clearing selections restores the permitted overview; categories with no matches show zero. |
| FR-015 | Keep request history to support accountability and basic performance reporting. | High | AC-FR-015-1: History records creation, ownership, status, priority, due-date and note changes with actor and time. | AC-FR-015-2: History survives restart and cannot be silently changed by ordinary users; timestamps support elapsed-time calculations. |
| FR-016 | Authenticate users and apply Requester, Staff and Management permissions. | High | AC-FR-016-1: Valid credentials for an enabled account grant the agreed access; invalid credentials grant no session. | AC-FR-016-2: Signed-out users cannot access protected functions, and users cannot grant themselves extra permissions. |
| FR-017 | Record a controlled priority for each request and allow authorised staff to change it. | Medium | AC-FR-017-1: A new request receives the default priority; permitted changes are saved and appear in sorting and filtering. | AC-FR-017-2: Unlisted priorities and requester attempts to change priority are rejected. |
| FR-018 | Allow staff to record due dates and identify overdue requests. | Medium | AC-FR-018-1: A valid due date and any change to it are saved with actor and time; invalid dates are rejected. | AC-FR-018-2: Only open requests past their due time are overdue; requests without a due date are clearly marked. |
| NFR-001 | Protect requests using role and record-level access controls. | High | AC-NFR-001-1: The role/access test set produces zero unauthorised disclosures or changes, including direct requests. | AC-NFR-001-2: Requester ownership and staff-category boundaries are enforced while authorised actions succeed. |
| NFR-002 | Complete at least 95% of submissions, status views, staff searches and management summaries within 3 seconds at pilot load, with at most 1% failures. | Medium | AC-NFR-002-1: Test with 10 concurrent users and 1,000 requests for 10 minutes after a 2-minute warm-up, with at least 100 runs per operation. | AC-NFR-002-2: Measure each operation separately from user action to visible result; record failures, environment and cold-start time separately. |
| NFR-003 | Save request data and its history together without partial updates. | High | AC-NFR-003-1: A failed submission, assignment or status change leaves either the complete change or the previous state. | AC-NFR-003-2: Saved records survive restart; a double-click submission does not create duplicate requests. |
| NFR-004 | Make request submission and status checking easy for users with basic digital skills. | Medium | AC-NFR-004-1: At least four of five representative participants submit a valid request and find its status within five minutes without coaching. | AC-NFR-004-2: All participants use the same task; completion times and errors are recorded. |
| NFR-005 | Provide a responsive web interface that supports keyboard use. | Medium | AC-NFR-005-1: At 360 px and 1366 px viewport widths in Chrome and Edge, controls remain readable and usable without page-level horizontal scrolling. | AC-NFR-005-2: Keyboard users can reach and operate core controls, see focus, submit a request and read validation errors. |
| NFR-006 | Use HTTPS to protect authentication and request data in deployed environments. | High | AC-NFR-006-1: Sign-in and protected pages use a valid certificate with no mixed-content requests. | AC-NFR-006-2: HTTP is redirected or denied without protected content; application-generated credentials and request data use HTTPS only. |
| NFR-007 | Support recovery with no more than 24 hours of lost data and restoration within four hours. | Medium | AC-NFR-007-1: A restore test meets both targets and recovers consistent request, ownership, status and history records. | AC-NFR-007-2: The backup age, restoration time and resulting records are recorded for verification. |
| NFR-008 | Keep sensitive information out of application logs and error messages. | High | AC-NFR-008-1: Tests using dummy secrets and private request content reveal none of that content in logs or user-facing errors. | AC-NFR-008-2: Errors identify the failed action without exposing credentials, connection secrets or internal stack traces to users. |
| NFR-009 | Allow core request rules to be tested with controlled data in a separate test environment. | High | AC-NFR-009-1: Submission, access, assignment, status and reporting rules accept test inputs and produce observable results without production data. | AC-NFR-009-2: Repeating a check from the same starting state gives the same outcome; deliberately invalid behaviour is detected. |

### Requirements sheet notes

Lifecycle | Requests follow New → Assigned → In Progress → Resolved → Closed. A New request may be rejected with a recorded reason. Open means New, Assigned or In Progress. Reopening is not included.

Permissions and feedback | Requesters view their own requests and public updates. Staff access follows role/category permissions. Management accesses service reporting. Feedback is provided within the application; internal staff notes remain private. Email notifications are not included in the initial version.

Priority and overdue rules | Priorities are Low, Normal and High, with Normal as default. Authorised staff can change priorities and set due dates. An open request is overdue when its due time has passed. Requests without a due date are clearly identified and are not counted as overdue. Automatic escalation remains deferred.

Categories | The initial categories are Facility Faults, Damaged Equipment, Security Concerns, IT Support, Maintenance, Lost Property and Other. Each request must use one category from this list.

Quality targets | The numerical performance, usability and recovery targets in NFR-002, NFR-004 and NFR-007 are acceptance targets for the pilot. Hosting options will be assessed against them in M2. Target changes follow change control. Achievement will be demonstrated through testing in later milestones.

Attachments | File and photo uploads are deferred to future scope to limit storage, security and implementation work. Initial submissions contain a category, description and location.

Scope boundaries | External messaging integrations, payments, multi-tenant support and native apps are excluded from the initial version. Attachments, automated escalation, public APIs, AI categorisation and multilingual support remain future scope. Technology and architecture decisions will be addressed in M2.

## RTM traceability

Each requirement below retains all 22 RTM fields. The fields are shown vertically for readability.

### FR-001 — Submit a service request

| Field | Record |
| --- | --- |
| Requirement ID | FR-001 |
| Requirement | Submit a service request |
| Stakeholder and source | Requester; PED §1.2, §2.1.1; MB §3 Requester |
| Scope reference | SR-IN-01 |
| Priority | High |
| Acceptance IDs | AC-FR-001-1; AC-FR-001-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | Creation and input validation not implemented |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Integrity; pilot performance |
| Architecture / module | Submission service (planned) |
| Data / persistence impact | ServiceRequest, Category, RequestHistory; atomic creation and idempotency |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-015; R-036 |
| Current requirement wording | Allow requesters to submit a request with a category, description and location. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Request creation and input-validation endpoint absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-002 — Controlled request categories

| Field | Record |
| --- | --- |
| Requirement ID | FR-002 |
| Requirement | Controlled request categories |
| Stakeholder and source | Requester; Staff; PED §2.1.1, A-02; MB §3 Requester |
| Scope reference | SR-IN-02 |
| Priority | High |
| Acceptance IDs | AC-FR-002-1; AC-FR-002-2 |
| Design reference | ADR-M2-03; ADR-M2-04; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | Controlled catalogue and persistent keys not implemented |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Integrity |
| Architecture / module | Category catalogue (planned) |
| Data / persistence impact | Category FK; retain referenced category |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-019 |
| Current requirement wording | Use a predefined list of request categories. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Controlled category catalogue/enforced category key absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-003 — View own request status

| Field | Record |
| --- | --- |
| Requirement ID | FR-003 |
| Requirement | View own request status |
| Stakeholder and source | Requester; PED §1.2, §2.1.1; MB §3 Requester |
| Scope reference | SR-IN-03 |
| Priority | High |
| Acceptance IDs | AC-FR-003-1; AC-FR-003-2 |
| Design reference | ADR-M2-01; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: RequestQueryService; RequestAccessPolicy; EfRequestReader; RequestStatusService. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | E01/E02/A04; S01/S03/H04/H05/H07; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Access isolation |
| Architecture / module | RequestsController; RequestQueryService |
| Data / persistence impact | Michael EF Core/PostgreSQL reader and persisted synthetic requests/notes; authorised application projection. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-013; R-017 |
| Current requirement wording | Allow requesters to view the current status of their own requests. |
| Expected / observed result | Owner reads saved Closed status/time and permitted notes; foreign record returns 404. |
| Defect or remaining acceptance gap | Full live account/UI acceptance pending. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-004 — View own request history

| Field | Record |
| --- | --- |
| Requirement ID | FR-004 |
| Requirement | View own request history |
| Stakeholder and source | Requester; PED §1.2, §2.1.1; MB §3 Requester |
| Scope reference | SR-IN-04 |
| Priority | Medium |
| Acceptance IDs | AC-FR-004-1; AC-FR-004-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | List/history query not implemented |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Access isolation |
| Architecture / module | Requester history query (planned) |
| Data / persistence impact | Requester-scoped ServiceRequest collection |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-013 |
| Current requirement wording | Allow requesters to view their previous requests. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Own request history/list workflow absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-005 — Requester feedback

| Field | Record |
| --- | --- |
| Requirement ID | FR-005 |
| Requirement | Requester feedback |
| Stakeholder and source | Requester; PED §2.1.1; §5.2 concern 5; MB §3 Requester |
| Scope reference | SR-IN-05 |
| Priority | Medium |
| Acceptance IDs | AC-FR-005-1; AC-FR-005-2 |
| Design reference | ADR-M2-02; ADR-M2-03; ADR-M2-04; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: RequestStatusService; EfRequestStatusWriter; EfRequestReader. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | E01/E02; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Integrity; access isolation |
| Architecture / module | RequestStatusService; transactional public feedback; authorised feedback projection |
| Data / persistence impact | PostgreSQL status/history/public-note transaction and optimistic version; broader schema/acceptance gaps remain. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-027; R-028 |
| Current requirement wording | Provide requester feedback when a request is accepted, rejected, updated or completed. |
| Expected / observed result | Status changes create persistent public feedback; rejection has a reason. |
| Defect or remaining acceptance gap | Event/date presentation and full feedback scope pending. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-006 — Authorised staff request list

| Field | Record |
| --- | --- |
| Requirement ID | FR-006 |
| Requirement | Authorised staff request list |
| Stakeholder and source | Staff; PED §1.2, §2.1.1, §2.2.3; MB §3 Staff |
| Scope reference | SR-IN-06 |
| Priority | High |
| Acceptance IDs | AC-FR-006-1; AC-FR-006-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: RequestAccessPolicy; request detail boundary only. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | S05/H09/BB05; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Access isolation |
| Architecture / module | Staff list query (planned) |
| Data / persistence impact | Category permission filter before counts/pagination |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-013; R-032 |
| Current requirement wording | Allow staff to view requests permitted by their role and assigned categories. |
| Expected / observed result | Outside-category detail/mutation denied. |
| Defect or remaining acceptance gap | Authorised staff list absent. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-007 — Search, filter and sort

| Field | Record |
| --- | --- |
| Requirement ID | FR-007 |
| Requirement | Search, filter and sort |
| Stakeholder and source | Staff; PED §2.1.1; MB §3 Staff |
| Scope reference | SR-IN-07 |
| Priority | Medium |
| Acceptance IDs | AC-FR-007-1; AC-FR-007-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | No search, sorting or load evidence |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Pilot performance; access isolation |
| Architecture / module | Search/filter query (planned) |
| Data / persistence impact | Permission-constrained sort/filter/index strategy |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-016; R-022 |
| Current requirement wording | Allow staff to search requests and filter by status, category, date and priority, and sort by date or priority. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Search/filter/sort workflow absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-008 — Full request details

| Field | Record |
| --- | --- |
| Requirement ID | FR-008 |
| Requirement | Full request details |
| Stakeholder and source | Staff; PED §2.1.1; MB §3 Staff |
| Scope reference | SR-IN-08 |
| Priority | High |
| Acceptance IDs | AC-FR-008-1; AC-FR-008-2 |
| Design reference | ADR-M2-01; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: RequestQueryService; RequestAccessPolicy; EfRequestReader. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | S04/S05/H08/H09; database checks; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Access isolation |
| Architecture / module | RequestsController; RequestQueryService |
| Data / persistence impact | Michael EF Core/PostgreSQL reader and persisted synthetic requests/notes; authorised application projection. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-013; R-017 |
| Current requirement wording | Allow authorised staff to view full request details. |
| Expected / observed result | Permitted staff sees internal notes; wrong category denied. |
| Defect or remaining acceptance gap | Full details omit owner/priority/due-date/requester; AC1 incomplete. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-009 — Assign or accept ownership

| Field | Record |
| --- | --- |
| Requirement ID | FR-009 |
| Requirement | Assign or accept ownership |
| Stakeholder and source | Staff; PED §2.1.1; MB §3 Staff |
| Scope reference | SR-IN-09 |
| Priority | High |
| Acceptance IDs | AC-FR-009-1; AC-FR-009-2 |
| Design reference | ADR-M2-02; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | Database concurrency mechanism and simultaneous-claim test pending |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Integrity |
| Architecture / module | Assignment service (planned) |
| Data / persistence impact | Conditional atomic owner update plus RequestHistory |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-015 |
| Current requirement wording | Allow authorised staff to assign a request or accept responsibility for it. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Eligible owner assignment/concurrent claiming absent; Assigned status is not owner assignment. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-010 — Controlled status transitions

| Field | Record |
| --- | --- |
| Requirement ID | FR-010 |
| Requirement | Controlled status transitions |
| Stakeholder and source | Staff; PED §2.1.1; MB §3 Staff |
| Scope reference | SR-IN-10 |
| Priority | High |
| Acceptance IDs | AC-FR-010-1; AC-FR-010-2 |
| Design reference | ADR-M2-02; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: StatusTransitionService; RequestStatusService; EfRequestStatusWriter. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | C01-C06; BB05; A03/A05/A06; E01/E02; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | Original requirement/AC unchanged. CR-M3-C03: team M3 scope confirmed 15 Oct; read/status increment conditionally ready in controlled simulation. PR17 and combined CI complete. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Integrity; testability |
| Architecture / module | StatusTransitionService; six State classes; RequestStatusService |
| Data / persistence impact | PostgreSQL status/history/public-note transaction and optimistic version; broader schema/acceptance gaps remain. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-011; R-027; R-047 closed for tested lifecycle; TD-04 closed for reviewed architecture (14 Oct 2026). |
| Current requirement wording | Allow request status changes only through the agreed lifecycle. |
| Expected / observed result | Legal lifecycle saved with actor/time/history; invalid/unauthorised/stale changes rejected. |
| Defect or remaining acceptance gap | Synthetic identities; simultaneous HTTP/audit coverage limited. Team accepts controlled M3 simulation; live accounts and full product remain incomplete for M4 finalisation. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-011 — Record actions and notes

| Field | Record |
| --- | --- |
| Requirement ID | FR-011 |
| Requirement | Record actions and notes |
| Stakeholder and source | Staff; PED §2.1.1; MB §3 Staff |
| Scope reference | SR-IN-11 |
| Priority | High |
| Acceptance IDs | AC-FR-011-1; AC-FR-011-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: DTO note filtering; EfRequestStatusWriter public feedback. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | S02/H05; BB03/BB04; E01/E02; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Access isolation; integrity |
| Architecture / module | RequestQueryService note projection; transactional status-related public-note writer; general note workflow unimplemented |
| Data / persistence impact | PostgreSQL status/history/public-note transaction and optimistic version; broader schema/acceptance gaps remain. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-017 |
| Current requirement wording | Allow staff to record comments, actions and resolution notes against a request. |
| Expected / observed result | Internal notes excluded from owner response; invalid feedback produces no writes. |
| Defect or remaining acceptance gap | General note author/time/correction workflow absent. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-012 — Resolve and close requests

| Field | Record |
| --- | --- |
| Requirement ID | FR-012 |
| Requirement | Resolve and close requests |
| Stakeholder and source | Staff; PED §2.1.1; MB §3 Staff |
| Scope reference | SR-IN-12 |
| Priority | High |
| Acceptance IDs | AC-FR-012-1; AC-FR-012-2 |
| Design reference | ADR-M2-02; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: StatusTransitionService; RequestStatusService; status POST. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | E01/E02; C01/C02; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Integrity; access isolation |
| Architecture / module | RequestStatusService; State rules for resolution and closure; transactional history/feedback writer |
| Data / persistence impact | PostgreSQL status/history/public-note transaction and optimistic version; broader schema/acceptance gaps remain. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-011; R-027; R-047 closed for tested lifecycle; TD-04 closed for reviewed architecture (14 Oct 2026). |
| Current requirement wording | Allow authorised staff to resolve and close requests. |
| Expected / observed result | Staff resolves with summary then closes; terminal reopen denied. |
| Defect or remaining acceptance gap | Broader live account/permission acceptance pending. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-013 — Service activity summary

| Field | Record |
| --- | --- |
| Requirement ID | FR-013 |
| Requirement | Service activity summary |
| Stakeholder and source | Management; PED §1.2, §2.1.1; MB §3 Management |
| Scope reference | SR-IN-13 |
| Priority | Medium |
| Acceptance IDs | AC-FR-013-1; AC-FR-013-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | No report implementation |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Pilot performance; access isolation |
| Architecture / module | Management reporting (planned) |
| Data / persistence impact | Aggregate status/open/overdue queries |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-014; R-016 |
| Current requirement wording | Allow management to view counts of open, overdue, resolved and closed requests. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Management service counts absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-014 — Management category/status analysis

| Field | Record |
| --- | --- |
| Requirement ID | FR-014 |
| Requirement | Management category/status analysis |
| Stakeholder and source | Management; PED §2.1.1; MB §3 Management |
| Scope reference | SR-IN-14 |
| Priority | Medium |
| Acceptance IDs | AC-FR-014-1; AC-FR-014-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | No grouping report implementation |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Pilot performance; access isolation |
| Architecture / module | Management category/status report (planned) |
| Data / persistence impact | Aggregate Category/ServiceRequest grouping |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-014; R-016 |
| Current requirement wording | Allow management to view requests and counts by category and status. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Management category/status analysis absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-015 — Accountability and performance history

| Field | Record |
| --- | --- |
| Requirement ID | FR-015 |
| Requirement | Accountability and performance history |
| Stakeholder and source | Management; Staff; PED §1.2, §2.1.1; MB §3 Management |
| Scope reference | SR-IN-15; SR-IN-11 |
| Priority | High |
| Acceptance IDs | AC-FR-015-1; AC-FR-015-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: RequestStatusHistoryEntity; EfRequestStatusWriter. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | E01; database checks; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Integrity; access isolation |
| Architecture / module | Accountability reporting (planned) |
| Data / persistence impact | PostgreSQL status/history/public-note transaction and optimistic version; broader schema/acceptance gaps remain. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-014 |
| Current requirement wording | Keep request history to support accountability and basic performance reporting. |
| Expected / observed result | Four status audit edges with correct actor/time/version; history saved in PostgreSQL. |
| Defect or remaining acceptance gap | Creation/ownership/priority/due-date/note history and restart/immutability checks pending. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-016 — Authenticated role access

| Field | Record |
| --- | --- |
| Requirement ID | FR-016 |
| Requirement | Authenticated role access |
| Stakeholder and source | Requester; Staff; Management; PED §2.2.3; §5.2 concern 1; MB §16 |
| Scope reference | Cross-cutting: SR-IN-03, 06, 08, 12-15 |
| Priority | High |
| Acceptance IDs | AC-FR-016-1; AC-FR-016-2 |
| Design reference | ADR-M2-01; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: Cookie authentication; central action/read policy. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | A02/BB05; S07-S12/H02/H03; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Access isolation |
| Architecture / module | Framework cookie gate and RequestAccessPolicy |
| Data / persistence impact | Trusted principal claims; user store/provisioning planned |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-013; R-031; R-032 |
| Current requirement wording | Authenticate users and apply Requester, Staff and Management permissions. |
| Expected / observed result | Signed-out calls return 401; ambiguous/inactive identities denied. |
| Defect or remaining acceptance gap | Actual credential login/provisioning and permission freshness absent. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### FR-017 — Request priority

| Field | Record |
| --- | --- |
| Requirement ID | FR-017 |
| Requirement | Request priority |
| Stakeholder and source | Staff; PED §2.1.1; MB §2 prioritisation need |
| Scope reference | SR-IN-07; SR-IN-15 |
| Priority | Medium |
| Acceptance IDs | AC-FR-017-1; AC-FR-017-2 |
| Design reference | ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | No priority mutation implementation |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Integrity; access isolation |
| Architecture / module | Priority update service (planned) |
| Data / persistence impact | Controlled Low/Normal/High; default Normal |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-036 |
| Current requirement wording | Record a controlled priority for each request and allow authorised staff to change it. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Controlled priority/default/update absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### FR-018 — Due dates and overdue identification

| Field | Record |
| --- | --- |
| Requirement ID | FR-018 |
| Requirement | Due dates and overdue identification |
| Stakeholder and source | Staff; Management; PED §2.1.1; MB §3 Management |
| Scope reference | SR-IN-13; SR-IN-15 |
| Priority | Medium |
| Acceptance IDs | AC-FR-018-1; AC-FR-018-2 |
| Design reference | ADR-M2-02; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | No due-date/overdue implementation |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Integrity; pilot performance |
| Architecture / module | Due-date service and overdue query (planned) |
| Data / persistence impact | Open states plus DueDate < now; null not overdue |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-014 |
| Current requirement wording | Allow staff to record due dates and identify overdue requests. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | Due-date/overdue calculation absent. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### NFR-001 — Access-control isolation

| Field | Record |
| --- | --- |
| Requirement ID | NFR-001 |
| Requirement | Access-control isolation |
| Stakeholder and source | Requester; Staff; Management; PED §2.2.2 Security; R-004; MB §16 |
| Scope reference | Cross-cutting: all protected scope |
| Priority | High |
| Acceptance IDs | AC-NFR-001-1; AC-NFR-001-2 |
| Design reference | ADR-M2-01; ADR-M2-03; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: RequestAccessPolicy; filtered DTO; versioned status command. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | C03/BB05/A04/A06/E01/E02; S02-S12; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Access isolation |
| Architecture / module | RequestAccessPolicy and DTO projection |
| Data / persistence impact | Michael EF Core/PostgreSQL reader and persisted synthetic requests/notes; authorised application projection. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-013; R-017; R-032 |
| Current requirement wording | Protect requests using role and record-level access controls. |
| Expected / observed result | No unauthorised disclosure/change in selected role/record cases. |
| Defect or remaining acceptance gap | Synthetic-ticket access matrix only; live permission freshness unverified. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### NFR-002 — Pilot response time

| Field | Record |
| --- | --- |
| Requirement ID | NFR-002 |
| Requirement | Pilot response time |
| Stakeholder and source | Requester; Staff; Management; PED A-03, §2.2.2 Quality/Cost; §5.2 concern 3; MB §4 |
| Scope reference | SR-IN-01; SR-IN-03; SR-IN-07; SR-IN-13 |
| Priority | Medium |
| Acceptance IDs | AC-NFR-002-1; AC-NFR-002-2 |
| Design reference | ADR-M2-01; ADR-M2-03; ADR-M2-04; docs/decisions/README.md |
| Issue / PR / code | tests/CivicConnect.PerformanceTests/Program.cs; EfRequestReader/PostgreSQL. PR16 performance merged into combined published implementation 668795a through PR17. |
| Test evidence | PERF-01: 200 reads, max 10 concurrent; 200 successes, 0 errors; 1.62 ms mean, 0.41 ms median, 8.69 ms p95; 0.12 s; 1696.16 ops/sec. |
| Change reference | M2 acceptance criteria unchanged. Original PERF-01 second run confirmed 14 Oct; combined main CI passes on 668795a. Full pilot acceptance remains unverified. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Pilot performance |
| Architecture / module | EfRequestReader; EF Core/PostgreSQL; separate concurrent-read benchmark |
| Data / persistence impact | One temporary request; separate DbContext per read; setup/cleanup excluded from measured workload. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-016; R-022; R-057; M3-R05 |
| Current requirement wording | Complete at least 95% of submissions, status views, staff searches and management summaries within 3 seconds at pilot load, with at most 1% failures. |
| Expected / observed result | No separate database-level target. Observed: 200/200 reads succeed; 1.62 ms mean, 8.69 ms p95. Full user-visible pilot acceptance not established. |
| Defect or remaining acceptance gap | AC-NFR-002-1/2 unchanged: 1000 requests, 10 users, 10 minutes after 2-minute warm-up; user-visible timings/cold start untested. Exact execution commit not recorded. |
| M3 execution evidence | PERF-01: docs/evidence/m3-performance-results.txt and m3-performance-run-confirmation.md; original execution SHA unconfirmed. Combined main CI on 668795a: https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024 (all four projects; separate from original human benchmark). |
| Evidence date / executor | 14 October 2026; Michael Ellis 600332; macOS; SDK 8.0.425; EF Core 8; PostgreSQL 17. Second run confirmed through Cameron. |

### NFR-003 — Data integrity on save

| Field | Record |
| --- | --- |
| Requirement ID | NFR-003 |
| Requirement | Data integrity on save |
| Stakeholder and source | Requester; Staff; Management; PED §1.1; §7.1.2; MB §15 |
| Scope reference | SR-IN-01; SR-IN-09-12; SR-IN-15 |
| Priority | High |
| Acceptance IDs | AC-NFR-003-1; AC-NFR-003-2 |
| Design reference | ADR-M2-02; ADR-M2-03; ADR-M2-04; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: EF transaction; concurrency version; status coordinator. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | A05/A06; E01; database checks; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Integrity |
| Architecture / module | RequestStatusService; EfRequestStatusWriter; transaction and optimistic concurrency |
| Data / persistence impact | PostgreSQL status/history/public-note transaction and optimistic version; broader schema/acceptance gaps remain. |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-015 |
| Current requirement wording | Save request data and its history together without partial updates. |
| Expected / observed result | Failed status write rolls back; stale version rejected; saved effects agree. |
| Defect or remaining acceptance gap | Submission/assignment/idempotency and complete process restart unverified. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### NFR-004 — Requester usability

| Field | Record |
| --- | --- |
| Requirement ID | NFR-004 |
| Requirement | Requester usability |
| Stakeholder and source | Requester; PED §1.2, A-01; MB §5 |
| Scope reference | SR-IN-01; SR-IN-03; SR-IN-04 |
| Priority | Medium |
| Acceptance IDs | AC-NFR-004-1; AC-NFR-004-2 |
| Design reference | ADR-M2-01; ADR-M2-04; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | No representative participant evaluation |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Usability supports pilot scope |
| Architecture / module | Future requester UI |
| Data / persistence impact | No additional data decision |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-021 |
| Current requirement wording | Make request submission and status checking easy for users with basic digital skills. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | No five-participant usability exercise. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### NFR-005 — Responsive and keyboard-usable interface

| Field | Record |
| --- | --- |
| Requirement ID | NFR-005 |
| Requirement | Responsive and keyboard-usable interface |
| Stakeholder and source | Requester; Staff; Management; PED §2.1.2; A-01; MB §5, §15 |
| Scope reference | Cross-cutting; responsive web scope |
| Priority | Medium |
| Acceptance IDs | AC-NFR-005-1; AC-NFR-005-2 |
| Design reference | ADR-M2-01; ADR-M2-04; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | Landing exists; viewport/keyboard/core-route acceptance pending |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Accessibility/responsive presentation |
| Architecture / module | Razor landing; future functional views |
| Data / persistence impact | No additional data decision |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-021 |
| Current requirement wording | Provide a responsive web interface that supports keyboard use. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | No representative responsive/keyboard workflow evaluation. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### NFR-006 — Protected data in transit

| Field | Record |
| --- | --- |
| Requirement ID | NFR-006 |
| Requirement | Protected data in transit |
| Stakeholder and source | Requester; Staff; Management; PED §2.2.2 Security; §5.2 concern 6; MB §16 |
| Scope reference | Cross-cutting: protected product data |
| Priority | High |
| Acceptance IDs | AC-NFR-006-1; AC-NFR-006-2 |
| Design reference | ADR-M2-01; ADR-M2-03; ADR-M2-04; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: Secure cookies; temporary HTTPS test host. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | M3 suite ran over HTTPS; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Secure deployment constraint |
| Architecture / module | Future HTTPS host and cookie deployment |
| Data / persistence impact | TLS for protected HTTP and database traffic |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-033 |
| Current requirement wording | Use HTTPS to protect authentication and request data in deployed environments. |
| Expected / observed result | Temporary loopback certificate and secure cookies used for test host. |
| Defect or remaining acceptance gap | Real deployed certificate/HTTPS redirect and browser acceptance pending. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### NFR-007 — Recoverable request records

| Field | Record |
| --- | --- |
| Requirement ID | NFR-007 |
| Requirement | Recoverable request records |
| Stakeholder and source | Project team; Management; PED §5.2 concern 7; §7.1; MB §17 |
| Scope reference | Cross-cutting: retained request/history data |
| Priority | Medium |
| Acceptance IDs | AC-NFR-007-1; AC-NFR-007-2 |
| Design reference | ADR-M2-03; ADR-M2-04; docs/decisions/README.md |
| Issue / PR / code | Planned. Design source PR #10 (architecture) / #11 (data/technology); no application implementation for this row. |
| Test evidence | RPO/RTO targets retained; no restore evidence |
| Change reference | M2 consolidation 2026-09-30; requirement/AC unchanged; M3 2026-10-13: evidence evolved; requirement/AC unchanged; CR-M3-C01. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | Planned |
| ASR / quality driver | Recovery constraint |
| Architecture / module | Future backup/restore process |
| Data / persistence impact | PostgreSQL backup and timed restore |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-018; R-040 |
| Current requirement wording | Support recovery with no more than 24 hours of lost data and restoration within four hours. |
| Expected / observed result | Not executed for complete acceptance. |
| Defect or remaining acceptance gap | No backup/restore measurement against RPO/RTO. |
| M3 execution evidence | No complete M3 execution evidence; see docs/release-evidence.md. |
| Evidence date / executor | 2026-10-13 evidence review. |

### NFR-008 — Privacy of logs and errors

| Field | Record |
| --- | --- |
| Requirement ID | NFR-008 |
| Requirement | Privacy of logs and errors |
| Stakeholder and source | Requester; Staff; Management; PED §5.2 concern 6; R-004, R-005; MB §16 |
| Scope reference | Cross-cutting: restricted information |
| Priority | High |
| Acceptance IDs | AC-NFR-008-1; AC-NFR-008-2 |
| Design reference | ADR-M2-01; ADR-M2-04; ADR-M2-05; ADR-M2-06; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: Generic HTTP exception boundary. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | A05; H12/H13; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Privacy and failure behaviour |
| Architecture / module | Safe error middleware; no private payload logging |
| Data / persistence impact | Filtered DTO; generic exception handling |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-034; R-035 |
| Current requirement wording | Keep sensitive information out of application logs and error messages. |
| Expected / observed result | 500 does not disclose exception/private marker; responses no-store. |
| Defect or remaining acceptance gap | Full dummy-secret/log-content audit pending. See docs/defects.md and technical-debt.md. |
| M3 execution evidence | docs/testing-m3.md; docs/evidence/m3/personal-20261013-181846/ (integration.txt, regression.txt, database.txt; mapped IDs in column I). |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

### NFR-009 — Testability of core business rules

| Field | Record |
| --- | --- |
| Requirement ID | NFR-009 |
| Requirement | Testability of core business rules |
| Stakeholder and source | Project team; PED §5.2 concern 4; MB §15; M1 §4 |
| Scope reference | Cross-cutting: core request lifecycle |
| Priority | High |
| Acceptance IDs | AC-NFR-009-1; AC-NFR-009-2 |
| Design reference | ADR-M2-01; ADR-M2-04; ADR-M2-05; ADR-M2-06; ADR-M2-07; docs/decisions/README.md |
| Issue / PR / code | src/CivicConnect.Web/: Injected reader/policy/writer; isolated executable suites. M2 PR12; M3 PR14; published combined implementation 668795a (PR17). |
| Test evidence | C01-C06; BB01-BB05; A01-A06; E01/E02; passed 2026-10-13 where mapped; docs/testing-m3.md. |
| Change reference | M2 requirement/AC unchanged; CR-M3-C01 retained. Bianca/Michael review and PR17 approvals: 14 Oct. Published 668795a; CI verified; docs 15 Oct 2026. |
| Release evidence | 15 Oct 2026; 668795a; PR17/CI pass. CONDITIONALLY READY: agreed M3 read/status increment, simulated staging only. PED Section 14. |
| Implementation status | In Development |
| ASR / quality driver | Testability |
| Architecture / module | Injected reader/policy; verification executable |
| Data / persistence impact | Synthetic data with controlled store fault |
| Technology decision | ADR-M2-04: ASP.NET Core MVC/Razor; EF Core/Npgsql 8.0.11; PostgreSQL adapters/migrations implemented by Michael (PR14). |
| ADR / risk reference | R-009; R-038; R-047 closed for tested lifecycle; R-053 closed for current combined CI coverage (run 38594557024). |
| Current requirement wording | Allow core request rules to be tested with controlled data in a separate test environment. |
| Expected / observed result | Repeatable core access/status cases; negative control fails forbidden transition. |
| Defect or remaining acceptance gap | Submission/assignment/reporting and real login remain original requirements planned for M4 finalisation; current evidence covers the agreed M3 read/status increment. |
| M3 execution evidence | Personal run: docs/evidence/m3/personal-20261013-181846/; IDs in I28. Combined main CI: https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024 on 668795a; regression/database/M3/performance, analysis and dependency checks pass. |
| Evidence date / executor | 2026-10-13 18:18-18:19 SAST; Cameron Purdon 601963; f774bf8. |

## RTM sheet notes

| Field | Record |
| --- | --- |
| Source key | PED = supplied PED1, sections 1.2 and 2.1–2.2. MB = SEN381 CivicConnect Master Project Brief, sections 3 and 11. |
| RTM format | Original requirements/ACs retained. Published 668795a; PR17 approvals and main CI 38594557024 pass. Cameron confirmed team M3 read/status scope on 15 Oct 2026. CONDITIONALLY READY for controlled simulated staging; CR-M3-C03 and PED Section 14. |
| Status | Planned = no working implementation. In Development = partial evidence; full acceptance unverified. Team plans unfinished original features for M4 finalisation; no requirements removed. M3 conditional readiness applies only to the agreed increment and simulation. |
| Maintenance | Keep requirement IDs stable. Update affected acceptance criteria and trace links when a change is approved. Add actual evidence links as work progresses. |
| Source PR 10 | https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/10 |
| Source PR 11 | https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/11 |
| M1 history | https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/3 |
| Local evidence | docs/evidence/local-verification.txt; checked 2026-09-30; 29 service/HTTP checks passed; fixture only |

## Workbook lookup formulas

For each requirement row `r` (rows 2–28), the workbook uses:

- Priority: `=INDEX(Requirements!$C$2:$C$28,MATCH(Ar,Requirements!$A$2:$A$28,0))`
- Current requirement wording: `=INDEX(Requirements!$B$2:$B$28,MATCH(Ar,Requirements!$A$2:$A$28,0))`
