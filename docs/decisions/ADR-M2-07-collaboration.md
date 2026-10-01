# ADR M2 07 Repeatable checks and reviewed integration

Owner: Cameron Purdon. Date: 30 September 2026. Status: Accepted for the M2 baseline; local build and verification evidence completed.

Assignment 2 Task 4 recommends short-lived issue-linked branches, protected main, two non-author approvals and progressive CI. Manual-only checking is easy to start but easy to omit. A complete deployment pipeline exceeds this slice's needs. Select a proportionate build plus the actual 29-check harness on pull requests to main and pushes to main. Fail on compilation warnings/errors or failed assertions. No deployment is performed.

The workflow in `.github/workflows/verify.yml` runs the build and verification harness for pull requests and pushes to main. A workflow file is not evidence that a remote run passed or that the check is required. A remote Actions run was not present at the public checkpoint, so the recorded local execution is the current verification evidence. Meaningful review findings and corrections must remain traceable.

Consequences: the package-free harness is straightforward to run but lacks a standard test runner's discovery/reporting. Revisit a conventional .NET test framework with Michael. The workflow checks the read slice only; it cannot establish PostgreSQL integrity, login, load or production readiness. Runtime upgrades affect global.json, project targets, workflow, README and the recorded verification environment together.

Evidence: `docs/evidence/local-verification.txt` records 29 passing checks. Public inspection confirmed protected main, two distinct reviewers on PR #11 and one on PR #10; detailed protection settings were not publicly accessible. Bianca Grobler and Michael Ellis reviewed the consolidated M2 PED for submission. Risks R-038 and R-044 remain open. GitHub's protected-branch documentation explains the configurable controls: https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches (accessed 30 September 2026).
