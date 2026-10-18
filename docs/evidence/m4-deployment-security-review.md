
# M4 — Deployment Security Review

**Project:** CivicConnect  
**Milestone:** M4  
**Review Date:** 16 October 2026  
**Review Status:** Completed — Production readiness pending

## 1. Purpose

This review evaluates the security configuration of CivicConnect during Milestone 4.

It focuses on database credentials, source-control protection, HTTPS, authentication cookies, ASP.NET Core Data Protection keys and production deployment readiness.

The objective is to identify implemented security controls, record verification evidence and document outstanding risks before deployment.

## 2. Database Credentials and Secrets Management

CivicConnect obtains its PostgreSQL connection string through ASP.NET Core IConfiguration.

The application retrieves the connection string using:

```csharp
var connectionString =
    configuration.GetConnectionString("CivicConnect")
    ?? throw new InvalidOperationException(
        "CivicConnect database connection is not configured.");
```

During development, the PostgreSQL connection string is stored using .NET User Secrets under:

`ConnectionStrings:CivicConnect`

This keeps the development password outside the Git repository.

**Verification result:** PASS — Development configuration verified.

**Limitations:**
- .NET User Secrets are not encrypted.
- User Secrets are not intended for production secret storage.
- Production credential provisioning and rotation have not been verified.

## 3. GitHub and Source-Control Protection

The project's `.gitignore` contains rules protecting local configuration, credentials and database backups.

Relevant patterns include:

```gitignore
.env
.env.*
**/appsettings.Local.json
**/appsettings.Development.json
**/DataProtection-Keys/
*.pfx
*.key
*.dump
```

Git ignore rules were verified using `git check-ignore`.

A tracked-file name inspection found no matches for the sensitive file patterns checked.

Gitleaks was also added to GitHub Actions. The observed scan completed without reporting secrets in the scanned commit.

**Verification result:** PASS — Selected repository safeguards verified.

**Limitations:**
- `.gitignore` does not remove previously committed files.
- Filename checks do not inspect file contents.
- The observed Gitleaks run did not establish that the entire repository history was free of secrets.
- Branch protection and mandatory security checks require separate verification.

## 4. HTTPS Configuration and Verification

HTTPS redirection was enabled in the application using:

```csharp
app.UseHttpsRedirection();
```

The application was tested locally with:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7001`

The ASP.NET Core development HTTPS certificate was trusted on macOS.

### Test Results

| Test | Observed Result | Status |
|---|---|---|
| HTTP request to `/health` | HTTP 307 redirect | PASS |
| Redirect destination | `https://localhost:7001/health` | PASS |
| HTTPS request to `/health` | HTTP 200 | PASS |
| Health response | `{"status":"Healthy"}` | PASS |
| HTTPS certificate validation | Request succeeded without bypassing certificate verification | PASS |

**Verification result:** PASS — Local HTTPS functionality verified.

**Limitations:**
- Production HTTPS hosting was not tested.
- Production TLS certificate provisioning and renewal remain outstanding.

## 5. Authentication Cookie Security

The application's existing cookie authentication configuration includes:

```csharp
o.Cookie.HttpOnly = true;
o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
o.Cookie.SameSite = SameSiteMode.Lax;
```

The configuration also returns HTTP 401 for unauthenticated access and HTTP 403 for access denial.

These settings provide useful protections for authentication cookies.

**Verification result:** PASS — Configuration inspected.

**Limitations:**
- Cookie attributes were not verified through a real authenticated browser session.
- SameSite=Lax is not a complete substitute for CSRF protection.
- CSRF controls must be reviewed when state-changing endpoints are integrated.

## 6. ASP.NET Core Data Protection

ASP.NET Core Data Protection protects sensitive application data, including authentication cookie tickets.

### 6.1 Verification Project

The verification project contains:

```csharp
builder.Services.AddDataProtection()
    .UseEphemeralDataProtectionProvider();
```

This uses temporary Data Protection keys for testing.

The configuration was found in the verification project, not in the main web application's startup configuration.

### 6.2 Web Application

The web application's startup logs confirmed the following:

```text
User profile is available.
Using the local .aspnet/DataProtection-Keys directory
as the key repository; keys will not be encrypted at rest.
```

A local key file was found in:

