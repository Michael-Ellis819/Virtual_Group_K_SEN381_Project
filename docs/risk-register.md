# Continuing Risk Register - M3 update

Bianca remains the register owner. Cameron consolidated the supplied proposals on 13 October 2026 for review. R-001-R-045 remain intact in the retained PED Risk Register; this update does not replace their history or close them. Michael's M3-R01-M3-R05 remain in retained PED 7.4 and map to the related entries below. Owner assignments follow the supplied work allocation; closure/acceptance requires actual team review.

| ID / earlier links | Risk | Likelihood / impact | Current evidence and remaining action | Owner / status |
| --- | --- | --- | --- | --- |
| R-046; R-011/027; M3-R01 | Lifecycle bypass. | Medium / High | Supported API uses coordinator; C04/E02 reject invalid changes. Low-level writer still callable; inspect all future entry points. | Michael/Cameron; partial mitigation. |
| R-047; R-026 | Incorrect transition matrix. | Medium / High | C01/C02 match five allowed and 31 prohibited edges. Michael/Bianca must review lifecycle implementation. | Michael/Bianca; partial mitigation. |
| R-048; R-015; M3-R01 | Status and audit inconsistency. | Medium / High | E01 and A05 plus Michael rollback checks pass. Audit events beyond status remain incomplete. | Michael; partial mitigation. |
| R-049; R-004/013/017 | Inconsistent resource authorisation. | Medium / High | BB05/A02/A04/A06 deny unsafe writes/reads. Real account permissions/revalidation are not proven. | Cameron/Michael; partial mitigation. |
| R-050; R-016; M3-R02 | Lost concurrent updates. | Medium / High | Version/status comparison and competing-context checks pass; add simultaneous HTTP/audit scenario. | Michael/Cameron; partial mitigation. |
| R-051; R-008/009 | Layer coupling. | Medium / Medium | Controller delegates decisions; one project does not enforce folder boundaries. Peer dependency review needed. | Bianca; Open. |
| R-052; R-032 | Green suite misses important behaviour. | Medium / High | 19 cases span levels; submission/reporting/login remain outside tested slice. Extend by risk. | Team; Open. |
| R-053; R-038/044 | Custom harness omitted from CI. | Medium / Medium | Workflow now calls all three harnesses; new-branch remote run and required-check settings pending. | Michael/Cameron; Open. |
| R-054; R-021/040; M3-R03 | Target environment fails to start. | Medium / High | Harness Staging works; Program still denies Staging. Configure and verify final deployed candidate. | Michael; Open. |
| R-055; R-045 | Defects lack release severity control. | Medium / High | Defect policy and NOT READY reasoning recorded; team release review pending. | Bianca; Open. |
| R-056; R-045 | Unsupported acceptance claims. | Medium / High | Existing RTM extended with actual results and explicit gaps; no whole requirement promoted prematurely. | Cameron; partial mitigation. |
| R-057; R-018/025; M3-R05 | Pilot performance unverified. | Medium / Medium | Separate workload evidence absent. Run/report justified scenario; do not claim NFR-002 met. | Michael; Open. |
| R-058; R-012/038 | Documentation differs from final product. | Medium / Medium | Current M3 index distinguishes retained historical statements; final freeze/review remains pending. | Cameron/Bianca; Open. |

M3-R04 continues secrets risk R-005/024: connection configuration uses environment variables; no production credential is introduced. M3-R05 also links NFR-007 recovery: backup/restore evidence remains pending. Residual risk and blockers are in [release-evidence.md](release-evidence.md).
