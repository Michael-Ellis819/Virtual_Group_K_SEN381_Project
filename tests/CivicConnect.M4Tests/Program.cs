using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CivicConnect;
using CivicConnect.Application;
using CivicConnect.Domain;
using CivicConnect.Lifecycle;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;

var passed = 0;
void Check(string id, bool condition) { if (!condition) throw new InvalidOperationException("FAIL " + id); Console.WriteLine("PASS " + id); passed++; }
var preview = args.Contains("--preview");
var fixture = new Fixture();
using var rsa = RSA.Create(2048);
var certificateRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using var transient = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(2));
using var certificate = new X509Certificate2(transient.Export(X509ContentType.Pfx), "", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

async Task<WebApplication> Start(
    bool adapters,
    bool includeAccounts = true)
{
    var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = typeof(Composition).Assembly.FullName, EnvironmentName = "Staging" });
    builder.Logging.ClearProviders();
    builder.Configuration["ConnectionStrings:CivicConnect"] = "Host=127.0.0.1;Database=unused_ui_contract_test;Username=unused";
    builder.Services.AddCivicConnect(builder.Configuration);
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
    builder.Services.AddSingleton<IRequestReader>(fixture);
    builder.Services.AddSingleton<IRequestStatusWriter>(fixture);
    builder.Services.RemoveAll<IPortalAccounts>();
    builder.Services.RemoveAll<IPortalRequestStore>();
    if (includeAccounts)
    {
        builder.Services.AddSingleton<IPortalAccounts>(fixture);
    }

    if (adapters)
    {
        builder.Services.AddSingleton<IPortalRequestStore>(fixture);
    }
    if (preview)
    {
        builder.WebHost.UseUrls("http://127.0.0.1:5089");
        // Only the separate synthetic preview host uses HTTP; runtime cookies remain Secure Always.
        builder.Services.PostConfigure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, o => o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None);
        builder.Services.Configure<AntiforgeryOptions>(o => o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None);
    }
    else builder.WebHost.ConfigureKestrel(s => s.Listen(IPAddress.Loopback, 0, l => l.UseHttps(certificate)));
    var app = builder.Build(); app.UseCivicConnect(); await app.StartAsync(); return app;
}
string Address(WebApplication app) => app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
HttpClient Client(WebApplication app) => new(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer(), ServerCertificateCustomValidationCallback = (_, cert, _, _) => cert?.Thumbprint == certificate.Thumbprint }) { BaseAddress = new Uri(Address(app)) };
string Token(string html)
{
    var input = Regex.Matches(html, "<input\\b[^>]*>").Select(m => m.Value).First(x => x.Contains("name=\"__RequestVerificationToken\""));
    return WebUtility.HtmlDecode(Regex.Match(input, "value=\"([^\"]+)\"").Groups[1].Value);
}
async Task<HttpResponseMessage> Form(HttpClient client, string path, Dictionary<string, string> fields, string? tokenPage = null)
{
    if (tokenPage is not null) fields["__RequestVerificationToken"] = Token(await client.GetStringAsync(tokenPage));
    return await client.PostAsync(path, new FormUrlEncodedContent(fields));
}
async Task Login(HttpClient client, string user)
{
    using var result = await Form(client, "/account/sign-in", new() { ["Username"] = user, ["Password"] = "synthetic-contract-password", ["role"] = "Management" }, "/account/sign-in");
    if (result.StatusCode != HttpStatusCode.Redirect) throw new InvalidOperationException("Fixture sign-in failed");
}
await using var app = await Start(true);
if (preview)
{
    Console.WriteLine("SYNTHETIC UI PREVIEW ONLY " + Address(app));
    Console.WriteLine("Fixture usernames: requester, staff, manager. Password: synthetic-contract-password");
    await app.WaitForShutdownAsync(); return;
}
using var requester = Client(app);
using var staff = Client(app);
using var manager = Client(app);
using var anonymous = Client(app);
try
{
    using var signedOut = await anonymous.GetAsync("/portal/requests");
    Check("UI01 signed-out portal access denied [FR-016]", signedOut.StatusCode == HttpStatusCode.Unauthorized);
    using var missingToken = await Form(anonymous, "/account/sign-in", new() { ["Username"] = "requester", ["Password"] = "synthetic-contract-password" });
    Check("UI02 sign-in requires antiforgery", missingToken.StatusCode == HttpStatusCode.BadRequest);
    using var invalidLogin = await Form(anonymous, "/account/sign-in", new() { ["Username"] = "requester", ["Password"] = "wrong" }, "/account/sign-in");
    Check("UI03 invalid credentials issue no session [EP FR-016]", invalidLogin.StatusCode == HttpStatusCode.OK && !invalidLogin.Headers.Contains("Set-Cookie") && (await invalidLogin.Content.ReadAsStringAsync()).Contains("Sign-in failed"));
    await Login(requester, "requester"); await Login(staff, "staff"); await Login(manager, "manager");
    using var spoofRole = await requester.GetAsync("/portal/management");
    Check("UI04 posted role cannot elevate requester [NFR-001]", spoofRole.StatusCode == HttpStatusCode.Forbidden);
    var signInHtml = await anonymous.GetStringAsync("/account/sign-in");
    Check("UI05 password is never echoed into sign-in HTML", !signInHtml.Contains("synthetic-contract-password"));
    var before = fixture.Writes;
    using var invalidCategory = await Form(requester, "/portal/requests/new", new() { ["Category"] = "Unlisted", ["Description"] = "Issue", ["Location"] = "Lab", ["SubmissionKey"] = Guid.NewGuid().ToString() }, "/portal/requests/new");
    Check("BB-M4-01 unlisted category rejected without write [FR-001/002 EP]", invalidCategory.StatusCode == HttpStatusCode.BadRequest && fixture.Writes == before);
    foreach (var length in new[] { 0, 2001 })
    {
        using var invalidDescription = await Form(requester, "/portal/requests/new", new() { ["Category"] = "IT Support", ["Description"] = new('x', length), ["Location"] = "Lab", ["SubmissionKey"] = Guid.NewGuid().ToString() }, "/portal/requests/new");
        Check($"BB-M4-02 description length {length} rejected without write [BVA]", invalidDescription.StatusCode == HttpStatusCode.BadRequest && fixture.Writes == before);
    }
    using var longLocation = await Form(requester, "/portal/requests/new", new() { ["Category"] = "IT Support", ["Description"] = "Issue", ["Location"] = new('x', 251), ["SubmissionKey"] = Guid.NewGuid().ToString() }, "/portal/requests/new");
    Check("BB-M4-03 location 251 rejected without write [BVA]", longLocation.StatusCode == HttpStatusCode.BadRequest && fixture.Writes == before);
    var submissionKey = Guid.NewGuid().ToString();
    var values = new Dictionary<string, string> { ["Category"] = "IT Support", ["Description"] = new('x', 2000), ["Location"] = new('y', 250), ["SubmissionKey"] = submissionKey };
    using var submitted = await Form(requester, "/portal/requests/new", values, "/portal/requests/new");
    Check("BB-M4-04 boundary 2000/250 accepted through store contract [BVA]", submitted.StatusCode == HttpStatusCode.Redirect);
    var detailPath = submitted.Headers.Location!.ToString();
    var id = Guid.Parse(detailPath.Split('/').Last());
    using var duplicate = await Form(requester, "/portal/requests/new", values, "/portal/requests/new");
    Check("UI06 resubmission preserves submission key and returns same record [contract only]", duplicate.Headers.Location?.ToString() == detailPath && fixture.Writes == before + 1);
    var ownList = await requester.GetStringAsync("/portal/requests");
    Check("UI07 list excludes other requester's record [FR-004]", ownList.Contains(fixture.Rows[id].Reference) && !ownList.Contains("OTHER-REQUESTER"));
    using var foreign = await requester.GetAsync("/portal/requests/" + fixture.ForeignId);
    Check("UI08 direct foreign detail denied [FR-003]", foreign.StatusCode == HttpStatusCode.NotFound);
    using var filtered = await staff.GetAsync("/portal/requests?From=2026-10-18&To=2026-10-18&Status=New&Category=IT%20Support&Priority=Normal&Sort=priority-desc");
    Check("BB-M4-05 combined filters preserve inclusive UTC boundaries [FR-007]", filtered.StatusCode == HttpStatusCode.OK && fixture.LastFilter is { Status: "New", Category: "IT Support", Priority: "Normal", Sort: "priority-desc" } && fixture.LastFilter.From == DateTimeOffset.Parse("2026-10-18T00:00:00Z") && fixture.LastFilter.To == DateTimeOffset.Parse("2026-10-18T23:59:59.9999999Z"));
    using var reversedDates = await staff.GetAsync("/portal/requests?From=2026-10-19&To=2026-10-18");
    Check("BB-M4-06 reversed dates rejected [EP]", reversedDates.StatusCode == HttpStatusCode.BadRequest);
    using var wrongCategory = Client(app); await Login(wrongCategory, "other-staff");
    using var denied = await Form(wrongCategory, detailPath + "/assign", new() { ["OwnerId"] = "staff-1", ["ExpectedVersion"] = "1" }, "/portal/requests");
    Check("UI09 staff category boundary enforced before assignment", denied.StatusCode == HttpStatusCode.NotFound);
    using var assign = await Form(staff, detailPath + "/assign", new() { ["OwnerId"] = "staff-1", ["ExpectedVersion"] = "1" }, detailPath);
    Check("UI10 assignment calls ownership and transition rules before store [FR-009]", assign.StatusCode == HttpStatusCode.Redirect && fixture.Rows[id].Owner == "staff-1" && fixture.Rows[id].Status == "Assigned");
    using var stale = await Form(staff, detailPath + "/schedule", new() { ["Priority"] = "High", ["ExpectedVersion"] = "1" }, detailPath);
    Check("UI11 stale write reports conflict", stale.StatusCode == HttpStatusCode.Conflict);
    using var progress = await Form(staff, detailPath + "/status", new() { ["ExpectedStatus"] = "Assigned", ["TargetStatus"] = "In Progress", ["Feedback"] = "Work started" }, detailPath);
    using var blankResolve = await Form(staff, detailPath + "/status", new() { ["ExpectedStatus"] = "In Progress", ["TargetStatus"] = "Resolved", ["Feedback"] = " " }, detailPath);
    Check("BB-M4-07 blank resolution summary rejected [FR-012 EP]", blankResolve.StatusCode == HttpStatusCode.BadRequest && fixture.Rows[id].Status == "In Progress");
    using var note = await Form(staff, detailPath + "/notes", new() { ["Content"] = "<script>private-marker</script>", ["ExpectedVersion"] = fixture.Rows[id].Version.ToString() }, detailPath);
    Check("UI12 internal note uses explicit private visibility", note.StatusCode == HttpStatusCode.Redirect);
    var requesterDetails = await requester.GetStringAsync(detailPath);
    var staffDetails = await staff.GetStringAsync(detailPath);
    Check("UI13 private notes excluded for requester and HTML encoded for staff [FR-011]", !requesterDetails.Contains("private-marker") && staffDetails.Contains("&lt;script&gt;private-marker&lt;/script&gt;") && !staffDetails.Contains("<script>private-marker"));
    using var schedule = await Form(staff, detailPath + "/schedule", new() { ["Priority"] = "High", ["DueAt"] = "2026-10-18T14:30", ["ExpectedVersion"] = fixture.Rows[id].Version.ToString() }, detailPath);
    Check("UI14 due date labelled UTC is passed as UTC [FR-017/018]", schedule.StatusCode == HttpStatusCode.Redirect && fixture.Rows[id].Due == DateTimeOffset.Parse("2026-10-18T14:30:00Z"));
    using var resolve = await Form(staff, detailPath + "/status", new() { ["ExpectedStatus"] = "In Progress", ["TargetStatus"] = "Resolved", ["Feedback"] = "Repaired and checked" }, detailPath);
    using var close = await Form(staff, detailPath + "/status", new() { ["ExpectedStatus"] = "Resolved", ["TargetStatus"] = "Closed", ["Feedback"] = "Closed after confirmation" }, detailPath);
    Check("E2E-M4-01 sign-in submit assign progress resolve close [synthetic store, not deployed acceptance]", progress.StatusCode == HttpStatusCode.Redirect && resolve.StatusCode == HttpStatusCode.Redirect && close.StatusCode == HttpStatusCode.Redirect && fixture.Rows[id].Status == "Closed" && (await requester.GetStringAsync(detailPath)).Contains("Closed after confirmation"));
    using var reopen = await Form(staff, detailPath + "/status", new() { ["ExpectedStatus"] = "Closed", ["TargetStatus"] = "New", ["Feedback"] = "Reopen" }, detailPath);
    Check("UI15 terminal status cannot reopen", reopen.StatusCode == HttpStatusCode.BadRequest);
    using var noToken = await Form(staff, detailPath + "/notes", new() { ["Content"] = "No token", ["ExpectedVersion"] = fixture.Rows[id].Version.ToString() });
    Check("UI16 note mutation requires antiforgery", noToken.StatusCode == HttpStatusCode.BadRequest);
    var overview = await manager.GetStringAsync("/portal/management?category=IT%20Support&status=Closed");
    Check("UI17 management filtered counts use Bianca calculator [FR-013/014]", overview.Contains("Closed<strong>1</strong>") && overview.Contains("Open<strong>0</strong>"));
    fixture.RequesterActive = false;
    using var revoked = await requester.GetAsync("/portal/requests");
    Check("UI18 disabled browser account loses access on next request", revoked.StatusCode == HttpStatusCode.Unauthorized);
    // Restore the synthetic requester for the remaining tests.
    fixture.RequesterActive = true;
    // UI19: Verify that sign-in fails safely without an account adapter.
    await using var noAccounts = await Start(false, false);
    using var noAccountsClient = Client(noAccounts);

    using var unavailableLogin = await Form(
        noAccountsClient,
        "/account/sign-in",
        new()
        {
            ["Username"] = "requester",
            ["Password"] = "synthetic-contract-password"
        },
        "/account/sign-in");

    Check(
        "UI19 missing real account adapter returns 503 and issues no session",
        unavailableLogin.StatusCode == HttpStatusCode.ServiceUnavailable &&
        !unavailableLogin.Headers.Contains("Set-Cookie"));
    await using var unbound = await Start(false);
    // Allow valid authentication while leaving the request-store adapter unavailable.
    using var unavailable = Client(unbound);
    // Minting is confined to this executable to inspect unavailable protected pages without a real account adapter.
    var cookieOptions = unbound.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
    string SyntheticCookie(string role) =>
        cookieOptions.Cookie.Name + "=" +
        cookieOptions.TicketDataFormat.Protect(
            new AuthenticationTicket(
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [
                            new Claim(
                                ClaimTypes.NameIdentifier,
                                role == "Management" ? "manager-1" : "requester-1"),
                            new Claim(ClaimTypes.Role, role),
                            new Claim("active", "true"),
                            new Claim("portal-stamp", "fixture-v1")
                        ],
                        "fixture")),
                new AuthenticationProperties(),
                CookieAuthenticationDefaults.AuthenticationScheme));
    using var missingStoreList = new HttpRequestMessage(HttpMethod.Get, "/portal/requests");
    missingStoreList.Headers.Add("Cookie", SyntheticCookie("Requester"));
    using var unavailableList = await unavailable.SendAsync(missingStoreList);
    Check("UI20 missing request store returns unavailable, not an empty accepted list",
        unavailableList.StatusCode == HttpStatusCode.ServiceUnavailable &&
        (await unavailableList.Content.ReadAsStringAsync())
        .Contains("temporarily unavailable"));
    using var missingStoreOverview = new HttpRequestMessage(HttpMethod.Get, "/portal/management");
    missingStoreOverview.Headers.Add("Cookie", SyntheticCookie("Management"));
    using var unavailableOverview = await unavailable.SendAsync(missingStoreOverview);
    Check("UI21 missing reporting store does not display false zero counts",
        unavailableOverview.StatusCode == HttpStatusCode.ServiceUnavailable &&
        !(await unavailableOverview.Content.ReadAsStringAsync())
            .Contains("Open<strong>0</strong>"));
    await unbound.StopAsync();
    Console.WriteLine($"RESULT {passed} UI/contract cases passed; 0 failed. Synthetic in-memory adapters only; no persistence, real-user usability or deployed E2E acceptance claimed.");
}
finally { await app.StopAsync(); }

