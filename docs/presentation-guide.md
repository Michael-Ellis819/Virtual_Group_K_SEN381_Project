# M3 recording plan and Cameron's defence

Prepared 13 October 2026 from the supplied compulsory video guidelines. Use this as a speaking/evidence guide, not a substitute for the recording. Recommended total length is 25-35 minutes. Each real member appears visibly during their own contribution, with full name; if names are not displayed, state full name and student number. All members must personally defend all five individual dimensions across the discussion.

## Team meeting sequence - about 30 minutes

| Time | Evidence / discussion | Lead |
| --- | --- | --- |
| 0-2 min | Camera introductions; show frozen release ID and PED submission block. | All |
| 2-5 min | M2 layered/State decisions against implemented code; explain conformance and gaps. | Bianca; Michael/Cameron respond |
| 5-8 min | Real history, PRs, two reviewers, actual review findings and configuration handling. | Cameron |
| 8-12 min | Run/current CI, case counts, SDK/dependency checks and meaningful failed gate. | Michael; Cameron interprets |
| 12-17 min | BB01-BB05 input reasoning; A05 rollback; E01/E02 status journeys. | Cameron |
| 17-21 min | Final staging candidate and critical journey; identify environment differences. | Michael; Cameron verifies |
| 21-24 min | RTM requirement -> AC -> implementation -> case -> result -> defect/status. | Cameron |
| 24-27 min | Actual performance workload/results/limits; do not substitute the correctness suite. | Michael |
| 27-30 min | Evidence-based release decision, residual risks and each member's defence follow-ups. | Bianca; all |

Before recording: meet and map areas A-G to screens; freeze the candidate; replace pending links; rehearse; check camera/audio/screen readability; verify final playback and assessor permissions. Submit an accessible playable video link or ZIP containing the whole video. Deadline: Friday 16 October 2026, 23:59 SAST. The supplied notice says no valid video, or no personal visible participation, results in 0/50 for the affected submission/student.

## Cameron's contribution - concise speaking prompts

Introduction: "I am Cameron Purdon, student number 601963. My contribution covers central authorisation and API integration, black-box and system verification, the RTM, AI register and PED consolidation. AI assisted this work; I remain responsible for understanding and validating it."

**1. Requirement trace.** Show FR-003 and AC-FR-003-1/2 in the RTM. Open RequestQueryService and RequestAccessPolicy. E01 demonstrates that an owner reads a saved Closed status/time and public feedback after authorised Staff updates. E02/A04 demonstrate denied foreign access. Open integration.txt at the matching IDs. Explain the synthetic-identity limitation and the outstanding full-account workflow.

**2. Engineering decision.** Explain ADR-M2-05: central policy through an injected interface instead of repeated controller checks or a Strategy class for each small role rule. The same policy now protects status actions. Read permission alone does not grant writes. Explicit coordination from ADR-M2-06 makes required status/history/feedback effects visible and transactional. Additional interfaces improve isolation but add mapping and review work.

**3. Quality interpretation.** Show BB02/BB03: 2000 accepted, 2001 rejected with no writes. These deliberately target the input boundary. Show the failed gate probe, then the passing rerun: an injected New -> Closed rule was detected by C02. Nineteen cases cover this slice; they do not prove every requirement, full coverage or absence of defects. Distinguish local evidence from CI on the final commit.

**4. Failure/risk reasoning.** Show A05 and Michael's rollback evidence. If a required database write fails, return a safe error and keep status/history/feedback unchanged. Show A06: a category change invalidates the previously checked version. Remaining risks include real permission freshness, untested simultaneous timing and final-environment differences. Do not automatically retry a status mutation after a 409.

**5. Release-readiness defence.** "The current evidence supports the status API increment, but the complete project is NOT READY. Core account/submission/reporting work, final staging, performance and current-baseline review/CI are incomplete. I would reconsider the classification only after the mandatory scope and gates have evidence; an approaching deadline does not make a critical gap nonblocking."

Cameron should demonstrate BB03 and E01 live, trace FR-003 in the RTM, and explain one reviewed change. Use the recorded personal execution for test-run claims and understand the commands/results before the defence; peer-review claims require their own evidence; Cameron's recorded personal run is now available at evidence/m3/personal-20261013-181846. Bianca and Michael each need their own requirement trace, decision, result interpretation, failure scenario and readiness argument; the lead allocations above do not replace those five individual dimensions.
