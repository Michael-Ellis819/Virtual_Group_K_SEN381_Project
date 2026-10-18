
# M4 — HTTPS and Cookie Security Verification

**Project:** CivicConnect  
**Milestone:** M4  
**Test Date:** 16 October 2026  
**Status:** Local verification passed; production security pending

## 1. Objective

Verify that CivicConnect supports HTTPS redirection and uses appropriate security settings for cookie-based authentication.

## 2. HTTPS Configuration

HTTPS redirection was enabled using:

```csharp
app.UseHttpsRedirection();
```

The application was started locally using:

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7001`

A trusted ASP.NET Core development certificate was used.

## 3. HTTPS Test Results

| Test | Expected Result | Actual Result | Status |
|---|---|---|---|
| HTTP redirection | Redirect to HTTPS | HTTP 307 | PASS |
| Redirect destination | HTTPS endpoint | `https://localhost:7001/health` | PASS |
| HTTPS health endpoint | HTTP 200 | HTTP 200 | PASS |
| HTTPS response | Healthy status | `{"status":"Healthy"}` | PASS |
| Certificate verification | Trusted certificate | HTTPS request succeeded without bypassing certificate verification | PASS |

## 4. Authentication Cookie Configuration

The existing ASP.NET Core cookie authentication configuration was inspected.

| Setting | Configuration | Purpose |
|---|---|---|
| HttpOnly | `true` | Prevents JavaScript from reading authentication cookies |
| SecurePolicy | `Always` | Restricts cookie transmission to HTTPS |
| SameSite | `Lax` | Provides partial cross-site request protection |
| Unauthenticated response | HTTP 401 | Returns an unauthorised response |
| Access denied response | HTTP 403 | Returns a forbidden response |

These settings were confirmed through source-code inspection, not through an authenticated browser session.

## 5. CSRF Review

Solution-wide searches were performed for:

- `ValidateAntiForgeryToken`
- `Antiforgery`
- `[HttpPost`
- `MapPost`
- `HttpPut`
- `HttpPatch`
- `HttpDelete`

No matches were found during the searches.

This does not establish that CSRF protection is unnecessary or that every possible state-changing endpoint has been identified.

CSRF protection must be reassessed when authenticated request submission, assignment and status-update functionality is integrated.

## 6. Limitations

- HTTPS was tested only in the local development environment.
- No production TLS certificate or deployment configuration was tested.
- Authentication cookie attributes were inspected in source code but not verified through a real sign-in response.
- CSRF protection remains an outstanding security consideration.
- Production cookie encryption key storage and rotation were not verified.

## 7. Conclusion

Local HTTPS redirection and HTTPS connectivity tests passed.

Existing authentication cookie configuration includes HttpOnly, Secure and SameSite settings.

These results provide M4 deployment-security evidence but do not establish production readiness.

**Overall Result: PASS — Local HTTPS verification and cookie configuration review.**

