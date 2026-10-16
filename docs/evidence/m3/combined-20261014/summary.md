# Combined local verification 14 October 2026

Executor: OpenAI Codex. These follow-up checks are distinct from Cameron's personal 13 October run and Michael's saved performance run.

Four Release projects build with zero warnings/errors. Correctness suites pass: 29 regression checks, 25 database checks and 19 M3 cases. Disposable PostgreSQL 18.4 database: civicconnect_combined_20261014_test, loopback 127.0.0.1:55438. SDK 8.0.425/runtime 8.0.31. Raw logs and working-copy source fingerprints are beside this summary. This is not remote CI or independent human review.

Transitive strict restore audit initially failed on System.Text.Json 8.0.0 in the test projects (NU1903). Both database/performance projects now explicitly resolve 8.0.5. Fresh restore with NuGetAudit=true, NuGetAuditMode=all and TreatWarningsAsErrors=true succeeds without reported advisories; both affected projects and all correctness suites were checked after the fix. Audit logs preserve the failure and passing restoration. Advisory results are dated and not complete security assurance.

The SDK package-list JSON command failed with an object-reference error, so the test-project security check uses strict restore-time audit. The existing web-project report/gate remains in the CI workflow. The new workflow also builds/runs PERF-01 and audits both affected test projects; no actual remote run of this combined workflow is claimed.

Michael's performance program/raw result is preserved from PR16; the original human performance run was not rerun or overwritten. The two existing fingerprint differences are the workflow and database-test package configuration. Application source and correctness-test programs remain unchanged.
