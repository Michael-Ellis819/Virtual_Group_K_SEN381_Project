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
