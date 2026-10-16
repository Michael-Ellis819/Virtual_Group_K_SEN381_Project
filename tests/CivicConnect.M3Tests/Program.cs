using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using CivicConnect;
using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Data.Entities;
using CivicConnect.Domain;
using CivicConnect.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

int passed = 0;
void Check(string id, bool ok)
{
    if (!ok) throw new InvalidOperationException("FAIL " + id);
    Console.WriteLine("PASS " + id); passed++;
}
ClaimsPrincipal User(string role = "Staff", string id = "staff-1", string category = "IT Support")
    => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id),
        new Claim(ClaimTypes.Role, role), new Claim("active", "true"),
        new Claim("category", category) }, "m3-test-ticket"));
var states = new StatusTransitionService();
string[] names = ["New", "Assigned", "In Progress", "Resolved", "Closed", "Rejected"];
var allowed = new HashSet<(string, string)>
    { ("New", "Assigned"), ("New", "Rejected"), ("Assigned", "In Progress"),
      ("In Progress", "Resolved"), ("Resolved", "Closed") };
Check("C01 legal lifecycle edges [FR-010 TC-ST-01/03]", allowed.All(p => states.IsAllowed(p.Item1, p.Item2)));
Check("C02 all 31 prohibited edges and unknown state [FR-010 TC-ST-02/05]",
    names.All(a => names.All(b => states.IsAllowed(a, b) == allowed.Contains((a, b)))) &&
    !states.IsAllowed("Unknown", "Assigned") && !states.IsAllowed("New", "Unknown"));
var spy = new SpyStore();
var coordinator = new RequestStatusService(spy, new RequestAccessPolicy(), states, spy);
Check("C03 direct requester mutation denied before writing [NFR-001 TC-ST-06]",
    await coordinator.ChangeAsync(User("Requester", "requester-1"), spy.Request.RequestId, "New", "Assigned", "Assigned") == StatusChangeResult.NotFound && spy.Writes == 0);
Check("C04 invalid transition does not call writer [FR-010 TC-ST-08/10/11]",
    await coordinator.ChangeAsync(User(), spy.Request.RequestId, "New", "Closed", "Closed") == StatusChangeResult.Invalid && spy.Writes == 0);
spy.Conflict = true;
Check("C05 optimistic conflict maps to conflict [R-050 TC-ST-14]",
    await coordinator.ChangeAsync(User(), spy.Request.RequestId, "New", "Assigned", "Assigned") == StatusChangeResult.Conflict);
spy.Conflict = false;
Check("C06 legal operation passes actor, version and feedback [FR-010 TC-ST-07/09]",
    await coordinator.ChangeAsync(User(), spy.Request.RequestId, "New", "Assigned", "Assigned") == StatusChangeResult.Saved &&
    spy.Last is { ActorId: "staff-1", ExpectedVersion: 1, PreviousStatus: "New", NewStatus: "Assigned", Feedback: "Assigned" });

if (!args.Contains("--database"))
{
    Console.WriteLine($"RESULT {passed} component cases passed; database cases NOT RUN (use --database).");
    return;
}
// No user-secrets fallback: only the explicitly supplied disposable test database.
var connection = Environment.GetEnvironmentVariable("ConnectionStrings__CivicConnect")
    ?? throw new InvalidOperationException("Set ConnectionStrings__CivicConnect to a dedicated M3 test database.");
if (new Npgsql.NpgsqlConnectionStringBuilder(connection).Database?.EndsWith("_test", StringComparison.Ordinal) != true)
    throw new InvalidOperationException("M3 fault tests require a disposable database whose name ends in _test.");
var options = new DbContextOptionsBuilder<CivicConnectDbContext>().UseNpgsql(connection).Options;
await using (var setup = new CivicConnectDbContext(options)) await setup.Database.MigrateAsync();
using var rsa = RSA.Create(2048);
var certificateRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using var transientCertificate = certificateRequest.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(1));
// Reimport for Windows Schannel, which cannot use the ephemeral CNG key directly.
using var certificate = new X509Certificate2(transientCertificate.Export(X509ContentType.Pfx), "",
    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    { ApplicationName = typeof(Composition).Assembly.FullName, EnvironmentName = "Staging" });
