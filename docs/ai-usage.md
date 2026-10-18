# AI usage record

Virtual Group K: Michael Ellis 600332 · Cameron Purdon 601963 · Bianca Grobler 600537

Updated 14 October 2026. Each member is responsible for checking, understanding and maintaining the AI-assisted work they accept.

## Milestone 1

The following entries continue the AI Research and Verification Record retained in the PED. All three members used ChatGPT.

| Member | Purpose and output used | Human verification and changes |
| --- | --- | --- |
| Michael Ellis 600332 | Identify a way to track task responsibility and evidence. The GitHub Issues suggestion was used. | Checked against project requirements. Other tracking suggestions were rejected in favour of clear ownership and traceable evidence. |
| Cameron Purdon 601963 | Understand the work, break it into activities and improve explanatory structure. Selected information and formatting were used. | Compared with online sources and lecture notes. Wordy or irrelevant material was rejected. |
| Bianca Grobler 600537 | Create risk-register tables. The table structures were used. | Checked against project requirements. Overly complex or unnecessary tables were rejected. |

## Milestone 2

Records continued from the PED's AI usage section and the 30 September 2026 baseline.

### Michael Ellis 600332

- **Tool and purpose:** ChatGPT supported data, persistence, technology-stack and deployment decisions, comparison of alternatives and the structure of Michael's PED contribution.
- **Output used:** Revised explanations and documentation.
- **Human verification:** Compared with the M2 brief, approved requirements, Assignment 2 research and agreed team decisions; reviewed the consolidated PED.
- **Changed or rejected:** Unsupported implementation claims, unsuitable technology suggestions and explanations inconsistent with scope, cost or schedule.

### Cameron Purdon 601963

- **Tool and purpose:** OpenAI Codex supported repository/document inspection, ADRs, design and API integration, the protected read implementation, verification checks and PED/RTM consolidation. The PED also records ChatGPT assistance with research and explanations.
- **Output used:** Revised prose, C# code, verification cases and document formatting.
- **Human verification:** Built with zero warnings/errors and ran 29 checks. Compared requirements, acceptance criteria and RTM identifiers with the M1 baseline. Bianca and Michael reviewed the consolidated M2 content.
- **Changed or rejected:** Incorrect, unclear, wordy and unsupported explanations; implementation claims were separated from work planned at the M2 checkpoint.

### Bianca Grobler 600537

- **Tool and purpose:** ChatGPT supported understanding architecture requirements, organising alternatives, explaining architecture/design patterns and improving her PED contribution.
- **Output used:** Revised architecture, quality-driver, pattern and baseline documentation.
- **Human verification:** Compared with the M2 brief, approved requirements, Assignment 2 research and team decisions; reviewed the consolidated PED.
- **Changed or rejected:** Unsupported claims, overly complicated alternatives and explanations that did not match CivicConnect's scope.

## Milestone 3

### Michael Ellis 600332

| Field | Record |
| --- | --- |
| Recorded date | 12 October 2026; performance execution evidence dated 14 October. |
| Tool | ChatGPT (OpenAI). |
| Purpose and output used | C#/EF Core guidance, PostgreSQL integration, troubleshooting, Git support, testing approaches and technical documentation for persistence. |
| Human verification | Michael's supplied record reports local review and database checks, followed by controlled Git commits. His separate performance report records 200 successful reads and zero failures; environment/results were confirmed on 14 October. |
| Review and limitations | AI suggestions required review and testing. Full integration/acceptance was pending at the original 12 October checkpoint. The later performance exercise demonstrates short database-read behaviour, not full NFR-002 acceptance. |
| Responsibility | Michael remains accountable for validating and maintaining his submitted contribution. |

### Cameron Purdon 601963

| Field | Record |
| --- | --- |
| Recorded dates | 13-14 October 2026. |
| Tool | OpenAI Codex. |
| Purpose and output used | Inspect the handover, scope and team work; implement access/API coordination and the missing State dependency; create executable cases and CI additions; consolidate the PED, RTM and supporting records. |
| Human verification | Cameron personally ran Release builds, SDK analysis, 19 M3 cases, 29 regression checks, 25 database checks, a dependency query/gate and a controlled failing gate with source restoration on 13 October, 18:18-18:19 SAST. Revision: f774bf851e6b0c8d69cd9b37f6849fb0d853d661. |
| Changed or corrected | Windows test-certificate handling, null-target and stale-category/version coverage, unsupported full-release claims, and inconsistent performance figures. A later transitive audit found vulnerable System.Text.Json 8.0.0; affected test projects were patched to 8.0.5. |
| Later assistance | Codex helped record the user-confirmed 14 October combined review, preserve document formatting, incorporate merged PR16, reconcile evidence, export the PED PDF, restore the preferred README/START_HERE structure, consolidate this register and prepare the lecturer-requested PED/RTM Markdown copies. |
| Responsibility | Cameron remains accountable for reviewing and understanding the accepted implementation, tests and documentation. |

### Bianca Grobler 600537

