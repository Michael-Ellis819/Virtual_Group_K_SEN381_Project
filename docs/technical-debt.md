# Continuing Technical Debt Register - M3

Bianca's supplied TD-01-TD-05 and Michael's PED TD-M3-01-TD-M3-04 retain their IDs. The entries below consolidate current evidence and ownership for review; they do not assert that either teammate approved the update.

| ID | Compromise and consequence | Priority / owner | Remediation and closure evidence | Status |
| --- | --- | --- | --- | --- |
| TD-01 | Layers share one project; folder boundaries can be crossed. | Medium / Bianca | Dependency review; strengthen boundaries when justified. | Open |
| TD-02 | Low-level writer can be called outside validated application use cases. | High / Michael/Cameron | Review all supported writes; require coordinator; prevent new bypasses. | Partial: current API covered |
| TD-03 | Custom executables lack standard discovery/reporting. | Medium / Michael/Cameron | Document exact commands; evidence all harnesses run in CI. Evaluate a standard framework later. | Partial: workflow updated |
| TD-04 | ADR consequence records need final team review. | Medium / Bianca | Review conformance updates against final implementation and tests. | Open |
| TD-05 | Readiness evidence may drift from release ID. | High / Bianca/Cameron | Freeze and link exact baseline, current CI, RTM, environment and approvals. | Partial: local baseline indexed |
| TD-M3-01 | Persistence initially separate from access/lifecycle. | High / Michael/Cameron | Review coordinator; run supported API journeys and independent reviews. | Integrated locally; review pending |
| TD-M3-02 | Audit history covers status only. | High / Michael | Add creation/ownership/priority/due-date/note events and restart/immutability checks. | Open |
| TD-M3-03 | Request submission lacks durable idempotency. | High / Michael | Implement creation and duplicate-submit safeguard; verify double-click behaviour. | Open |
| TD-M3-04 | Staging, backup and restore incomplete. | High / Michael | Verify actual deployed startup, migrations, rollback and backup/restore. | Open |
| TD-M3-C01 | Synthetic identity tickets used for verification; live permission freshness absent. | High / Michael | Implement trusted accounts/permissions and revocation/revalidation tests. | Open |

Preserved database tests exercise raw persistence writes to isolate the adapter. Their direct New -> In Progress assignment is not a legal application lifecycle example. Supported mutations use RequestStatusService. Adding another route requires the same policy and State controls.