builder.Logging.ClearProviders();
builder.Configuration["ConnectionStrings:CivicConnect"] = connection;
builder.Services.AddCivicConnect(builder.Configuration);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.WebHost.ConfigureKestrel(server => server.Listen(IPAddress.Loopback, 0, listen => listen.UseHttps(certificate)));
await using var app = builder.Build();
app.UseCivicConnect();
await app.StartAsync();
var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var client = new HttpClient(new HttpClientHandler
    { AllowAutoRedirect = false, UseCookies = false,
      ServerCertificateCustomValidationCallback = (_, cert, _, _) => cert?.Thumbprint == certificate.Thumbprint }) { BaseAddress = new Uri(address) };
var cookieOptions = app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
var requestIds = new List<Guid>();
string Cookie(ClaimsPrincipal user) => cookieOptions.Cookie.Name + "=" + cookieOptions.TicketDataFormat.Protect(
    new AuthenticationTicket(user, new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme));
async Task<HttpResponseMessage> Get(Guid id, ClaimsPrincipal user)
{
    using var message = new HttpRequestMessage(HttpMethod.Get, "/requests/" + id);
    message.Headers.Add("Cookie", Cookie(user));
    return await client.SendAsync(message);
}
async Task<HttpResponseMessage> Post(Guid id, string expected, string? target, string? feedback,
    ClaimsPrincipal? user = null, bool csrf = true)
{
    var cookie = user is null ? "" : Cookie(user);
    string? token = null;
    if (csrf && user is not null)
    {
        using var tokenMessage = new HttpRequestMessage(HttpMethod.Get, "/requests/antiforgery");
        tokenMessage.Headers.Add("Cookie", cookie);
        using var response = await client.SendAsync(tokenMessage);
        response.EnsureSuccessStatusCode();
        token = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("requestToken").GetString();
        cookie += "; " + response.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
    }
    using var message = new HttpRequestMessage(HttpMethod.Post, $"/requests/{id}/status");
    message.Content = JsonContent.Create(new { expectedStatus = expected, targetStatus = target, feedback });
    if (cookie.Length > 0) message.Headers.Add("Cookie", cookie);
    if (token is not null) message.Headers.Add("X-CSRF-TOKEN", token);
    return await client.SendAsync(message);
}
async Task<Guid> Seed(string status = "New")
{
    var id = Guid.NewGuid(); requestIds.Add(id);
    await using var db = new CivicConnectDbContext(options);
    db.ServiceRequests.Add(new ServiceRequestEntity { RequestId = id, ReferenceNumber = "M3-" + id.ToString("N"),
        RequesterId = "requester-1", CategoryId = "IT Support", Description = "Synthetic M3 fixture", Location = "Test lab",
        Status = status, UpdatedAt = DateTimeOffset.UtcNow,
        Notes = [new RequestNoteEntity { NoteId = Guid.NewGuid(), Content = "PRIVATE-M3-MARKER", IsRequesterVisible = false }] });
    await db.SaveChangesAsync(); return id;
}
async Task<bool> Unchanged(Guid id)
{
    await using var db = new CivicConnectDbContext(options);
    return await db.ServiceRequests.AnyAsync(r => r.RequestId == id && r.Status == "New" && r.Version == 1) &&
        await db.RequestNotes.CountAsync(n => n.RequestId == id) == 1 &&
        !await db.RequestStatusHistory.AnyAsync(h => h.RequestId == id);
}
try
{
    Console.WriteLine("ENVIRONMENT Staging; Release test host; HTTPS loopback; PostgreSQL; synthetic identities issued only by test executable.");
    Console.WriteLine("ASSEMBLY " + typeof(Composition).Assembly.GetName().Version);
    var staff = User(); var owner = User("Requester", "requester-1");
    var id = await Seed();
    using (var response = await Post(id, "New", "Assigned", "X", staff))
    {
        await using var db = new CivicConnectDbContext(options);
        Check("BB01 feedback length 1 accepted [BVA API contract FR-010]", response.StatusCode == HttpStatusCode.NoContent &&
            await db.RequestNotes.AnyAsync(n => n.RequestId == id && n.Content == "X"));
    }
    id = await Seed();
    using (var response = await Post(id, "New", "Assigned", new string('X', 2000), staff))
    {
        await using var db = new CivicConnectDbContext(options);
        Check("BB02 feedback length 2000 accepted [BVA API contract]", response.StatusCode == HttpStatusCode.NoContent &&
            await db.RequestNotes.AnyAsync(n => n.RequestId == id && n.Content.Length == 2000));
    }
    id = await Seed();
    using (var response = await Post(id, "New", "Assigned", new string('X', 2001), staff))
        Check("BB03 feedback length 2001 rejected with no writes [BVA API contract]", response.StatusCode == HttpStatusCode.BadRequest && await Unchanged(id));
    using (var response = await Post(id, "New", "Assigned", "", staff))
    using (var spaces = await Post(id, "New", "Assigned", "   ", staff))
    using (var unknown = await Post(id, "New", "Unknown", "Reason", staff))
    using (var missingTarget = await Post(id, "New", null, "Reason", staff))
        Check("BB04 empty whitespace and unknown target rejected [EP TC-ST-04/05]", response.StatusCode == HttpStatusCode.BadRequest &&
            spaces.StatusCode == HttpStatusCode.BadRequest && unknown.StatusCode == HttpStatusCode.BadRequest &&
            missingTarget.StatusCode == HttpStatusCode.BadRequest && await Unchanged(id));
    using (var response = await Post(id, "New", "Assigned", "Reason", owner))
    using (var otherCategory = await Post(id, "New", "Assigned", "Reason", User(category: "Maintenance")))
        Check("BB05 requester and wrong-category writes denied [EP NFR-001 TC-ST-06]", response.StatusCode == HttpStatusCode.NotFound &&
            otherCategory.StatusCode == HttpStatusCode.NotFound && await Unchanged(id));
    using (var response = await Post(id, "New", "Assigned", "Reason", staff, false))
        Check("A01 missing antiforgery rejected [security contract]", response.StatusCode == HttpStatusCode.BadRequest && await Unchanged(id));
    using (var response = await Post(id, "New", "Assigned", "Reason"))
        Check("A02 signed-out mutation denied [FR-016 partial]", response.StatusCode == HttpStatusCode.Unauthorized && await Unchanged(id));
    using (var response = await Post(id, "Assigned", "In Progress", "Reason", staff))
        Check("A03 stale expected status returns 409 [R-050]", response.StatusCode == HttpStatusCode.Conflict && await Unchanged(id));
    using (var missing = await Post(Guid.NewGuid(), "New", "Assigned", "Reason", staff))
    using (var denied = await Post(id, "New", "Assigned", "Reason", owner))
        Check("A04 missing and inaccessible share 404 title [NFR-001]", missing.StatusCode == HttpStatusCode.NotFound &&
            denied.StatusCode == HttpStatusCode.NotFound && (await missing.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("title").GetString() ==
            (await denied.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("title").GetString());
    // Controlled database fault: temporarily constrain ONLY this generated fixture.
    await using (var db = new CivicConnectDbContext(options))
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" ADD CONSTRAINT m3_fault CHECK (\"Description\" <> 'Synthetic M3 fixture' OR \"Status\" <> 'Assigned') NOT VALID");
    try
    {
        using var fault = await Post(id, "New", "Assigned", "Reason", staff);
        var text = await fault.Content.ReadAsStringAsync();
        Check("A05 database failure gives safe 500 and rolls back [NFR-003/008 TC-ST-13]", fault.StatusCode == HttpStatusCode.InternalServerError &&
            !text.Contains("m3_fault") && !text.Contains("PRIVATE-M3-MARKER") && !text.Contains("Exception") && await Unchanged(id));
    }
    finally
    {
        await using var db = new CivicConnectDbContext(options);
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS m3_fault");
    }
    id = await Seed();
    foreach (var edge in new[] { ("New", "Assigned"), ("Assigned", "In Progress"), ("In Progress", "Resolved"), ("Resolved", "Closed") })
    {
        using var result = await Post(id, edge.Item1, edge.Item2, "Public update: " + edge.Item2, staff);
        if (result.StatusCode != HttpStatusCode.NoContent) throw new InvalidOperationException("E01 transition failed");
    }
    using (var read = await Get(id, owner))
    {
        var text = await read.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(text).RootElement;
        await using var db = new CivicConnectDbContext(options);
        var history = await db.RequestStatusHistory.Where(h => h.RequestId == id).ToListAsync();
        Check("E01 staff lifecycle to requester read and durable audit [FR-003/005/010/012/015]", read.StatusCode == HttpStatusCode.OK &&
            json.GetProperty("status").GetString() == "Closed" && json.GetProperty("updatedAt").GetDateTimeOffset() > DateTimeOffset.UtcNow.AddMinutes(-5) &&
            json.GetProperty("notes").GetArrayLength() == 4 && !text.Contains("PRIVATE-M3-MARKER") &&
            history.Count == 4 && history.All(h => h.ChangedByUserId == "staff-1" && h.ChangedAt > DateTimeOffset.UtcNow.AddMinutes(-5) &&
                allowed.Contains((h.PreviousStatus, h.NewStatus)) && h.NewStatus != "Rejected") &&
            history.Select(h => (h.PreviousStatus, h.NewStatus)).Distinct().Count() == 4 &&
            await db.ServiceRequests.AnyAsync(r => r.RequestId == id && r.Version == 5));
    }
    id = await Seed();
    using (var emptyReason = await Post(id, "New", "Rejected", "", staff))
    using (var rejected = await Post(id, "New", "Rejected", "Duplicate request", staff))
    using (var reopen = await Post(id, "Rejected", "New", "Reopen", staff))
    using (var read = await Get(id, owner))
    using (var foreignRead = await Get(id, User("Requester", "requester-2")))
    {
        var text = await read.Content.ReadAsStringAsync();
        await using var db = new CivicConnectDbContext(options);
        Check("E02 reject with reason; own feedback; no reopening or foreign disclosure [FR-003/005/010]", emptyReason.StatusCode == HttpStatusCode.BadRequest &&
            rejected.StatusCode == HttpStatusCode.NoContent && reopen.StatusCode == HttpStatusCode.BadRequest && read.StatusCode == HttpStatusCode.OK &&
            text.Contains("Rejected") && text.Contains("Duplicate request") && !text.Contains("PRIVATE-M3-MARKER") &&
            foreignRead.StatusCode == HttpStatusCode.NotFound && await db.RequestStatusHistory.CountAsync(h => h.RequestId == id) == 1);
    }
    id = await Seed();
    await using (var changed = new CivicConnectDbContext(options))
    {
        var entity = await changed.ServiceRequests.SingleAsync(r => r.RequestId == id);
        entity.CategoryId = "Maintenance";
        await changed.SaveChangesAsync();
    }
    bool staleVersionDenied = false;
    await using (var writerDb = new CivicConnectDbContext(options))
    {
        try { await new EfRequestStatusWriter(writerDb).SaveAsync(new(id, "New", "Assigned", "staff-1", "Stale", 1), CancellationToken.None); }
        catch (DbUpdateConcurrencyException) { staleVersionDenied = true; }
    }
    await using (var verify = new CivicConnectDbContext(options))
        Check("A06 changed category/version prevents stale authorised write [R-049/050]", staleVersionDenied &&
            await verify.ServiceRequests.AnyAsync(r => r.RequestId == id && r.Status == "New" && r.CategoryId == "Maintenance" && r.Version == 2) &&
            !await verify.RequestStatusHistory.AnyAsync(h => h.RequestId == id) && await verify.RequestNotes.CountAsync(n => n.RequestId == id) == 1);
    Console.WriteLine($"RESULT {passed} cases passed; 0 failed. E01/E02 cover the status-management API slice, not login/submission/reporting UI.");
}
finally
{
    await app.StopAsync();
    await using var cleanup = new CivicConnectDbContext(options);
    await cleanup.ServiceRequests.Where(r => requestIds.Contains(r.RequestId)).ExecuteDeleteAsync();
}

sealed class SpyStore : IRequestReader, IRequestStatusWriter
{
    public RequestSnapshot Request { get; } = new(Guid.NewGuid(), "M3-SPY", "requester-1", "IT Support", "Fixture", "Lab", "New", DateTimeOffset.UtcNow, []);
    public int Writes { get; private set; }
    public bool Conflict { get; set; }
    public StatusChange? Last { get; private set; }
    public Task<RequestSnapshot?> FindAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RequestSnapshot?>(id == Request.RequestId ? Request : null);
    public Task SaveAsync(StatusChange change, CancellationToken cancellationToken)
    {
        if (Conflict) throw new DbUpdateConcurrencyException("Test conflict");
        Last = change; Writes++; return Task.CompletedTask;
    }
}