| Field | Record |
| --- | --- |
| Source | Bianca's M3 entry in PED Section 12.1. |
| Tool | ChatGPT (OpenAI). |
| Purpose and output used | Format tables in her M3 documentation using her own data. |
| Human verification | Bianca read through the formatted tables to ensure AI had not changed her data. |
| Responsibility | Bianca remains accountable for the table content and engineering findings. AI formatting is separate from her completed review of the combined work on 14 October. |

## Verification and evidence

Human execution reports retain their original operators and dates. Codex's follow-up checks are identified separately; they are not relabelled as student execution. AI assistance does not provide independent teammate approval, risk acceptance or personal presentation participation.

- [PED](PED_v3.0.docx): retained M1 record, Section 12 M2 entries and Section 12.1 M3 entries.
- [Cameron's personal verification](evidence/m3/personal-20261013-181846/summary.md) and [execution metadata](evidence/m3/personal-20261013-181846/execution.json).
- [Michael's performance report](evidence/m3-performance-results.txt) and [run confirmation](evidence/m3-performance-run-confirmation.md).
- [Codex follow-up verification](evidence/m3/combined-20261014/summary.md): post-fix builds, correctness suites and strict dependency audits.
- [Bianca's completed conformance review](reviews/Bianca_M3_Final_Review_Updated.docx).
- [GitHub review/CI evidence](github-evidence.md).

## Milestone 4 integration checkpoint 18 October 2026

OpenAI Codex inspected the supplied handover, M4 brief, repository, PED, RTM, Bianca's ZIP and Michael's PR20. It backed up the five pending M3 documentation edits and prepared a separate M4 branch, preserving the original checkout. It reconciled overlapping lifecycle implementations, retained the established status/antiforgery wiring, corrected the stale CSRF observation and executed the combined 73-check harness, 25 database checks and 19 M3 cases. [Execution evidence](evidence/m4-integration-20261018/summary.md) identifies Codex as the operator. Unsupported completion percentages, stale missing-integration claims and the older replacement PED were not adopted. This entry records AI assistance and automated verification. Student review and acceptance were still pending at this checkpoint.

## Cameron M4 presentation and consolidation 18 October 2026

OpenAI Codex authored Cameron's Razor pages/controllers, proposed account/request interfaces, session revalidation and 30 black-box/UI contract checks. It moved and guarded the review workflow, continued the same RTM with per-AC outcomes/formulas, and appended a dated M4 draft to the retained PED. It inspected a synthetic browser preview and ran the existing database/API suites. The packaged document renderer lacked LibreOffice; the installed Word exporter was used for PDF layout review.

Rejected/corrected: replacing current composition/PED with older supplied files; adopting the stale 33.3% estimate; treating test adapters as delivered persistence or UI checks as participant/deployed acceptance; a source-scan false positive for an optional Status parameter. Raw logs and limitations are in docs/evidence/m4-cameron-20261018. Codex ran the new automated checks. Personal review, teammate approval, participant validation and submission were still pending at this checkpoint.

## M4 final closure 18 October 2026

### Cameron Purdon M4 AI usage

| Field | Record |
| --- | --- |
| Tools | ChatGPT and OpenAI Codex |
| Purpose | Guidance, troubleshooting, rough drafts and consolidation of the team’s M4 work |
| How assistance was used | Help with understanding requirements and interfaces, diagnosing integration problems, preparing initial drafts and bringing the existing PED, RTM and registers together. Codex also assisted with portal implementation, test code and automated verification. |
| Human verification | Cameron confirmed on 18 October that he reviewed all the code, worked through the application, read and inspected the PED and RTM several times, and ran builds himself several times. He also observed all five usability sessions and recorded completion, timings, errors and coaching. The automated execution reports retain their original operators. |
| Changes following checks | Priority sorting, the management landing page, history/audit information, safe error messages and assignment rules were corrected during integration. Earlier completion estimates were replaced with the final RTM results. |
| Responsibility | Cameron remains responsible for checking and understanding the work he submits and for explaining his contribution during the defence. Final teammate review is recorded separately. |

### Michael Ellis M4 AI usage

Michael supplied the following account through Cameron on 18 October 2026.

| Field | Record |
| --- | --- |
| Tool | ChatGPT (OpenAI) |
| Purpose | Assistance with error handling, debugging and troubleshooting during M4 implementation |
| How assistance was used | Identifying possible causes of errors, explaining exception messages and suggesting solutions for authentication, database connectivity and testing issues |
| Human verification | Michael reports that he reviewed, implemented and tested all suggested changes using JetBrains Rider, .NET build commands and automated tests. |
| Outcome | Improved error handling and resolved implementation issues, with Michael retaining responsibility for the final code. |

### Bianca Grobler M4 AI usage

| Field | Record |
| --- | --- |
| Tools | ChatGPT and Claude Code; both confirmed through Cameron on 18 October 2026 |
| How assistance was used | Bianca reports using ChatGPT to format tables and tidy the writing. Her supplied register records Claude Code assistance with lifecycle, ownership and summary code, 44 checks, and initial document drafts. |
| Human verification | Bianca confirmed through Cameron that she personally tested all her M4 work. |
| Responsibility | Bianca remains responsible for checking and understanding her contribution. |
