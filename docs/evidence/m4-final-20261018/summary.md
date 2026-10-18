# M4 final verification

Date: 18 October 2026. Operator: OpenAI Codex. Code baseline: `625e4e99882150f79ad3e00bae327b14e63e5d9c`. Automated verification was run through Codex. Participant observations and teammate reviews are recorded separately.

| Suite | Result | Boundary |
| --- | --- | --- |
| Read and business rules | 73 passed | Central access, lifecycle, ownership and management calculations |
| PostgreSQL integration | 43 passed | Persistence, versions, stored accounts/stamps, transactional assignment, scheduling and duplicate protection |
| M3 regression cases | 19 passed | Component, API and controlled system paths. E01 now starts Assigned; first ownership must use the atomic assignment command |
| UI/contract checks | 30 passed | Synthetic adapters isolate presentation and unavailable-adapter behaviour |
| Persistent HTTPS portal | 36 passed | Real account hashing and PostgreSQL adapters; submit → claim → progress → resolve → close, notes/privacy, schedule audit, rollback, races, sorting, exact date bounds, restart and revocation |
| Local restore | PASS | Backup age 0.917 seconds; restoration and consistency verification 0.676 seconds; 24-hour RPO and 4-hour RTO targets met for the recorded small dataset |

The four existing suites and the persistent suite contain 201 passing checks in total. This count is a verification result, not an M4 quota or a scope-completion calculation. Correctness reports and recovery.json are retained here. Local cached builds disabled NuGet audit; remote scan/audit conclusions are recorded separately against their actual revision.

Browser preparation used the persistent fixture host: keyboard sign-in, submission, claim, internal note, priority/due date and closure were exercised with real stored requests. Screenshots identify synthetic data. Chrome sign-in at 360/1366 had matching viewport/scroll widths. The complete Edge matrix is not evidenced. Browser checks were completed separately from participant sessions after a shared-cookie problem during preparation.

The participant preview is loopback HTTP with temporary accounts in a separate disposable database. The automated persistent suite uses HTTPS and pins its generated test certificate. Neither establishes a public production deployment or a publicly trusted deployed certificate. NFR-002's full user-visible performance workload is not executed by the database-read exercise.

Earlier M3 records and the earlier 47.2% M4 checkpoint remain dated history. The current assessment uses the same 27 requirements and 54 original criteria: 48.5/54 = 89.8%. Per-criterion results and limitations remain in the continuing RTM. No original commitment was removed to improve the denominator.

Cameron observed five participants completing the same task; the results are recorded in [usability.md](usability.md). Three finished without coaching, below the required four. AC-NFR-004-1 is Not Met; AC-NFR-004-2 is Met because the same task, completion times, errors and coaching were recorded.

Document checks: the final PDF has 85 pages; M4 begins on page 79 and its submission-information block is on page 85. Retained M3 page text matches apart from the edited cover and updated version/footer labels. DOCX package comparison retains 1,614 historical body blocks, with declared cover/document-control exceptions, and changes only document/header/footer/settings parts. RTM original Requirements content and A:V values/styles, sheets, merged ranges and freeze panes are preserved. Its outcome-mutation and missing-score checks pass. The original five uncommitted M3 files match their saved backup byte for byte. Word exported the PDF because the bundled renderer’s LibreOffice executable was unavailable; all pages were rasterised and visually checked.

Team review: Cameron Purdon, Michael Ellis and Bianca Grobler reviewed the combined final product, PED and RTM and agreed on a group call to submit on 18 October 2026. Cameron confirmed this agreement. Formal PR reviews are recorded separately on GitHub.
