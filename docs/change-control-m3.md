# CR-M3-C01 - controlled implementation and impact record

Date: 13 October 2026. Owner: Cameron Purdon. Status: implemented locally; independent team review pending. This is an engineering implementation record, not an invented lecturer/client scope-change request.

| Original decision | Implementation problem | Resulting implementation | Consequence |
| --- | --- | --- | --- |
| ADR-M2-05: central injected authorisation; read access does not permit writes. | Michael's writer persisted changes but the API had no authorised mutation use case. | Add CanChangeStatus and an application coordinator. Check Staff/category permission for HTTP and direct service callers. | One permission rule set; real account provisioning and freshness remain outstanding. |
| ADR-M2-06: explicit required-effect coordination and one transaction. | Status/history/feedback were available only through a low-level writer. | Add POST status contract, State validation, feedback bounds, antiforgery and HTTP conflict/failure mapping. Retain Michael's writer and migrations. | Controlled end-to-end status slice; persistence stays separate from legality and access. |
| ADR-M2-02: State encapsulates legal lifecycle transitions. | The selected service and classes were absent from supplied main. | Add minimal State/service integration dependency from the existing lifecycle; Michael reviews and owns continuation. | Six state classes add structure; reopening remains excluded. State validates; the writer physically saves the command. |
| Optimistic version token in Michael's implementation. | Checking current status alone could miss a category change between authorisation and saving. | Carry the observed version from snapshot to writer and compare it before saving. | Stale permission decisions cannot commit through this route; A06 verifies the boundary. |

Impact: FR-003/005/010/012/015/016 and NFR-001/003/008/009 gain partial implementation evidence. No requirement ID, wording, priority or acceptance criterion changes. Existing database migrations remain unchanged. GET output remains compatible; the snapshot's version is internal and is not added to the public DTO. Tests, API documentation, ADR consequences, RTM and risk records evolve together.

Validation: six component, five black-box, six API/integration and two system cases passed. Existing 29 verification and 25 database checks passed. A controlled forbidden-transition injection failed C02; the original source was restored. Human review and CI on the new branch remain pending.


## CR-M3-C02 Combined performance evidence and dependency audit

14 October 2026: integrate PR16 source/report into Cameron’s existing branch; preserve Michael’s second-run report and correct final PED/RTM figures. Add benchmark/strict transitive-audit CI steps. Fix DEF-M3-C06 using JSON 8.0.5 in database/performance projects. No requirement or acceptance-criterion change, schema migration or new production route. Preserve the human-run evidence and mark new local checks separately.
