# Release Evidence Summary 15 October 2026

Current recommendation: **CONDITIONALLY READY for the agreed M3 increment in controlled simulated staging**. On 15 October 2026, Cameron confirmed that all three members agree with the current M3 work and plan to complete the outstanding original features during M4 finalisation. The protected read/status increment has passing verification, a separate performance exercise, simulated staging, completed peer review and successful combined main CI.

| Area | Evidence | Interpretation |
| --- | --- | --- |
| Published implementation | `668795a42d8191600e79ff4decad1642bd3e88b0`; [PR17](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/17), merged 14 October at 18:01:52 SAST | The 15 October submission edits change documentation only; application, tests and workflow match this published baseline |
| Functional verification | Cameron's personal 19 M3 cases, 29 regression checks and 25 database checks; combined remote run | Required case mix, negative paths, EP/BVA and selected high-priority risks are evidenced; broader acceptance remains limited |
| Performance | Michael PERF-01: 200/200 reads, 0 failures, 1.62 ms mean, 8.69 ms p95, 0.12 s, 1,696.16 operations/sec | Small database-read exercise; no separate database target; full NFR-002 pilot acceptance unverified |
| CI and gates | [Main run 38594557024](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024) succeeds on 668795a | All four projects build/run, including the 19 M3 cases, SDK analysis and dependency checks; the personal forbidden-edge probe demonstrates failure detection |
| Reviews | PR17 approvals from Bianca and Michael on 14 October; [completed conformance review](reviews/Bianca_M3_Final_Review_Updated.docx) | Two non-author approvals and actual combined-work findings are recorded. Returned GitHub review bodies were empty; no additional written review comments were available |
| Security | Patched System.Text.Json 8.0.5 in both affected test projects; combined audit checks pass | DEF-M3-C06 fixed in merged code; dated checks do not prove complete security |
| Simulated staging | E01/E02 pass in a Release HTTPS Staging test host using real PostgreSQL and synthetic identities | Credible simulation is allowed by M3. Normal web Program remains Development-only; real sign-in is unimplemented |
| Defects and acceptance | C01/C02 fixed, reviewed and merged; C04/C05 retain recorded limitations | C04 normal deployment is outside the accepted M3 simulation. C05 remains open in original scope for M4 finalisation; original RTM statuses are preserved |
| Recovery | Rollback approach documented | Timed restore and full NFR-007 acceptance are not claimed; M3 does not introduce a mandatory full recovery programme |

The agreed M3 scope is retrieval of existing requests with ownership/category access checks and authorised staff status changes through the approved lifecycle, including transactional history/public feedback, invalid-input rejection, rollback and stale-update handling. The demonstration uses prepared data, synthetic identities and the separate HTTPS Staging host. This team decision defines the M3 increment; it does not remove original requirements or establish full-product acceptance.

Residual risks include live permission freshness, simultaneous HTTP/audit timing, callable-writer and shared-project boundaries, environment differences and full pilot-load/recovery acceptance. R-047 and TD-04 remain closed only for the tested lifecycle and reviewed architecture. R-053's combined-CI omission is resolved by the successful run; merge-check enforcement remains a separate control.


Conditions: retain the controlled simulated environment and identify the demonstrated candidate; do not use this assessment as approval for real-user or production deployment. Real login, new-request submission, owner assignment, lists/search and reporting remain unfinished original requirements planned for M4 finalisation. Full pilot-load/recovery acceptance and residual permission/concurrency risks remain recorded. Any code or environment change needs the relevant checks before reusing this judgement.

Bianca's completed review dated 14 October remains a historical conformance record. This 15 October assessment incorporates the team scope agreement confirmed by Cameron.

## M4 checkpoint 18 October 2026

Current recommendation: **NO-GO for real-user production release**, pending trusted account/request adapters, complete original journeys and normal deployment verification. This is a checkpoint recommendation; team go-live approval remained outstanding at that checkpoint.

Cameron's screens/controllers and synthetic contract journey are prepared. Local checks pass: 73 read/rule checks, 25 database checks, 19 M3 cases and 30 UI/contract cases. Missing adapters return unavailable responses. The normal startup guard remains unchanged. PR20's successful remote run supports its own revision; it is not remote verification of these additional changes.

Before the final decision: identify the shared commit/tag/environment, connect and reverify the actual persistent journey, confirm important M3 issue outcomes, and update RTM/PED/registers. A staging no-go must name the remaining security/integrity/operational concern and remediation. M3 remains CONDITIONALLY READY only for its agreed controlled increment.

## M4 final closure 18 October 2026

Assessed project state: **staging, through a controlled local simulation**. Recommendation: **NO-GO for public production**. This is a technical conclusion from the recorded evidence, not a new team risk-acceptance signature.

Real stored accounts, submission, scoped lists/search, ownership, notes, schedule audit, lifecycle and management views now form one integrated product. Code baseline `625e4e99882150f79ad3e00bae327b14e63e5d9c`; release identifier `m4-final-20261018-r5` identifies the assessed repository version. Existing M3 CONDITIONALLY READY remains limited to its agreed protected read/status increment.

The persistent suite passes 36 checks and the local restore meets its recorded RPO/RTO targets. Public production remains blocked by the absence of a hosted TLS/certificate and protected operating configuration, the unexecuted full NFR-002 user-visible workload, unproven production recovery/rollback and remaining identity hardening. Deadline pressure is not treated as evidence that these risks are safe. No hosting account/environment is available, confirmed by Cameron on 18 October.

The participant fixture host uses synthetic accounts over loopback HTTP for usability only. HTTPS tests use a pinned generated certificate. Temporary test access is not a public deployment URL. Student review, two genuine current non-author approvals and actual Moodle submission remain distinct human evidence; they must be recorded separately.
