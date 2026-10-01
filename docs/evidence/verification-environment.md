# Verification environment

Date: 30 September 2026. Cameron reproduced the verification run locally after the initial environment check.

Build command: dotnet build tests/CivicConnect.Verification/CivicConnect.Verification.csproj --ignore-failed-sources
Build result: succeeded; 0 warnings, 0 errors. SDK 8.0.300. No external PackageReference dependencies.

Execution: isolated official ASP.NET Core runtime 8.0.31, including Microsoft.NETCore.App 8.0.31.
Command: dotnet tests/CivicConnect.Verification/bin/Debug/net8.0/CivicConnect.Verification.dll
Result: 29 checks passed, 0 failed. See local-verification.txt for individual checks.
The initial system-runtime attempt failed before running checks because ASP.NET Core 8.0.10 required a missing matching core runtime. A task-local runtime resolved this; the system installation was not upgraded.

Scope: immutable synthetic records; direct application service; real loopback HTTP/MVC and real protected-cookie parsing. Test tickets are minted only by the harness. No PostgreSQL, real login, TLS, load, restore or full acceptance result is claimed.

Document verification: Word export used for page inspection because the packaged LibreOffice renderer was unavailable. Source and final PDFs/PNGs remained local QA files. Workbook requirement wording/54 acceptance criteria/source fields/formulas were compared with M1; added status validation reflects Planned/In Development.
