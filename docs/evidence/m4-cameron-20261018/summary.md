# Cameron M4 verification 18 October 2026

Operator: OpenAI Codex. All new executions below are agent checks, not student execution or stakeholder acceptance.

| Check | Result | Evidence boundary |
| --- | --- | --- |
| Release builds | Zero warnings/errors for four correctness projects | Cached restore; see build reports. This is not a fresh dependency audit. |
| Existing verification and Bianca rules | 73 passed | Read/access and pure lifecycle/ownership/summary rules |
| PostgreSQL suite | 25 passed | New disposable database; persistence, versions and status/history/feedback rollback |
| M3 component/API/system suite | 19 passed | Existing protected read/status increment, HTTPS, antiforgery, safe errors and stale changes |
| M4 UI/contract suite | 30 passed | Server-rendered forms, EP/BVA, role/category/privacy boundaries, conflict handling and a synthetic submit-to-close journey |
| Browser inspection | Submission and requester-details layouts inspected at 558px; no page overflow on submission | In-app browser, synthetic HTTP preview. Keyboard Tab/Enter used for sign-in and submission. Screenshots retained. |

The first expanded verification run failed AR01 because its text scan treated an optional record parameter `Status = null` as a mutation. The filter parameters were made explicit without weakening the guard; the subsequent 73-check run passed. `architecture-scan-first-run.txt` preserves the original failure.

The synthetic store and account fixtures are compiled only into `tests/CivicConnect.M4Tests`. They are not registered in the normal product. A successful contract journey does not establish real sign-in, durable creation/idempotency, simultaneous claims, complete PostgreSQL audit, deployed E2E, participant usability or pilot performance. Missing adapters produce 503 responses; management does not show invented zero counts.

The current RTM checkpoint awards 25.5/54 points (47.2%): 13 Met, 25 Partially Met and 16 Not Met. Each criterion has an explicit outcome/value and remaining gap. This is provisional evidence assessment while teammates continue; the 80% target is not met. No original commitments were removed or relabelled as approved deferrals.

Historical M3 personal/performance/peer-review records keep their dates and attribution. The M3 CONDITIONALLY READY decision remains limited to its agreed increment and controlled simulation. The M4 full-product and deployment decisions remain pending shared integration.

The PED package preserves the earlier milestone body except its current cover/control information and updated page furniture. Only document.xml, header1.xml, footer1.xml and settings.xml change; styles, images, relationships and other package parts are retained. The RTM keeps both original sheets, all prior values/formulas, merges, freeze panes and existing validations, adding M4 columns and calculation inputs.

The failing-gate helper was updated to target the integrated Lifecycle/RequestState.cs. The deliberately allowed New-to-Closed edge failed the component gate; source bytes were restored exactly, rebuilt, and all six component cases passed. The driver and probe reports retain both outcomes.
