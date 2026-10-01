# CivicConnect Milestone 2 submission

Prepared for SEN381 Virtual Group K on 30 September 2026.

## Submission documents

1. `docs/PED_v2.0_FINAL.docx` — the reviewed Project Engineering Document containing the retained M1 baseline and consolidated M2 decisions.
2. `docs/CivicConnect_RTM_v2.0.xlsx` — the requirements traceability matrix containing the 27 requirements and 54 acceptance criteria.
3. `README.md` — purpose, implementation status, prerequisites, setup, interfaces, structure, testing and known limitations.

## Supporting evidence

- `docs/decisions/` — shared M2 decision index and ADRs.
- `docs/api-integration.md` — the established read interface and planned write integration.
- `docs/design.md` — component and interaction diagrams.
- `docs/traceability-demo.md` — the selected FR-003 requirement-to-implementation trace.
- `docs/github-evidence.md` — dated repository, pull-request and review evidence.
- `docs/evidence/local-verification.txt` — recorded result of 29 passing local checks.
- `docs/ai-usage.md` — AI assistance and human verification record.

## Implementation scope

The repository contains a protected request-details read path, central authorisation policy, filtered response DTO and repeatable verification harness. Login, PostgreSQL persistence, migrations, State implementation, write operations and production deployment remain planned and are not claimed as completed.
