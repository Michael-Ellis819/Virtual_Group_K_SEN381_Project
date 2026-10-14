# Continuing Technical Debt Register - M3

Bianca's supplied TD-01-TD-05 and Michael's PED TD-M3-01-TD-M3-04 retain their IDs. The entries below include Bianca’s completed 14 October conformance review and its scoped closure decisions. Teammate review is recorded separately from GitHub approvals.

| ID | Compromise and consequence | Priority / owner | Remediation and closure evidence | Status |
| --- | --- | --- | --- | --- |
| TD-01 | Layers share one project; folder boundaries can be crossed. | Medium / Bianca | Dependency review; strengthen boundaries when justified. | Open |
| TD-02 | Low-level writer can be called outside validated application use cases. | High / Michael/Cameron | Review all supported writes; require coordinator; prevent new bypasses. | Partial: current API covered |
| TD-03 | Custom executables lack standard discovery/reporting. | Medium / Michael/Cameron | Document exact commands; evidence all harnesses run in CI. Evaluate a standard framework later. | Partial: workflow updated |
| TD-04 | Architecture consequence review. | Medium / Bianca | Bianca’s completed 14 October review covers layered, State and policy/API consequences; deployment caveat remains under R-054/DEF-M3-C04. | Closed for reviewed architecture |
| TD-05 | Readiness evidence may drift from release ID. | High / Bianca/Cameron | Freeze and link exact baseline, current CI, RTM, environment and approvals. | Partial: local baseline indexed |
| TD-M3-01 | Persistence initially separate from access/lifecycle. | High / Michael/Cameron | Review coordinator; run supported API journeys and independent reviews. | Integrated and reviewed for tested increment |
| TD-M3-02 | Audit history covers status only. | High / Michael | Add creation/ownership/priority/due-date/note events and restart/immutability checks. | Open |
| TD-M3-03 | Request submission lacks durable idempotency. | High / Michael | Implement creation and duplicate-submit safeguard; verify double-click behaviour. | Open |
| TD-M3-04 | Staging, backup and restore incomplete. | High / Michael | Verify actual deployed startup, migrations, rollback and backup/restore. | Open |
| TD-M3-C01 | Synthetic identity tickets used for verification; live permission freshness absent. | High / Michael | Implement trusted accounts/permissions and revocation/revalidation tests. | Open |

Preserved database tests exercise raw persistence writes to isolate the adapter. Their direct New -> In Progress assignment is not a legal application lifecycle example. Supported mutations use RequestStatusService. Adding another route requires the same policy and State controls.


## 14 October review update

TD-04 is closed for the reviewed architecture consequences; deployment qualifications remain. TD-03 has explicit steps for four projects; PR16’s successful newer CI covers older suites/performance/test audits, while combined M3 remote coverage is not established. PERF-01 does not establish full pilot performance or recovery. DEF-M3-C06 remains fixed locally. Completed review: reviews/Bianca_M3_Final_Review_Updated.docx.
