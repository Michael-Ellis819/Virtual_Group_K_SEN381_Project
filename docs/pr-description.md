# Prepared PR title and body

Title: Complete M3 authorised status integration, black-box/system tests and traceability

Michael's persistence writer was not connected to an authorised status API. This change adds a Staff/category action policy and explicit status coordinator, using the agreed State lifecycle and existing transactional writer. Successful updates commit status, audit history and public feedback; invalid, unauthorised and stale changes are rejected. Cookie mutations require antiforgery.

Adds 19 meaningful cases (6 component, 5 black-box using BVA/EP, 6 API/integration, 2 system), extends the workflow with these cases/static/dependency gates, evolves the existing RTM, and continues the existing PED with current evidence, registers and presentation guidance. Original project history and Michael's contribution are preserved. Minimal State implementation was an integration dependency; Michael must review it.

Validation: 19 new cases, 29 existing verification checks and 25 database checks passed locally; builds/static checks produced zero warnings/errors; NuGet reported no vulnerable packages. Deliberately permitting New -> Closed failed C02; source restored and passing rerun retained. Local PostgreSQL 18.4 differs from CI 17. Remote checks on this PR are still required.

Review: Bianca checks conformance/lifecycle/registers; Michael checks persistence/version/State/CI. Two independent non-author approvals required. Real login, creation/assignment/reporting, final web staging, performance/recovery and final team release approval remain pending; overall classification is NOT READY. See docs/change-control-m3.md, testing-m3.md, release-evidence.md and the continuing RTM.
