# GitHub and controlled-review evidence 15 October 2026

Published combined implementation: `668795a42d8191600e79ff4decad1642bd3e88b0`. [PR17](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/17) merged on 14 October 2026 at 18:01:52 SAST. The final submission documentation is dated 15 October; application, tests and CI configuration are unchanged from this published implementation.

| Evidence | Verified result |
| --- | --- |
| PR17 source | `454261512cd4104c000e8da071d6886af18cbc01` |
| Bianca approval | 14 October, 17:46:01 SAST |
| Michael approval | 14 October, 18:01:38 SAST |
| [Combined main CI 38594557024](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024) | Success on 668795a. Regression, database, M3 and performance projects build/run; analysis and dependency steps pass |
| Combined PR CI | Run 38591809871 succeeds on PR17 source 4542615 |
| Actual conformance findings | [Bianca's completed review](reviews/Bianca_M3_Final_Review_Updated.docx), dated 14 October; combined-work review confirmed by Cameron |

Returned GitHub approval bodies were empty. The standalone conformance findings provide separate evidence of actual checking; no review comments or rerun by a reviewer are inferred. Main CI success is demonstrated; required status-check enforcement is a separate repository setting. The Main ruleset was rechecked on 15 October: it requires two approvals and dismisses stale approvals, with no required-status-check rule.

Earlier records below preserve what was observed at each checkpoint. Their publication/CI/review gaps describe those earlier dates and are superseded by the current evidence above.

# Historical checkpoint - 13 October 2026

Existing repository: https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project. Current inspected main: `216f405ce9d7aae79194db873c699dac5f517265`, after Bianca PR15 merged. Local feature branch: `feature/cameron-m3-integration`. Implementation/test checkpoint: `fef0469d27fffb1a7b10de0acbd9a3faae703717`. These are authentic new local commits; publication is pending.

| Evidence | Observed status / limit |
| --- | --- |
| [PR12 Complete Milestone 2 contribution](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/12) | Bianca and Michael approvals present. The handover's unknown second-review state is superseded. |
| [PR14 persistence implementation](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/14) | Merged in inspected main; approvals by Bianca333-b and Cameron. Review bodies returned empty. |
| [PR15 my parts](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/15) | Merged on 13 October 2026; Michael and Cameron approvals present. Added Milestone3 Bianca.docx. The Word contribution is incorporated into the continued PED; no application code changed in this merge. |
| [Actions run 38218128326](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38218128326) | Successful on f1b11d7f36a8e67570052ddf073879a2c3f35117. Build and both existing harness steps succeeded. This predates Cameron's new code. |
| Main protection / new checks | [Active Main ruleset](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/rules/22445828) inspected on 13 October: two approvals required and stale approvals dismissed. No required CI-status-check rule is present. No current Cameron branch remote run or open PR was recorded. |

The root file github_workflows_auto-merge-approved-prs.yml is outside .github/workflows, so its presence does not establish an active automation. No protection weakening or GitHub merge was performed by Codex. Updated upstream main was incorporated into the local feature branch.

## Historical publication and review checklist

1. Open this checkout/branch in GitHub Desktop. Inspect the implementation, test/CI and documentation commits, then publish the branch to the existing repository and open a PR into main.
2. Bianca independently checks lifecycle/conformance, invalid transitions and the release/register statements. Michael checks transactional integration, version handling, migrations, CI and the State dependency. Each records what was actually checked and any findings on the PR before approving.
3. Fix review findings on the feature branch; rerun affected checks. Both non-author approvals and required CI must apply to the current revision. An agent's own review cannot substitute for two independent peers.
4. Merge only after required reviews/checks. Freeze the final assessed commit/tag and align PED, RTM, environment, evidence and recorded demo with it.

Suggested review checklist: reproduce E01/E02; inspect BB03 and A05 no-write/rollback assertions; verify A06 rejects stale category/version; confirm no new input supplies actor/roles; compare unchanged requirement/AC IDs; confirm honest pending scope; check current CI and staging parity. Review comments and any rework must be real; no example comment should be posted as if it occurred.


## 14 October combined-review checkpoint

PR16 remains open on 4a7cebe; one Bianca approval with empty review body. CI run 38518306719 is successful for the existing two suites only. Its source is merged locally with Cameron’s branch and the native PED/RTM consolidated; no GitHub merge/publication/approval is claimed. Local candidate tag: m3-combined-review-20261014. Corrected figures and dependency fix are part of the local candidate, not yet PR16 on GitHub.


## Historical teammate-review and PR16 CI checkpoint 14 October

Bianca and Michael reviewed the combined implementation 3760605. Bianca’s completed conformance review is in reviews/Bianca_M3_Final_Review_Updated.docx. PR16 remains open on 7f0417bfe1c41d26a30b773dbbdb620cdf90a139; run 38577509314 succeeds for regression/database, performance and both test dependency audits. Earlier Bianca approval was dismissed after source updates; no new active formal approval was returned at this inspection. Combined M3 remote coverage is not established. These repository facts are distinct from the completed offline review.


## PR16 merged and local publication preparation

PR16 was merged on 14 October 2026 at 16:21:59 SAST into main, commit cb588d4b69b447ca72c11c91d795b7ea0daf91f9. The current revision received approvals from Bianca333-b and Cameron at 16:15 and 16:21 SAST; returned review bodies were empty. The completed offline conformance record is separate checking evidence. Scope section 8 expects reviews to show actual checking; the reviewers can record their real checks/findings in the PR discussion.

[Main CI run 38579869423](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38579869423) passed build/run steps for the legacy verification, database and performance projects, followed by two dependency-report steps. It did not run Cameron's 19 M3 cases. The local feature branch incorporates this merged main, retains the combined suite and strict gates, and preserves the supplied final PED/RTM. No remote run, PR approval or submission is claimed for this newly prepared branch. Earlier checkpoints above are retained as history.

## M4 final closure 18 October 2026

Michael’s latest branch input was `5a17e07` (accounts, request operations and scheduling/audit). Main `51c4f9a` merged PR21, which uploaded Bianca’s ZIP; the ZIP was inspected and selectively reconciled rather than overwriting the current source/PED. A backup branch retained the pre-finalisation revision. Original M3 local edits remain separate and preserved.

The tested code is `625e4e99882150f79ad3e00bae327b14e63e5d9c` on `feature/cameron-m4-finalisation`. Push/CI, any later document commit, final tag and PR status are recorded against their actual observed identifiers. A green run is not substituted for two independent current peer reviews. Personal instructions are excluded from the submitted tree; their removal does not rewrite Git history.

Final code CI: [run 39145430277](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/39145430277) completed successfully on `625e4e99882150f79ad3e00bae327b14e63e5d9c`, including static analysis, secrets/dependency gates and the persistent HTTPS suite. The paced read exercise is separate from full NFR-002 acceptance.
