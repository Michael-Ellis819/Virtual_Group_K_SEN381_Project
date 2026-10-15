# ADR M2 07 Repeatable checks and reviewed integration

Owner: Cameron Purdon. Date: 30 September 2026. Status: Accepted for the M2 baseline; local build and verification evidence completed.

Assignment 2 Task 4 recommends short-lived issue-linked branches, protected main, two non-author approvals and progressive CI. Manual-only checking is easy to start but easy to omit. A complete deployment pipeline exceeds this slice's needs. Select a proportionate build plus the actual 29-check harness on pull requests to main and pushes to main. Fail on compilation warnings/errors or failed assertions. No deployment is performed.

The workflow in `.github/workflows/verify.yml` runs the build and verification harness for pull requests and pushes to main. A workflow file is not evidence that a remote run passed or that the check is required. A remote Actions run was not present at the public checkpoint, so the recorded local execution is the current verification evidence. Meaningful review findings and corrections must remain traceable.

Consequences: the package-free harness is straightforward to run but lacks a standard test runner's discovery/reporting. Revisit a conventional .NET test framework with Michael. The workflow checks the read slice only; it cannot establish PostgreSQL integrity, login, load or production readiness. Runtime upgrades affect global.json, project targets, workflow, README and the recorded verification environment together.

Evidence: `docs/evidence/local-verification.txt` records 29 passing checks. Public inspection confirmed protected main, two distinct reviewers on PR #11 and one on PR #10; detailed protection settings were not publicly accessible. Bianca Grobler and Michael Ellis reviewed the consolidated M2 PED for submission. Risks R-038 and R-044 remain open. GitHub's protected-branch documentation explains the configurable controls: https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches (accessed 30 September 2026).

## M3 later consequence - 13 October 2026

Michael's PR14 is merged and has two distinct approvals. PR12 also has two approvals. GitHub Actions run 38218128326 succeeded for Michael's f1b11d7 revision, including both original harnesses. Cameron's new branch has progressive local implementation/test/docs history, with new harness/static/dependency steps configured. The local forbidden-transition probe failed and passing rerun followed restoration. New-branch publication, two meaningful independent reviews and current-revision remote CI remain pending. Existing approval bodies are empty; detailed checking/rework cannot be inferred. Protection settings must still be inspected by an authorised repository maintainer.


## Submission consequence 15 October 2026

PR17 merged the combined implementation at 668795a after approvals from Bianca and Michael on 14 October. [Main run 38594557024](https://github.com/Michael-Ellis819/Virtual_Group_K_SEN381_Project/actions/runs/38594557024) successfully executes all four projects, including the 19 M3 cases, analysis and dependency checks. Earlier publication/review/CI gaps above describe their historical checkpoints. Required CI merge enforcement remains separate from successful execution. Bianca's completed conformance findings record actual checking; empty approval bodies are not treated as written review explanations.
