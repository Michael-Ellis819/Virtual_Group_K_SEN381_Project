# CR-M3-C01 - controlled implementation and impact record

Date: 13 October 2026. Owner: Cameron Purdon. Status: implemented, reviewed by Bianca and Michael on 14 October, merged through PR17 and verified by combined main CI. This is an engineering implementation record, not an invented lecturer/client scope-change request.

| Original decision | Implementation problem | Resulting implementation | Consequence |
| --- | --- | --- | --- |
| ADR-M2-05: central injected authorisation; read access does not permit writes. | Michael's writer persisted changes but the API had no authorised mutation use case. | Add CanChangeStatus and an application coordinator. Check Staff/category permission for HTTP and direct service callers. | One permission rule set; real account provisioning and freshness remain outstanding. |
| ADR-M2-06: explicit required-effect coordination and one transaction. | Status/history/feedback were available only through a low-level writer. | Add POST status contract, State validation, feedback bounds, antiforgery and HTTP conflict/failure mapping. Retain Michael's writer and migrations. | Controlled end-to-end status slice; persistence stays separate from legality and access. |
| ADR-M2-02: State encapsulates legal lifecycle transitions. | The selected service and classes were absent from supplied main. | Add minimal State/service integration dependency from the existing lifecycle; Michael reviews and owns continuation. | Six state classes add structure; reopening remains excluded. State validates; the writer physically saves the command. |
| Optimistic version token in Michael's implementation. | Checking current status alone could miss a category change between authorisation and saving. | Carry the observed version from snapshot to writer and compare it before saving. | Stale permission decisions cannot commit through this route; A06 verifies the boundary. |

Impact: FR-003/005/010/012/015/016 and NFR-001/003/008/009 gain partial implementation evidence. No requirement ID, wording, priority or acceptance criterion changes. Existing database migrations remain unchanged. GET output remains compatible; the snapshot's version is internal and is not added to the public DTO. Tests, API documentation, ADR consequences, RTM and risk records evolve together.

Validation: six component, five black-box, six API/integration and two system cases passed. Existing 29 verification and 25 database checks passed. A controlled forbidden-transition injection failed C02; the original source was restored. PR17 has both teammate approvals and is merged; main CI 38594557024 passes on 668795a.


## CR-M3-C02 Combined performance evidence and dependency audit

14 October 2026: integrate PR16 source/report into Cameron’s existing branch; preserve Michael’s second-run report and correct final PED/RTM figures. Add benchmark/strict transitive-audit CI steps. Fix DEF-M3-C06 using JSON 8.0.5 in database/performance projects. No requirement or acceptance-criterion change, schema migration or new production route. Preserve the human-run evidence and mark new local checks separately.


## CR-M3-C03 M3 increment and release decision 15 October 2026

Cameron confirmed that the team agrees with the current M3 increment and will complete unfinished original features during M4 finalisation. The M3 candidate is published implementation 668795a: protected retrieval of existing requests and authorised lifecycle/status updates with transactional history/public feedback, input/access checks, rollback and stale-update handling. Its verification uses controlled HTTPS simulated staging, real PostgreSQL, prepared records and synthetic identities.

Decision: CONDITIONALLY READY for this M3 increment in that environment, supported by PR17 approvals, passing combined CI, functional/system evidence and the separate performance exercise. Normal deployment, real login, submission, assignment, lists/search and reporting remain incomplete. C04 remains open for normal deployment and C05 remains open in original scope. The team plans their finalisation in M4; no M1 requirement, priority or acceptance criterion is removed or rewritten. Real-user/production release and full product acceptance are not approved. Team agreement was confirmed by Cameron on 15 October.
