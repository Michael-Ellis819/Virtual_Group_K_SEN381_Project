# ADR-M2-01 / ADR-M2-02 later consequences

Consolidation by Cameron on 13 October 2026 for Bianca's review. Original ADRs remain in the retained PED. These linked entries provide implementation-stage consequences without replacing the accepted decisions.

| Decision | Observed implementation / problem | Decision retained and consequence | Evidence / remaining action |
| --- | --- | --- | --- |
| ADR-M2-01 single layered application | MVC delegates to application services; readers/writer use EF Core/PostgreSQL; required effects share a transaction. | Retain one deployable application. Integration is manageable; logical folders still allow coupling inside one project. | Source, build, E01/E02/A05. Bianca dependency review and final deployment still required. |
| ADR-M2-02 State lifecycle | Selected State implementation was absent in main. Minimal six-state dependency added to enable coordinated writes. Writer remains physical persistence component. | Retain State. One service decides legality; coordinator checks access, writer commits. State is not a database atomicity guarantee. | C01/C02/C04; E01/E02; A05. Michael validates ownership/implementation; Bianca reviews consequence and supported entry points. |

No reopening or scope/AC change is introduced. The implementation dependency was necessary for Cameron's integration; it is disclosed rather than attributed to Michael as pre-existing work. Review status: pending.
