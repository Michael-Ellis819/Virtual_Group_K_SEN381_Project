# PERF-01 second-run confirmation

Michael Ellis 600332 supplied the following run details, relayed by Cameron in this chat on 14 October 2026. The saved second run is the intended evidence; it has not been relabelled as Cameron's execution.

- Report: `docs/evidence/m3-performance-results.txt`, preserved from PR16.
- Date: 14 October 2026.
- Environment: macOS; .NET SDK 8.0.425; EF Core 8; PostgreSQL 17.
- Workload: 200 reads of one temporary service request, maximum concurrency 10.
- Results: 200 successful; 0 failures; 1.62 ms average; 0.41 ms median; 8.69 ms p95; 0.12 seconds; 1,696.16 operations/sec.
- Purpose: requesters and staff need responsive request-detail/status retrieval.
- Interpretation: short database-read benchmark only; no separate database-level target; not sustained HTTP/user-visible pilot-load or full NFR-002 acceptance.
- Exact execution commit: not specified in the supplied confirmation. PR16 source is `4a7cebe46dbf92f150c49646da86bf1b8e72d365`, not asserted as the execution commit.

Original macOS recording command (spacing normalised):

```bash
dotnet run --project tests/CivicConnect.PerformanceTests/CivicConnect.PerformanceTests.csproj --configuration Release | tee docs/evidence/m3-performance-results.txt
```

On another machine use a separate output filename and a migrated disposable test database. Do not overwrite Michael's historical report. The 14 October local dependency fix changes test-project package configuration and leaves the performance program unchanged; the original report is not asserted as execution of the patched candidate.
