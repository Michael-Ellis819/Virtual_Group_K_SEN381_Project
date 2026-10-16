# CivicConnect Milestone 3 submission

Prepared for SEN381 Virtual Group K on 15 October 2026.

## Submission documents

1. [docs/PED_v3.0_Updated.pdf](docs/PED_v3.0_Updated.pdf) — the primary M3 submission document, continuing the retained M1/M2 history with implementation, verification and release evidence.
2. [docs/PED_v3.0.docx](docs/PED_v3.0.docx) — the editable Project Engineering Document.
3. [docs/CivicConnect_RTM_v2.0.xlsx](docs/CivicConnect_RTM_v2.0.xlsx) — the continuing requirements traceability matrix containing 27 requirements and 54 acceptance criteria, with M3 evidence.
4. [docs/PED_v3.0.md](docs/PED_v3.0.md) — the lecturer-requested Markdown copy of the PED, including tables and the component diagram.
5. [docs/CivicConnect_RTM_v2.0.md](docs/CivicConnect_RTM_v2.0.md) — the lecturer-requested Markdown copy of both workbook sheets and all traceability fields.
6. [README.md](README.md) — purpose, implementation status, prerequisites, setup, interfaces, structure, testing and known limitations.

## Supporting evidence

- [docs/decisions/](docs/decisions/README.md) — shared decision index and ADRs.
- [docs/api-integration.md](docs/api-integration.md) — protected read and authorised status-update contracts.
- [docs/design.md](docs/design.md) — component and interaction diagrams.
- [docs/traceability-demo.md](docs/traceability-demo.md) — the selected FR-003 requirement-to-implementation trace.
- [docs/github-evidence.md](docs/github-evidence.md) — dated repository, pull-request, review and CI evidence.
- [docs/testing-m3.md](docs/testing-m3.md) — test basis, techniques, expected/observed results and limitations.
- [docs/evidence/m3/personal-20261013-181846/](docs/evidence/m3/personal-20261013-181846/summary.md) — Cameron's recorded 29 regression checks, 25 database checks and 19 M3 cases.
- [docs/evidence/m3-performance-results.txt](docs/evidence/m3-performance-results.txt) — Michael's separate database-read performance result.
- [docs/reviews/Bianca_M3_Final_Review_Updated.docx](docs/reviews/Bianca_M3_Final_Review_Updated.docx) — completed combined-work conformance review, 14 October 2026.
- [docs/risk-register.md](docs/risk-register.md), [docs/technical-debt.md](docs/technical-debt.md) and [docs/defects.md](docs/defects.md) — current risks, compromises and findings.
- [docs/release-evidence.md](docs/release-evidence.md) — release-readiness assessment and residual risks.
- [docs/ai-usage.md](docs/ai-usage.md) — AI assistance and human verification record.
- [docs/submission-information.md](docs/submission-information.md) — candidate, repository, evidence and test/staging access information.

## Implementation scope

The repository contains protected persisted request-details reads, central authorisation, filtered response data, PostgreSQL migrations, State transition rules and authorised status updates with transactional history/feedback and concurrency checks. The harnesses demonstrate these paths, including two system journeys in credible simulated Staging.

The team confirmed the M3 protected read/status increment on 15 October 2026. Its release assessment is CONDITIONALLY READY for controlled simulated staging. Real sign-in, request submission, owner assignment, lists/search and reporting remain unfinished original requirements planned for M4 finalisation. Full pilot-load, recovery and production acceptance are not claimed. PR17 has merged with both teammate approvals and successful combined main CI. Submission date: 15 October 2026.
