# M3 defect and finding register

Updated 14 October 2026 following the completed combined review and PR16 merge. Critical/High defects affecting confidentiality, integrity or essential agreed scope block release. A lower-severity deferral requires a named owner, impact and team acceptance. No accepted deferral is invented here.

| ID | Severity / impact | Status | Fix or next action / owner | Requirement / regression |
| --- | --- | --- | --- | --- |
| DEF-M3-C01 | High: persistence not reachable through an authorised status API. | Fixed and locally verified; combined-work review completed 14 October. | Coordinator, Staff action policy, State validation and POST contract; Cameron/Michael review. | FR-010; C03/C04/BB05/E01/E02. |
| DEF-M3-C02 | High: stale category permission could survive status-only comparison. | Fixed and locally verified; combined-work review completed 14 October. | Carry checked version and reject changed record; Cameron/Michael review. | NFR-001, R-050; A06. |
| DEF-M3-C03 | Medium test infrastructure: Windows TLS failed before HTTP cases. | Corrected. | Reimport temporary certificate for Schannel; Cameron. Original failure log retained. | integration-first-run.txt; final integration.txt passes 19. |
| DEF-M3-C04 | High release gap: web Program refuses Staging. | Open. | Michael must implement/review final deployment configuration and rerun critical journey. Harness Staging is partial evidence. | R-054; final environment startup/E01 pending. |
| DEF-M3-C05 | High scope gap: real login, requester submission, owner assignment and reporting not implemented. | Open. | Team must implement agreed core scope or formally approve a reduced M3 increment without rewriting historical requirements. | FR-001/004/006/007/009/013/014/016 etc.; RTM remains partial/planned. |

The failed gate probe is a deliberate verification experiment, not a discovered product defect. PERF-01 is evidenced. Full pilot-load/recovery acceptance and formal combined-branch approvals remain separately assessed in the release summary. The original failed TLS run and the passing rerun are both retained; the old error is not concealed.


## DEF-M3-C06 Test-project transitive dependency advisories

Severity: High. Status: Fixed in merged PR16, 14 October 2026; two non-author approvals recorded and main CI run 38579869423 passed. The combined-work review is recorded separately. NuGetAuditMode=all exposes System.Text.Json 8.0.0 through test configuration packages. Advisories: https://github.com/advisories/GHSA-8g4q-xg66-9fp4 and https://github.com/advisories/GHSA-hh2w-p6rv-4g7w. Database/performance projects explicitly resolve patched 8.0.5. Strict restore audits and repeated 29/25/19 correctness checks pass. Raw failure/fix logs: evidence/m3/combined-20261014. The earlier web-only scan did not cover this test-package graph.
