# M3 defect and finding register

Updated 15 October 2026 following PR17 merge, two teammate approvals and successful combined main CI. Critical/High defects affecting confidentiality, integrity or essential agreed scope block release. The team confirmed the protected read/status increment and controlled simulation as M3 scope on 15 October. Original unfinished requirements remain open for M4 finalisation; this is not acceptance of a defect affecting the included core.

| ID | Severity / impact | Status | Fix or next action / owner | Requirement / regression |
| --- | --- | --- | --- | --- |
| DEF-M3-C01 | High: persistence not reachable through an authorised status API. | Fixed, personally verified, reviewed and merged in PR17; combined main CI passes. | Coordinator, Staff action policy, State validation and POST contract; Cameron/Michael review. | FR-010; C03/C04/BB05/E01/E02. |
| DEF-M3-C02 | High: stale category permission could survive status-only comparison. | Fixed, personally verified, reviewed and merged in PR17; combined main CI passes. | Carry checked version and reject changed record; Cameron/Michael review. | NFR-001, R-050; A06. |
| DEF-M3-C03 | Medium test infrastructure: Windows TLS failed before HTTP cases. | Corrected. | Reimport temporary certificate for Schannel; Cameron. Original failure log retained. | integration-first-run.txt; final integration.txt passes 19. |
| DEF-M3-C04 | High limitation for normal deployment: Program refuses Staging. | Open for normal deployment; verified M3 simulation accepted by the team on 15 October. | Use the separate HTTPS Staging host and disposable data for M3; Michael owns operational deployment finalisation. | R-054; E01/E02 pass in the permitted simulation. |
| DEF-M3-C05 | High original-scope gap: login, submission, owner assignment, lists/search and reporting incomplete. | Open in original scope; planned M4 finalisation. | Team confirmed the protected read/status increment as M3 scope on 15 October. Remaining features retain original requirements and acceptance criteria; team implementation owners remain responsible. | Existing FR/NFR rows remain partial/planned; no acceptance is invented. |

The failed gate probe is a deliberate verification experiment, not a product defect. PERF-01 is evidenced; combined main CI and two PR17 approvals are complete. Full pilot-load/recovery acceptance remains limited. Original TLS failure and passing rerun are retained.


## DEF-M3-C06 Test-project transitive dependency advisories

Severity: High. Status: Fixed in merged PR16, 14 October 2026; two non-author approvals recorded and main CI run 38579869423 passed. The combined-work review is recorded separately. PR17 now merges the complete increment; main run 38594557024 passes all four projects and dependency checks. NuGetAuditMode=all exposes System.Text.Json 8.0.0 through test configuration packages. Advisories: https://github.com/advisories/GHSA-8g4q-xg66-9fp4 and https://github.com/advisories/GHSA-hh2w-p6rv-4g7w. Database/performance projects explicitly resolve patched 8.0.5. Strict restore audits and repeated 29/25/19 correctness checks pass. Raw failure/fix logs: evidence/m3/combined-20261014. The earlier web-only scan did not cover this test-package graph.

## M4 checkpoint 18 October 2026

DEF-M3-C01/C02 remain fixed for the tested status path: all 19 M3 cases and the PostgreSQL suite pass after presentation changes. C04 remains open because normal non-Development startup is still blocked. C05 remains open: the presentation and rules are prepared, but real account/request adapters and original persistent journeys are not delivered by this contribution.

The AR01 optional-parameter false positive and its passing correction are retained in the [M4 execution evidence](evidence/m4-cameron-20261018/summary.md). No observed product status write was bypassing the service. Synthetic contract checks do not close the outstanding ownership, idempotency, note audit, performance, recovery or usability gaps.

## M4 final closure 18 October 2026

| Finding | Final disposition | Evidence |
| --- | --- | --- |
| DEF-M3-C01/C02 | Fixed and rerun after integration | Existing M3 regressions and P07/P10a/P12/P29 |
| DEF-M3-C04 | Normal controlled Staging startup guard corrected; public deployment remains a limitation | Program permits Development/Staging and refuses unapproved Production; no hosted environment is claimed |
| DEF-M3-C05 | Original presentation/storage journeys now integrated; outstanding NFR acceptance remains explicit | Persistent HTTPS suite and continuing RTM |
| DEF-M4-01/02 from Bianca’s snapshot | Superseded and corrected in current README | Configuration, real adapters and current counts replace the older snapshot’s claims |
| DEF-M4-03 | Writer wiring existed in M3; its residual illegal-status/ownerless-assignment paths are now guarded | P10/P10a/P10b and lifecycle matrix |
| DEF-M4-04/05 | Internal notes and author/time persistence delivered | P13–P15; RequestNoteEntity; restored notes |
| DEF-M4-06/07 | Reader note ordering and one timestamp per status transaction corrected | Ordered reader; writer uses one `now`; persistence/browser audit |
| DEF-M4-08 | Build warnings are blocking in the relevant CI steps | Current workflow; actual run recorded separately |
| DEF-M4-09 | Historical AI tool labels retained with their milestone provenance; M4 assistance explicitly added | Existing AI Usage Register; no personal review is invented |
| DEF-M4-10 | Root workflow moved and guarded; no one-review merge is enabled | Current workflow checks two current approvals and enforced protection; otherwise manual merge |
| DEF-M4-C01 | Fixed: priority sort names differed between form and query | P20 uses three priorities in both directions |
| DEF-M4-C02 | Fixed: saved scheduling/note audit and public status history were not projected | P15/P18; staff/requester browser views |
| DEF-M4-C03 | Fixed: full exception logging could disclose private database detail | P23 dummy-secret/private-content audit; custom log retains type only |
| DEF-M4-C04 | Fixed: manager sign-in landed on the requester/staff-only list | Persistent sign-in assertion now requires `/portal/management` |
| Preparatory session interference | Shared in-app cookies could invalidate an older sign-in form’s antiforgery token; authentication remained protected | Browser checks stopped and participant session handed back. This preparation incident is not a participant usability result |

No participant result, team approval or final upload is inferred from these agent checks. Open acceptance limitations are carried in the RTM and release decision.
