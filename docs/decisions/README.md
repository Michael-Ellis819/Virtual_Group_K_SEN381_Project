# Shared decision log

This index preserves the M2 decision identifiers used in the reviewed PED v2.0. Bianca Grobler, Michael Ellis and Cameron Purdon accepted the documented baseline for submission on 30 September 2026.

| ID | Owner | Decision | Evidence and status |
| --- | --- | --- | --- |
| ADR-M2-01 | Bianca | Layered single application | PED M2 5.3; layered local slice now exists |
| ADR-M2-02 | Bianca | State for controlled status transitions | PED M2 5.6; State implementation still planned; A2 qualification recorded in consolidation corrections |
| ADR-M2-03 | Michael | PostgreSQL persistence | PED M2 5.4; schema/migrations/transaction spike planned |
| ADR-M2-04 | Michael | C# ASP.NET Core MVC, Razor, EF Core, PostgreSQL | PED M2 5.5; web slice runs, EF/provider/database versions still pending |
| ADR-M2-05 | Cameron | Central authorisation with DI | ADR-M2-05-authorisation.md; accepted, implemented and checked locally |
| ADR-M2-06 | Cameron | Same-application HTTP and explicit coordination | ADR-M2-06-integration.md; read boundary checked; future writes specified |
| ADR-M2-07 | Cameron | Repeatable checks and reviewed integration | ADR-M2-07-collaboration.md; local harness passed; peer review completed; remote CI evidence remains unavailable |

The M1 decision and operational material remains in Part I of the consolidated PED. Implementation limitations and deferred evidence remain recorded against the relevant decisions.


## Current M3 implementation update 14 October

The table above preserves the M2 checkpoint. Layered MVC, State lifecycle, PostgreSQL adapter/migrations and central action/API coordination now exist for the tested increment. PERF-01 uses the relational reader. Bianca’s completed 14 October review confirms scoped conformance closure for the reviewed architecture. Deployment qualifications remain separately recorded. Deployment/startup, future-route enforcement and identity freshness remain qualified.


## Final M3 evidence 15 October 2026

The M2 decision table above records the original baseline. Persistence, State, policy and API coordination now have integrated implementation evidence. PR17 merged at 668795a with Bianca and Michael approvals. [Main CI](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024) passes all four projects. The completed conformance review and PED Section 14 retain the actual deployment, identity and acceptance limitations.

## M4 final closure 18 October 2026

The final product connects stored accounts and permissions, record-scoped PostgreSQL queries, real MVC forms and transactional commands. Lifecycle legality remains in Bianca’s rules; authorisation decides who may act. Assignment saves one eligible owner with its status/history/feedback, while generic status routes cannot create ownerless assignment. The persistence boundary now rechecks legality, reducing the earlier writer-bypass debt.

Repeated local checks cover the persistent journey, races, injected rollback, safe errors and revocation. The custom harness remains an explicit CI responsibility. Hosted TLS and the full pilot performance measurements remain outstanding. Final teammate reviews must also be recorded before merge.
