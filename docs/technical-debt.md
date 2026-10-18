# Continuing Technical Debt Register - M3

Bianca's supplied TD-01-TD-05 and Michael's PED TD-M3-01-TD-M3-04 retain their IDs. The entries below include Bianca’s completed 14 October conformance review and its scoped closure decisions. Teammate review is recorded separately from GitHub approvals.

| ID | Compromise and consequence | Priority / owner | Remediation and closure evidence | Status |
| --- | --- | --- | --- | --- |
| TD-01 | Layers share one project; folder boundaries can be crossed. | Medium / Bianca | Dependency review; strengthen boundaries when justified. | Open |
| TD-02 | Low-level writer can be called outside validated application use cases. | High / Michael/Cameron | Review all supported writes; require coordinator; prevent new bypasses. | Partial: current API covered |
| TD-03 | Custom executables lack standard discovery/reporting. | Medium / Michael/Cameron | All four projects execute in successful main CI 38594557024; standard discovery/reporting remains a maintainability limitation. | Partial: CI execution verified |
| TD-04 | Architecture consequence review. | Medium / Bianca | Bianca’s completed 14 October review covers layered, State and policy/API consequences; deployment caveat remains under R-054/DEF-M3-C04. | Closed for reviewed architecture |
| TD-05 | Readiness evidence may drift from release ID. | High / Bianca/Cameron | Published implementation 668795a, PR17 approvals, main CI and 15 October documents aligned. Recheck after later changes. | Mitigated at submission |
| TD-M3-01 | Persistence initially separate from access/lifecycle. | High / Michael/Cameron | Review coordinator; run supported API journeys and independent reviews. | Integrated and reviewed for tested increment |
| TD-M3-02 | Audit history covers status only. | High / Michael | Add creation/ownership/priority/due-date/note events and restart/immutability checks. | Open |
| TD-M3-03 | Request submission lacks durable idempotency. | High / Michael | Implement creation and duplicate-submit safeguard; verify double-click behaviour. | Open |
| TD-M3-04 | Staging, backup and restore incomplete. | High / Michael | Verify actual deployed startup, migrations, rollback and backup/restore. | Open |
| TD-M3-C01 | Synthetic identity tickets used for verification; live permission freshness absent. | High / Michael | Implement trusted accounts/permissions and revocation/revalidation tests. | Open |

Preserved database tests exercise raw persistence writes to isolate the adapter. Their direct New -> In Progress assignment is not a legal application lifecycle example. Supported mutations use RequestStatusService. Adding another route requires the same policy and State controls.


## Review and submission evidence 15 October 2026

TD-04 is closed for the reviewed architecture; deployment qualifications remain. All four projects, including the M3 suite, pass main CI 38594557024 on 668795a. The custom reporting limitation remains TD-03. PERF-01 does not establish full pilot performance or recovery. DEF-M3-C06 is fixed in merged code and passes dependency checks. The dated 14 October review remains in reviews/Bianca_M3_Final_Review_Updated.docx; repository evidence was updated on 15 October.

## M4 checkpoint 18 October 2026

| Continuing item | Current outcome and next action |
| --- | --- |
| TD-02 / R-011 | The UI uses the existing status coordinator and Bianca's eligibility/State rules. The low-level writer remains callable and the legacy status API still does not implement owner assignment. Shared integrity integration remains open. |
| TD-03 | CI includes the fifth executable, M4 UI/contract checks. Custom discovery/reporting remains; the source guard's optional-parameter false positive illustrates its limits. |
| TD-M3-02/03 | Complete audit and durable idempotency require Michael's adapter/migrations; an in-memory fixture is not repayment. |
| TD-M3-C01 | Browser controller and session revalidation contract are present. The real account adapter and deployed permission freshness still require verification. |
| TD-M4-C01 | Proposed portal account/request contracts are unbound. Agree them with Michael, implement versioned atomic adapters and run the persistent staging journey. Owner: Cameron/Michael; High; Open. |

Evolution priority: connect original user journeys, verify integrity and permissions, freeze the candidate, then reconsider test-framework/source-analysis improvements. No architecture rewrite is required for M4.

## M4 final closure 18 October 2026

| Existing item | Final disposition and consequence |
| --- | --- |
| TD-01 | Retained. Logical layers share one assembly; source checks and review mitigate rather than physically prevent cross-layer calls. |
| TD-02; R-011 | Reduced. Supported status routes reject ownerless assignment and the low-level status writer rechecks Bianca’s lifecycle rules. Assignment uses the validated-change proof and eligible stored accounts. Privileged direct EF code remains possible. |
| TD-03 | Retained. Custom harnesses are explicitly invoked by CI; standard test discovery/report formats remain future maintenance work. |
| TD-M3-01 | Integrated for the supported journeys: actual account, query, command and status services call PostgreSQL through the defined interfaces. |
| TD-M3-02 | Repaid for the original audit events. Creation, ownership, status, priority, due date and notes record actor/time and survive restart/restore. Administrator tamper resistance is not claimed. |
| TD-M3-03 | Repaid for tested key reuse and races. Actor/key uniqueness is durable; matching payloads return the saved request and changed reuse conflicts. |
| TD-M3-C01 | Repaid for stored identities and live stamp/role/category/active checks. Rate limiting, password reset and production key protection remain security work. |
| TD-M3-04 | Partial. Controlled Staging is supported and a timed local restore passes. Public hosting, trusted certificate, scheduled off-site backups and a hosted rollback exercise are not delivered. |
| TD-05 | Requires final-version discipline. The assessed release identifier and code CI must match submission information; final review and submission must be recorded against that version. |

Evolution priorities are the original performance and deployed-security gaps, support-lifecycle review, a hosting/operating-cost baseline and then proportionate maintenance improvements. No optional feature takes precedence over these gaps.
