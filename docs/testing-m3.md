# M3 test basis and execution

Owner and current executor: Cameron Purdon 601963. Personal verification completed on 13 October 2026, 18:18-18:19 SAST, against revision f774bf851e6b0c8d69cd9b37f6849fb0d853d661. [Execution metadata](evidence/m3/personal-20261013-181846/execution.json) records the operator, environment, revision and result. Earlier AI-run evidence is retained as historical provenance. Tests were derived from the unchanged PED requirements, the M2 lifecycle, the current API contract and Bianca's TC-ST specifications.

Risk priority: authorisation (R-049), invalid lifecycle changes (R-046/047), partial writes (R-048), conflicts (R-050) and misleading evidence (R-052/056). These receive stronger verification because they affect confidentiality and record integrity.

## Environment and count

Windows; .NET SDK 8.0.425; runtime 8.0.31; Release builds; PostgreSQL 18.4 in an isolated loopback cluster/database. The M3 harness uses Staging configuration, a temporary HTTPS certificate and synthetic protected test tickets. CI declares PostgreSQL 17; cross-version parity is not assumed.

The new suite contains **19 distinct cases: 6 component + 5 black-box + 6 API/integration + 2 system**. BB cases also cross the API/database boundary, but they are counted once overall. At least three negative cases and five high-priority requirement/risk cases are directly evidenced. The old harnesses add 29 and 25 assertion checks; they are not automatically counted as 54 additional independent M3 cases.

BVA uses 1, 2000 and 2001 code units around the nonblank 1-2000 feedback contract. The empty boundary is checked in BB04. EP separates valid/invalid feedback/status groups and permitted/requester/wrong-category identities. State testing separately covers allowed/prohibited lifecycle edges. Inputs were selected from the contract before checking the outcome.

## Case records

All cases below use fresh synthetic records where applicable. Each row's final result is tied to its ID in [integration.txt](evidence/m3/personal-20261013-181846/integration.txt). Source: [M3 harness](../tests/CivicConnect.M3Tests/Program.cs).

| ID | Level / technique | Basis | Input / precondition | Expected result | Actual result | Status |
| --- | --- | --- | --- | --- | --- | --- |
| C01 | Component; State transition | FR-010; TC-ST-01/03 | Five allowed lifecycle edges | Each edge accepted | All five accepted | Pass |
| C02 | Component; State transition | FR-010; TC-ST-02/05 | All 36 state pairs plus unknown states | Exactly five allowed; 31 denied; unknown denied | Matrix matched | Pass |
| C03 | Component; negative permission | NFR-001; TC-ST-06 | Own Requester attempts New -> Assigned | NotFound and zero writer calls | Denied; zero writes | Pass |
| C04 | Component; negative state | FR-010; TC-ST-08/10/11 | Staff asks New -> Closed | Invalid; zero writer calls | Rejected; zero writes | Pass |
| C05 | Component; injected conflict | R-050; TC-ST-14 partial | Writer throws concurrency exception | Conflict returned | Conflict returned | Pass |
| C06 | Component; coordination | FR-010; TC-ST-07/09 | Valid Staff command with version 1 | Writer receives actor/version/status/feedback | Command matched | Pass |
| BB01 | Black-box; BVA | API feedback bound; FR-010 | Authorised change, feedback length 1 | 204; feedback saved | 204; saved | Pass |
| BB02 | Black-box; BVA | API feedback bound | Authorised change, feedback length 2000 | 204; full feedback saved | 204; saved | Pass |
| BB03 | Black-box; BVA negative | API feedback bound; NFR-003 | Feedback length 2001 | 400; original status/version/notes/history | 400; unchanged | Pass |
| BB04 | Black-box; EP negative | API contract; TC-ST-04/05 | Empty/whitespace feedback; unknown/null target | 400 for each; no writes | All 400; unchanged | Pass |
| BB05 | Black-box; EP negative | NFR-001; TC-ST-06 | Requester and wrong-category Staff mutations | 404 for each; no writes | Both 404; unchanged | Pass |
| A01 | API; antiforgery negative | Cookie mutation security contract | Valid Staff cookie but no CSRF token | 400; no writes | 400; unchanged | Pass |
| A02 | API; authentication negative | FR-016 AC2 partial | Signed-out mutation | 401; no writes | 401; unchanged | Pass |
| A03 | API; stale state | R-050; FR-010 | Stored New; client expects Assigned | 409; no writes | 409; unchanged | Pass |
| A04 | API; disclosure negative | NFR-001; FR-003 AC2 | Absent record vs inaccessible record | Same 404 status and public title | Matched | Pass |
| A05 | API/database; controlled failure | NFR-003 AC1 status part; NFR-008 AC2; TC-ST-13 | Valid change; temporary DB constraint prevents save | Generic 500; no status/history/feedback saved | 500; full rollback | Pass |
| A06 | Persistence integration; stale permission | R-049/050; NFR-001 | Category changes after checked version 1 | Reject stale version; retain category/version 2; no status effects | Rejected; no effects | Pass |
| E01 | System; lifecycle journey | FR-003 AC1; FR-010 AC1; FR-012; FR-015 status part | Staff progresses New to Closed; requester reads through HTTP | 204 per change; Closed/time/public notes; four correct audit edges and actors/times; version 5 | Journey and audit matched | Pass |
| E02 | System; rejection and isolation | FR-003 AC2; FR-005 partial; FR-010 | Missing reason rejected; Staff rejects with reason; requester reads; reopening and other owner try access | 400 blank reason; 204 rejection; 400 reopen; own reason visible; 404 foreign read; one history entry | Journey and isolation matched | Pass |