sealed class Fixture : IRequestReader, IRequestStatusWriter, IPortalAccounts, IPortalRequestStore
{
    public Dictionary<Guid, Row> Rows { get; } = [];
    private readonly Dictionary<Guid, Guid> submissions = [];
    public Guid ForeignId { get; } = Guid.NewGuid();
    public int Writes { get; private set; }
    public bool RequesterActive { get; set; } = true;
    public PortalFilter? LastFilter { get; private set; }
    public Fixture() => Rows[ForeignId] = new(ForeignId, "OTHER-REQUESTER", "requester-2", "Maintenance", "Foreign issue", "Other lab");
    public Task<SignInResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        var id = username switch { "requester" => "requester-1", "staff" => "staff-1", "other-staff" => "staff-2", "manager" => "manager-1", _ => "" };
        var account = Account(id);
        return Task.FromResult(password == "synthetic-contract-password" && account is not null ? new SignInResult(SignInOutcome.Authenticated, account) : new SignInResult(SignInOutcome.InvalidCredentials));
    }
    private PortalAccount? Account(string id) => id switch
    {
        "requester-1" => new(id, "Synthetic requester", "Requester", RequesterActive, [], "fixture-v1"),
        "staff-1" => new(id, "Synthetic staff", "Staff", true, ["IT Support"], "fixture-v1"),
        "staff-2" => new(id, "Other staff", "Staff", true, ["Maintenance"], "fixture-v1"),
        "manager-1" => new(id, "Synthetic management", "Management", true, [], "fixture-v1"), _ => null
    };
    Task<PortalAccount?> IPortalAccounts.FindAsync(string id, CancellationToken cancellationToken) => Task.FromResult(Account(id));
    public Task<RequestSnapshot?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!Rows.TryGetValue(id, out var r)) return Task.FromResult<RequestSnapshot?>(null);
        return Task.FromResult<RequestSnapshot?>(new(r.Id, r.Reference, r.Requester, r.Category, r.Description, r.Location, r.Status, r.Updated,
            r.Notes.Select(n => new RequestNote(n.Content, n.IsPublic)).ToArray(), r.Version));
    }
    public Task SaveAsync(StatusChange change, CancellationToken cancellationToken)
    {
        var r = Rows[change.RequestId];
        if (r.Version != change.ExpectedVersion || r.Status != change.PreviousStatus) throw new DbUpdateConcurrencyException();
        r.Status = change.NewStatus; r.Updated = DateTimeOffset.UtcNow; r.Version++;
        r.Notes.Add(new(change.Feedback, change.ActorId, r.Updated, true)); r.History.Add(new(change.NewStatus, change.ActorId, r.Updated)); Writes++;
        return Task.CompletedTask;
    }
    private static bool CanRead(PortalActor actor, Row row) => actor.Role == "Requester" ? row.Requester == actor.Id : actor.Role == "Staff" && actor.Categories.Contains(row.Category);
    public Task<IReadOnlyList<PortalRow>> ListAsync(PortalActor actor, PortalFilter filter, CancellationToken cancellationToken)
    {
        LastFilter = filter;
        return Task.FromResult<IReadOnlyList<PortalRow>>(Rows.Values.Where(r => CanRead(actor, r)).Select(r => new PortalRow(r.Id, r.Reference, r.Category, r.Created, r.Status, r.Priority, r.Owner, r.Due)).ToArray());
    }
    public Task<PortalMetadata?> MetadataAsync(PortalActor actor, Guid requestId, CancellationToken cancellationToken)
    {
        if (!Rows.TryGetValue(requestId, out var r) || !CanRead(actor, r)) return Task.FromResult<PortalMetadata?>(null);
        return Task.FromResult<PortalMetadata?>(new(r.Requester, r.Priority, r.Owner, r.Due, r.Notes, r.History));
    }
    public Task<PortalWriteResult> SubmitAsync(PortalActor actor, PortalSubmission submission, CancellationToken cancellationToken)
    {
        if (submissions.TryGetValue(submission.SubmissionKey, out var old)) return Task.FromResult(new PortalWriteResult(PortalWriteOutcome.Saved, old));
        var id = Guid.NewGuid(); Rows[id] = new(id, "CC-" + id.ToString("N"), actor.Id, submission.Category, submission.Description, submission.Location);
        submissions[submission.SubmissionKey] = id; Writes++;
        return Task.FromResult(new PortalWriteResult(PortalWriteOutcome.Saved, id));
    }
    public Task<PortalWriteResult> AssignAsync(PortalActor actor, Guid requestId, string ownerId, long expectedVersion, ValidatedStatusChange change, CancellationToken cancellationToken)
        => Write(actor, requestId, expectedVersion, r => { r.Owner = ownerId; r.Status = change.NewStatus; });
    public Task<PortalWriteResult> AddNoteAsync(PortalActor actor, Guid requestId, string content, bool isPublic, long expectedVersion, CancellationToken cancellationToken)
        => Write(actor, requestId, expectedVersion, r => r.Notes.Add(new(content, actor.Id, DateTimeOffset.UtcNow, isPublic)));
    public Task<PortalWriteResult> SetScheduleAsync(PortalActor actor, Guid requestId, string priority, DateTimeOffset? dueAt, long expectedVersion, CancellationToken cancellationToken)
        => Write(actor, requestId, expectedVersion, r => { r.Priority = priority; r.Due = dueAt; });
    private Task<PortalWriteResult> Write(PortalActor actor, Guid id, long expectedVersion, Action<Row> change)
    {
        if (!Rows.TryGetValue(id, out var r) || actor.Role != "Staff" || !CanRead(actor, r)) return Task.FromResult(new PortalWriteResult(PortalWriteOutcome.NotFound));
        if (r.Version != expectedVersion) return Task.FromResult(new PortalWriteResult(PortalWriteOutcome.Conflict));
        change(r); r.Version++; r.Updated = DateTimeOffset.UtcNow; Writes++; return Task.FromResult(new PortalWriteResult(PortalWriteOutcome.Saved, id));
    }
    public Task<IReadOnlyList<PortalActivity>> ActivityAsync(PortalActor actor, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PortalActivity>>(actor.Role == "Management" ? Rows.Values.Select(r => new PortalActivity(r.Category, r.Status, r.Due)).ToArray() : []);
    public sealed class Row(Guid id, string reference, string requester, string category, string description, string location)
    {
        public Guid Id { get; } = id; public string Reference { get; } = reference; public string Requester { get; } = requester;
        public string Category { get; } = category; public string Description { get; } = description; public string Location { get; } = location;
        public DateTimeOffset Created { get; } = DateTimeOffset.UtcNow; public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = "New"; public long Version { get; set; } = 1; public string Priority { get; set; } = "Normal";
        public string? Owner { get; set; } public DateTimeOffset? Due { get; set; }
        public List<PortalNote> Notes { get; } = []; public List<PortalHistory> History { get; } = [];
    }
}
