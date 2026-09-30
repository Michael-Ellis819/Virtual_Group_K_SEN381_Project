# Cameron traceability demonstration

FR-003 is the selected M2 requirement-to-implementation trace. The current evidence demonstrates the protected read path; durable database acceptance remains pending.

1. **Requirement:** Requirements sheet, FR-003 and AC-FR-003-1/-2. The requester sees their own status and cannot reveal another requester's record. NFR-001 is the access-isolation driver; NFR-009 requires repeatable controlled-data checks.
2. **Architecture:** ADR-M2-01 allocates HTTP handling, application logic and data access within one application. Open RequestsController and RequestQueryService.
3. **Data decision:** ADR-M2-03 gives ServiceRequest a RequesterId and RequestNote a visibility flag. The development snapshot models these facts. The PostgreSQL adapter is still planned; do not call the fixture a working database.
4. **Design:** ADR-M2-05 selects a central policy behind IRequestAccessPolicy; A2 Task 1 Problem 2 provides the alternatives. Open RequestAccessPolicy to show requester ownership, staff categories and default denial. Explain why three Strategy implementations were unnecessary here.
5. **Interface and technology:** ADR-M2-06 defines GET /requests/{id}; ADR-M2-04 selects MVC/C#. Show UUID validation, the trusted principal and the filtered response DTO. State transitions remain Bianca's responsibility.
6. **Application evidence:** Run the verification executable. S01/S03 and H04/H07 demonstrate own-status reads and rejection of another owner. H05 checks the actual response has no private note. S07 demonstrates direct service calls cannot bypass the check.
7. **Limit:** RTM status is In Development. Live login, database persistence and an authorised update followed by a persisted read are not implemented, so AC-FR-003-1 is only partially evidenced.

## Short defence notes

- The selected approach is a central policy with dependency injection rather than separate Strategy classes for the current three-role model.
- Authentication establishes identity; the policy checks access to this record; DTO projection controls which notes leave the service.
- Explicit write coordination is selected over Observer because the small current workflow requires state/history/feedback to succeed together. Those writes are still forward work.
- All 27 requirement IDs and 54 acceptance criteria are preserved. Design/data/technology links have evolved; initial tests are narrower than complete verification.
- A changed permission rule requires changes to the ADR, policy, tests, interface notes, RTM and relevant risks. A passing local check supports only the implemented read slice.