## Two system journeys and interpretation

E01 starts from a seeded New request. Authorised Staff moves it through Assigned, In Progress, Resolved and Closed using HTTP, valid antiforgery and public feedback. The owner reads it over HTTP; a fresh database context checks the four history records, actor/times and version. This spans MVC, authentication, access policy, State logic, writer and PostgreSQL.

E02 checks a rejection journey: reject a missing reason, accept a reason, read the own rejected request, reject reopening and deny another requester. It verifies both useful feedback and isolation.

These are system tests of the integrated status-management API slice. Synthetic tickets establish identity only for the tests; no claim is made for real login, new-request submission, owner assignment, reporting, browser usability or persistence after a complete process/database restart. Those acceptance gaps remain in the RTM. E01/E02 provide the recorded critical journeys in the credible simulated Staging host. A separate externally hosted deployment is not claimed. A changed target/configuration requires its own verification.

## Regression, static and security evidence

- [regression.txt](evidence/m3/personal-20261013-181846/regression.txt): 29 existing service/HTTP checks passed.
- [database.txt](evidence/m3/personal-20261013-181846/database.txt): Michael's 25 database checks passed, including competing contexts and transaction rollback.
- [static-analysis.txt](evidence/m3/personal-20261013-181846/static-analysis.txt): SDK code analysis/build completed with zero warnings/errors. This is the enabled analyzer set, not proof of zero defects or measured code coverage.
- [dependencies.json](evidence/m3/personal-20261013-181846/dependencies.json): NuGet advisory query reported no vulnerable packages on this date. [Dependency gate](../tools/check_dependencies.py) rejects reported high/critical advisories and incomplete scans. This does not establish future dependency safety or a full security audit.
- [gate-negative-control.txt](evidence/m3/personal-20261013-181846/gate-negative-control.txt): deliberately permitting New -> Closed caused C02 to fail and the executable to exit nonzero. Original source restored, rebuilt and 19 cases rerun successfully. The CI job stops on such a failure; requiring that job for merge must be confirmed in repository protection settings.

Bianca TC-ST coverage: 01/02/03 -> C01/C02/E01; 04/05 -> BB04; 06/07 -> C03/BB05/C06/E01; 08/10/11 -> C04/BB03/BB04; 09 -> E01/E02; 12 -> direct service C04 and the supported HTTP route (E02 terminal-state denial), with broader internal writer bypass retained as debt; 13 -> A05; 14 -> C05/A03/A06 and Michael's competing-context checks. A simultaneous pair of HTTP updates with audit inspection remains additional verification work.

Performance: Michael's separate PERF-01 exercise is recorded below and was merged through PR16. Cameron's personal correctness run is separate from this performance report. NFR-002's unchanged target is 95% within 3 seconds, at most 1% failures, 10 concurrent users/1,000 requests/10 minutes after 2 minutes warm-up, at least 100 runs per operation. A smaller justified M3 load exercise may satisfy the milestone evidence requirement while leaving NFR-002 partially verified.


## Separate performance exercise PERF-01

Michael’s second run: 200 direct reader operations, maximum concurrency 10; 1.62 ms mean, 8.69 ms p95, 0 failures, 0.12 seconds. Raw evidence and environment confirmation: evidence/m3-performance-results.txt and evidence/m3-performance-run-confirmation.md. No database target; full NFR-002 acceptance remains unverified. The performance project requires a migrated disposable database and uses environment/User Secrets configuration. The original report is retained independently of dependency-fix follow-up checks.


## Combined remote verification indexed 15 October 2026

[Main run 38594557024](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024) passes on 668795a42d8191600e79ff4decad1642bd3e88b0 after PR17 merged on 14 October. It builds and runs all four projects, including the 19 M3 cases, with analysis and dependency checks. This is additional remote evidence; Cameron's 13 October personal execution and Michael's original 14 October performance measurement keep their original attribution and values.

## M4 integration checkpoint 18 October 2026

The isolated M4 candidate combines PR20 with Bianca's State classes, ownership/summary rules and 44 checks. The existing status coordinator now uses her lifecycle matrix while retaining its action permission, feedback, antiforgery and transaction contracts. Codex executed 73 verification checks, 25 database checks and 19 M3 cases successfully on a new disposable database. These are agent results, not student execution or complete product acceptance. See [integration summary](evidence/m4-integration-20261018/summary.md) for the source inputs, raw reports, initial sandbox restriction and remaining gaps. The historical M3 results and conditional-readiness decision remain unchanged.

## M4 presentation and finalisation checks 18 October 2026

Cameron's new `CivicConnect.M4Tests` executable exercises the real MVC/form pipeline with synthetic test-only account/request adapters. Thirty checks cover sign-in and role spoofing, submission EP/BVA, inclusive date bounds, access denial, assignment-rule orchestration, internal notes/encoding, stale writes, UTC binding, management counts and the submit → assign → progress → resolve → close contract journey. The normal product still needs real adapters. Full deployed E2E and NFR acceptance are not inferred.

Run `dotnet run --project tests/CivicConnect.M4Tests --configuration Release`. CI now calls this harness as well as the previous projects. After these changes, 73 existing/rule checks, 25 database checks, 19 M3 cases and all 30 UI/contract checks passed locally. [M4 summary and limitations](evidence/m4-cameron-20261018/summary.md) index the raw reports. Re-run the affected journeys against the actual final persistence/deployment before accepting the shared product.
