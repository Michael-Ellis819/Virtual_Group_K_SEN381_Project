# GitHub and controlled-review evidence - 13 October 2026

Existing repository: https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project. Current inspected main: `216f405ce9d7aae79194db873c699dac5f517265`, after Bianca PR15 merged. Local feature branch: `feature/cameron-m3-integration`. Implementation/test checkpoint: `fef0469d27fffb1a7b10de0acbd9a3faae703717`. These are authentic new local commits; publication is pending.

| Evidence | Observed status / limit |
| --- | --- |
| [PR12 Complete Milestone 2 contribution](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/12) | Bianca and Michael approvals present. The handover's unknown second-review state is superseded. |
| [PR14 persistence implementation](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/14) | Merged in inspected main; approvals by Bianca333-b and RinNoKaze. Review bodies returned empty. |
| [PR15 my parts](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/15) | Merged on 13 October 2026; Michael and RinNoKaze approvals present. Added Milestone3 Bianca.docx. The Word contribution is incorporated into the continued PED; no application code changed in this merge. |
| [Actions run 38218128326](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38218128326) | Successful on f1b11d7f36a8e67570052ddf073879a2c3f35117. Build and both existing harness steps succeeded. This predates Cameron's new code. |
| Main protection / new checks | Historical protection evidence exists. Current rules and required-check names were not independently retrieved. Maintainer must confirm two approvals and civicconnect-build-and-check required on current main. |

The root file github_workflows_auto-merge-approved-prs.yml is outside .github/workflows, so its presence does not establish an active automation. No protection weakening or GitHub merge was performed by Codex. Updated upstream main was incorporated into the local feature branch.

## Publish and review Cameron's contribution

1. Open this checkout/branch in GitHub Desktop. Inspect the implementation, test/CI and documentation commits, then publish the branch to the existing repository and open a PR into main. A prepared title/body is in [pr-description.md](pr-description.md).
2. Bianca independently checks lifecycle/conformance, invalid transitions and the release/register statements. Michael checks transactional integration, version handling, migrations, CI and the State dependency. Each records what was actually checked and any findings on the PR before approving.
3. Fix review findings on the feature branch; rerun affected checks. Both non-author approvals and required CI must apply to the current revision. An agent's own review cannot substitute for two independent peers.
4. Merge only after required reviews/checks. Freeze the final assessed commit/tag and align PED, RTM, environment, evidence and recorded demo with it.

Suggested review checklist: reproduce E01/E02; inspect BB03 and A05 no-write/rollback assertions; verify A06 rejects stale category/version; confirm no new input supplies actor/roles; compare unchanged requirement/AC IDs; confirm honest pending scope; check current CI and staging parity. Review comments and any rework must be real; no example comment should be posted as if it occurred.
