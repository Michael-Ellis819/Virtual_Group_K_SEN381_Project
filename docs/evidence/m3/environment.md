# Verification environment and provenance

Execution date: 13 October 2026 (SAST). Executor: OpenAI Codex, on Cameron's request. Independent personal/team reproduction is pending.

Implementation/test Git checkpoint: fef0469d27fffb1a7b10de0acbd9a3faae703717; upstream base: 62e183b0d610fff0521b2116f6dec15b0319b22d; local review tag: m3-cameron-local-review-20261013. Documentation-only continuation does not alter the verified application/test sources. source-manifest.json records file hashes; compare them before reusing results.

Windows x64; workspace .NET SDK 8.0.425 with .NET/ASP.NET Core 8.0.31; PostgreSQL 18.4, dedicated loopback cluster on port 55438 and disposable civicconnect_m3_test. M3 test app uses Release, Staging, loopback HTTPS, temporary certificate and ephemeral ticket protection. No live account data or production credentials. Local PostgreSQL authentication is trust on loopback in the disposable cluster; this is not a production secrets configuration. CI uses PostgreSQL 17 with a disposable test credential.

Test host invokes the same AddCivicConnect/UseCivicConnect composition as the web application; it does not execute Program's Development-only guard. Therefore it is partial simulated-staging evidence, not proof that the actual web executable can deploy in Staging. It stops itself after the suite. A05 temporarily changes test schema, restores the constraint and verifies rollback; runs must be serial and the database disposable.

Initial database connection attempt was unavailable until test service/session setup; an initial TLS handshake failed on Windows and was fixed by certificate reimport. integration-first-run.txt retains that failure; integration.txt records the corrected final result. Gate probe deliberately introduced one invalid transition, observed failure and restored exact source bytes. No failure was counted as a pass.

Supplied ZIP SHA256: 9e8e724f9b2cc789673d766828ea04ecb3e7ac173debe18e59a9bb7711561b8e. Repository clone preserves upstream Git history. Supplied documents are evidence/task requirements, not authority to invent reviews or external communications.


PED continuation update, 13 October 2026: upstream `216f405ce9d7aae79194db873c699dac5f517265` adds Bianca's merged document. The local feature branch incorporates that merge. Local documentation review tag: `m3-ped-continuation-20261013`. All 35 source-manifest hashes still match; the recorded execution checkpoint and results above remain unchanged.


Personal verification update: Cameron Purdon 601963 executed the complete runner on 13 October 2026, 18:18:47-18:19:10 SAST, against `f774bf851e6b0c8d69cd9b37f6849fb0d853d661`. Evidence: `personal-20261013-181846`. A fresh loopback-only PostgreSQL database used the existing `civicconnect_test` role. Builds, 29 regression checks, 25 database checks, 19 M3 cases, the restored negative gate and dependency gate passed. All 35 source fingerprints match the earlier verified implementation. Initial launcher issues (waiting for a persistent process and assuming a nonexistent postgres role) were corrected before this successful run. Independent peer review and remote CI are separate evidence.