`~/.aspnet/DataProtection-Keys/`

The observed key file permissions were:

```text
-rw-------
```

This restricts file access to the owning macOS user under normal filesystem permissions.

### 6.3 Findings

| Security Property | Finding |
|---|---|
| Persistent local key storage | Confirmed |
| Storage outside Git repository | Confirmed |
| Owner-only key file permissions | Observed |
| Encryption at rest | Not enabled in observed development configuration |
| Production key persistence | Not verified |
| Production key encryption and rotation | Not verified |

**Verification result:** PARTIAL — Development behaviour verified; production protection outstanding.

### 6.4 Required Production Controls

Before production deployment:

1. Configure persistent Data Protection key storage appropriate for the hosting environment.
2. Protect keys at rest using an appropriate encryption or key-management mechanism.
3. Restrict access to the key repository.
4. Ensure application instances can access the required keys where multiple instances are used.
5. Establish key backup, rotation and recovery procedures.
6. Verify authentication cookie continuity across application restarts and deployments.

## 7. Deployment Readiness

The application's Program.cs currently prevents startup outside the Development environment:

```csharp
if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException(
        "This slice requires Development. Integrate persistent storage and identity provisioning before deployment.");
```

This restriction remains in place intentionally.

Although database persistence and several security controls have been implemented, production authentication, deployment configuration and operational safeguards still require integration and verification.

### Outstanding Deployment Requirements

| Requirement | Status |
|---|---|
| Local HTTPS redirection | Verified |
| Secure cookie configuration | Inspected |
| Development secrets stored outside Git | Verified |
| CI secrets scanning | Tested on observed commit |
| Persistent production Data Protection keys | Pending |
| Encrypted production key storage | Pending |
| Production secret management | Pending |
| Authentication and authorisation integration | Pending |
| CSRF protection for relevant state-changing endpoints | Pending review |
| Production HTTPS and certificate configuration | Pending |
| Staging deployment and release verification | Pending |

## 8. Security Risks and Recommendations

**Risk 1 — Unprotected production Data Protection keys**

Keys that are not appropriately protected could expose authentication-related protected data if compromised.

Recommendation: Configure persistent, access-controlled and encrypted production key storage.

**Risk 2 — Incomplete authentication integration**

Unfinished identity and role provisioning could lead to unauthorised access.

Recommendation: Complete authentication integration and verify record-level authorisation before deployment.

**Risk 3 — Exposure of deployment credentials**

Credentials placed in source control or deployment scripts could be exposed.

Recommendation: Use environment-specific secret management, least-privilege database accounts and secret rotation procedures.

**Risk 4 — CSRF vulnerabilities**

Future authenticated state-changing endpoints may be vulnerable to cross-site request forgery if appropriate protection is absent.

Recommendation: Assess each state-changing endpoint and implement anti-forgery protection where required.

## 9. Conclusion

The CivicConnect M4 deployment security review confirmed several implemented controls, including:

- Development database credentials stored using .NET User Secrets.
- Git ignore rules protecting local configuration and backups.
- CI secrets scanning.
- Working local HTTPS redirection.
- Secure authentication cookie configuration.
- Persistent local ASP.NET Core Data Protection key storage.

However, production Data Protection key encryption, production secret management, authentication integration and deployment verification remain outstanding.

**Overall Assessment: NO-GO for production deployment at the time of this review.**

The Development-only startup restriction should remain until the outstanding requirements have been implemented and verified.

## Final integration update — 18 October 2026

The earlier assessment above retains its review context. Stored password hashes, server-owned roles/category grants, active/stamp session checks and real portal adapters are now integrated. The persistent HTTPS suite passes 36 checks, including bad credentials, unauthorised access, antiforgery, competing claims, rollback, safe errors and revoked sessions; remote CI run 39145430277 passed on 625e4e9. This replaces the earlier statement that authentication integration and end-to-end authorisation were entirely outstanding.

Controlled Development/Staging startup is now permitted; Production remains blocked. Local generated/pinned certificates and the loopback participant preview do not establish deployed trusted TLS, production key encryption/secret operations or a hosted rollback. Login rate limiting, independent security assessment and production monitoring remain limitations. Public production remains NO-GO; no security owner acceptance is invented.
