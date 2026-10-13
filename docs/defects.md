# M3 defect and finding register

Updated 13 October 2026 by Cameron's consolidation. Critical/High defects affecting confidentiality, integrity or essential agreed scope block release. A lower-severity deferral requires a named owner, impact and team acceptance. No accepted deferral is invented here.

| ID | Severity / impact | Status | Fix or next action / owner | Requirement / regression |
| --- | --- | --- | --- | --- |
| DEF-M3-C01 | High: persistence not reachable through an authorised status API. | Fixed locally; peer review pending. | Coordinator, Staff action policy, State validation and POST contract; Cameron/Michael review. | FR-010; C03/C04/BB05/E01/E02. |
| DEF-M3-C02 | High: stale category permission could survive status-only comparison. | Fixed locally; peer review pending. | Carry checked version and reject changed record; Cameron/Michael review. | NFR-001, R-050; A06. |
| DEF-M3-C03 | Medium test infrastructure: Windows TLS failed before HTTP cases. | Corrected. | Reimport temporary certificate for Schannel; Cameron. Original failure log retained. | integration-first-run.txt; final integration.txt passes 19. |
| DEF-M3-C04 | High release gap: web Program refuses Staging. | Open. | Michael must implement/review final deployment configuration and rerun critical journey. Harness Staging is partial evidence. | R-054; final environment startup/E01 pending. |
| DEF-M3-C05 | High scope gap: real login, requester submission, owner assignment and reporting not implemented. | Open. | Team must implement agreed core scope or formally approve a reduced M3 increment without rewriting historical requirements. | FR-001/004/006/007/009/013/014/016 etc.; RTM remains partial/planned. |

The failed gate probe is a deliberate verification experiment, not a discovered product defect. Missing performance/recovery evidence and new-branch approvals are release concerns in the release summary. The original failed TLS run and the passing rerun are both retained; the old error is not concealed.
