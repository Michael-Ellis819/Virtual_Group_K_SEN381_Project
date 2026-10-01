# GitHub and collaborative engineering evidence

Repository: https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project

Checkpoint: 30 September 2026. Public repository information was inspected read-only; no remote setting, workflow result, commit, issue, pull request or review was invented.

## Observed history

| Item | Observed evidence |
| --- | --- |
| Main | Commit `fef07828d1`; merged final A2 report on 15 September 2026; the tree still contained Assignment 2 material at the checkpoint |
| Main protection | The public branch response reported `protected=true`; detailed protection settings were not publicly accessible |
| Cameron M1 | PR #2 added the requirements and RTM; PR #3 refined rules and scope; both were merged on 9 September with approvals from Michael and Bianca |
| Cameron A2 | PR #8 was merged on 15 September; Michael and Bianca approved it |
| Bianca M2 | PR #10 was open at the checkpoint; Michael approved that revision and no second independent approval was publicly visible |
| Michael M2 | PR #11 was open at the checkpoint; Bianca and RinoKaze approved that revision and no requested changes were publicly visible |
| Final PED review | Bianca Grobler and Michael Ellis reviewed the consolidated M2 PED and accepted it for submission on 30 September 2026 |
| Remote checks | No GitHub Actions run was visible at the public checkpoint; the submission therefore relies on the recorded local build and verification result |

Permanent references: [M1 PR #2](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/2), [M1 PR #3](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/3), [A2 PR #8](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/8), [Bianca M2 PR #10](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/10), [Michael M2 PR #11](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/pull/11), [M1 issue #1](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/issues/1) and [A2 issue #7](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/issues/7).

## Repeatable verification decision

ADR-M2-07 selects a build and the 29-check verification harness for pull requests and pushes to main. The workflow source is `.github/workflows/verify.yml`. A workflow file shows the intended check but is not evidence of a successful remote run. The local result is recorded in `docs/evidence/local-verification.txt`. Risks R-038 and R-044 retain the clean-environment and remote-CI limitations.
