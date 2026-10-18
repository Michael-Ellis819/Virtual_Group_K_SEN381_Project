
# M4 — STRIDE Security Threat Assessment

**Project:** CivicConnect  
**Milestone:** M4  
**Assessment Date:** 16 October 2026  
**Assessment Method:** STRIDE  
**Status:** Initial implementation-stage assessment

## 1. Purpose

This assessment identifies security threats affecting CivicConnect and evaluates the controls implemented or still required.

The STRIDE methodology considers six categories of threats:

- Spoofing
- Tampering
- Repudiation
- Information Disclosure
- Denial of Service
- Elevation of Privilege

The assessment supports the project's security, access-control, operational-readiness and risk-management activities.

## 2. System Scope

The assessment covers:

- ASP.NET Core application and HTTP endpoints.
- Authentication and role-based authorisation.
- Request ownership and category-based access restrictions.
- PostgreSQL database and EF Core persistence.
- Request status changes and history.
- Application logging and health monitoring.
- GitHub repository and CI security checks.

The system currently contains implemented functionality alongside features that remain under development.

## 3. STRIDE Threat Analysis

| ID | STRIDE Category | Threat | Potential Impact | Existing Control or Evidence | Remaining Risk / Action | Risk |
|---|---|---|---|---|---|---|
| ST-01 | Spoofing | An attacker impersonates a requester or staff member. | Unauthorised access to requests and staff functions. | RequestAccessPolicy validates identity claims and denies invalid or ambiguous identities. | Complete and verify production authentication and session management. | High |
| ST-02 | Tampering | A user changes request status or database records without permission. | Incorrect request status and loss of business correctness. | EF Core persistence and transactional status-write functionality exist. | Verify end-to-end authorisation, allowed status transitions and concurrency behaviour. | High |
| ST-03 | Repudiation | A user denies changing a request's status. | Disputes cannot be investigated reliably. | RequestStatusHistory table and persistence structures exist. | Verify actor attribution, append-only behaviour and coverage of all relevant changes. | Medium |
| ST-04 | Information Disclosure | A requester accesses another requester's records or internal staff notes. | Exposure of confidential request information. | Central RequestAccessPolicy enforces ownership, staff-category rules and note visibility; verification tests exist. | Confirm all new M4 endpoints consistently use the policy. | High |
| ST-05 | Denial of Service | Excessive requests or a database outage makes the application unavailable. | Requesters and staff cannot use CivicConnect. | Application and database health endpoints, HTTP 5xx logging and slow-request warnings have been locally tested. | Add appropriate rate limiting, alerting and deployment monitoring. | Medium |
| ST-06 | Elevation of Privilege | A requester gains staff permissions through manipulated claims or insecure endpoints. | Unauthorised staff operations and access to internal data. | Access policy checks recognised roles and denies ambiguous claims. | Verify role provisioning, authentication configuration and endpoint-level authorisation. | High |
| ST-07 | Tampering | An attacker submits malicious input to manipulate database queries (SQL injection). | Unauthorised changes to or disclosure of database records. | EF Core is used for database access and normally parameterises LINQ-generated queries. | Review any raw SQL usage, validate inputs and test database-facing endpoints. | High |


## 4. Additional Security Considerations

### 4.1 Secrets Management

A Gitleaks scan was added to the GitHub Actions workflow.

The scan completed successfully without reporting leaks in the tested commit.

Remaining actions:
- Keep database credentials out of source control.
- Use environment-specific secret management.
- Verify that CI checks remain mandatory before merging.

### 4.2 Database Recovery

PostgreSQL backup and restore verification was completed using a separate test database.

The restored database contained the expected tables and matching record counts.

Evidence: `docs/evidence/m4-backup-restore.md`

Remaining actions:
- Define backup frequency and retention.
- Protect backup files against unauthorised access.
- Establish recovery-time and recovery-point objectives.

### 4.3 Application Observability

The application includes:
- `/health` for application health.
- `/health/database` for PostgreSQL connectivity.
- Logging of HTTP server errors.
- Warnings for requests exceeding 1,000 milliseconds.

These checks were tested locally.

Remaining actions:
- Configure monitoring and alerts in the target environment.
- Restrict unnecessary exposure of operational details.
- Verify logging does not disclose sensitive information.


## 5. Risk Priorities

The highest-priority threats are:

1. **ST-01 — Spoofing:** Authentication and identity provisioning must be completed before deployment.
2. **ST-02 — Tampering:** Business-rule and persistence controls must be verified through integration testing.
3. **ST-04 — Information Disclosure:** Record ownership and internal-note restrictions must remain enforced across every relevant endpoint.
4. **ST-06 — Elevation of Privilege:** Staff permissions must not be obtainable through untrusted claims or insecure endpoints.

These threats should be reviewed alongside the project's existing Risk Register.

## 6. Verification Limitations

This assessment identifies threats and records available implementation evidence. It does not establish that the application is penetration-tested or production secure.

Outstanding verification includes:

- Full authentication and role-management testing.
- End-to-end authorisation testing for new M4 workflows.
- Deployment security and HTTPS configuration.
- Rate-limiting and denial-of-service resilience.
- Independent security testing.
- Production monitoring and alert configuration.

## 7. Conclusion

STRIDE identified six primary threat categories affecting CivicConnect.

Existing access-control, persistence, CI scanning and observability mechanisms provide partial mitigation.

However, authentication completion, end-to-end authorisation and deployment hardening remain necessary before production release.

**Assessment Outcome:** Threats identified and prioritised; production security readiness is not yet established.
