# M4 integration verification 18 October 2026

Operator: OpenAI Codex. Codex ran these automated checks. Cameron's personal checks and stakeholder acceptance are recorded separately.

The isolated candidate starts from main `20fdf39f2f1769e0cb1abcd81fc2fbad4e4c8b28`, applies Michael's PR20 head `f5253d216f3c8f25c1b5e3e58cf473f9cf20a0ba`, and reconciles Bianca's supplied lifecycle source and checks. The five pending M3 documentation edits are preserved. The original M3 checkout is unchanged.

| Verification | Observed result | Scope |
| --- | --- | --- |
| Release builds | All three correctness projects built with zero warnings and errors | Cached package restore; no fresh dependency audit in this run |
| Verification harness | 73 passed, 0 failed | Existing 29 read/access checks plus Bianca's 44 lifecycle, ownership and summary checks |
| PostgreSQL suite | 25 passed | Migrations, persistence, concurrency and transactional rollback on a new disposable database |
| M3 API/system suite | 19 passed, 0 failed | Authorised lifecycle, rejection, antiforgery, stale changes, safe failure and saved requester feedback |

The first sandboxed loopback run failed with socket error 10013 after the 15 service checks. The subsequent permitted loopback run passed; the original failure is retained in `regression-sandbox-first-run.txt`. This was an execution restriction, not a failed application assertion.

Reconciliation retained the M3 coordinator, writer interface, action authorisation and antiforgery registrations. Its transition adapter now delegates the legal-edge matrix to Bianca's State classes. Her richer evaluator and ownership/summary calculators are registered without importing the stale replacement `Composition.cs` or `Program.cs`.

Owner eligibility and management counts are still pure rules. They have no persisted owner/due-date fields or application journeys. Existing New-to-Assigned status tests do not prove owner assignment. `ValidatedStatusChange` has not replaced the persistence command; the lower-level writer bypass risk remains open. No requirement is promoted to complete from this check total.

Michael's performance and backup/restore records retain their attribution and limitations. Performance was not repeated: the measured code is unchanged, and these correctness changes do not establish the user-visible NFR-002 workload. PR20's remote check supports PR20, not this additional local integration.

This is an integration checkpoint, not the final M4 release or go-live decision. Sign-in, submission, lists/search, assignment, management views, complete audit and original acceptance gaps remain to be finalised or accounted for.
