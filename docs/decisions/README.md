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
