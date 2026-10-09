using System.Net;
using System.Security.Claims;
using System.Text.Json;
using CivicConnect;
using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Domain;
using CivicConnect.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

int passed = 0;
void Check(string id, bool condition)
{
    if (!condition) throw new InvalidOperationException("FAIL " + id);
    Console.WriteLine("PASS " + id);
    passed++;
}
ClaimsPrincipal User(string role = "Requester", string id = "requester-1", string active = "true", string category = "IT Support")
    => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id),
        new Claim(ClaimTypes.Role, role), new Claim("active", active), new Claim("category", category) }, "test-ticket"));

var reader = new DevelopmentRequestReader();
var policy = new RequestAccessPolicy();
var service = new RequestQueryService(reader, policy);
var requestId = DevelopmentRequestReader.RequestId;
var requester = User();
var staff = User("Staff", "staff-1");
var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
var own = await service.ReadAsync(requester, requestId);
Check("S01 owner reads status and timestamp [FR-003]", own is { Status: "New" } && own.UpdatedAt == DateTimeOffset.Parse("2026-09-30T08:00:00Z"));
Check("S02 requester sees public notes only [NFR-001 FR-011 partial]", own!.Notes.SequenceEqual(new[] { "Request received." }));
Check("S03 another requester denied [AC-FR-003-2]", await service.ReadAsync(User(id: "requester-2"), requestId) is null);
Check("S04 staff category grant includes internal notes [FR-008]", (await service.ReadAsync(staff, requestId))!.Notes.Count == 2);
Check("S05 staff outside category denied [FR-008]", await service.ReadAsync(User("Staff", "staff-2", category: "Maintenance"), requestId) is null);
Check("S06 management detail access denied [NFR-001]", await service.ReadAsync(User("Management"), requestId) is null);
Check("S07 anonymous direct service caller denied [FR-016]", await service.ReadAsync(anonymous, requestId) is null);
Check("S08 inactive identity denied [NFR-001]", await service.ReadAsync(User(active: "false"), requestId) is null);
Check("S09 unknown role denied [NFR-001]", await service.ReadAsync(User("Admin"), requestId) is null);
var missingId = User(); ((ClaimsIdentity)missingId.Identity!).RemoveClaim(missingId.FindFirst(ClaimTypes.NameIdentifier)!);
Check("S10 missing user ID denied", await service.ReadAsync(missingId, requestId) is null);
var twoRoles = User(); ((ClaimsIdentity)twoRoles.Identity!).AddClaim(new Claim(ClaimTypes.Role, "Staff"));
Check("S11 ambiguous roles denied", await service.ReadAsync(twoRoles, requestId) is null);
var twoIdentities = User(); twoIdentities.AddIdentity(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Staff") }));
Check("S12 mixed identities denied", await service.ReadAsync(twoIdentities, requestId) is null);
Check("S13 missing record denied", await service.ReadAsync(requester, Guid.Parse("22222222-2222-2222-2222-222222222222")) is null);
Check("S14 empty ID denied", await service.ReadAsync(requester, Guid.Empty) is null);
using (var cts = new CancellationTokenSource())
{
    cts.Cancel(); bool cancelled = false;
    try { await service.ReadAsync(requester, requestId, cts.Token); }
    catch (OperationCanceledException) { cancelled = true; }
    Check("S15 read cancellation propagated", cancelled);
}

// Real loopback HTTP, MVC routing and cookie authentication. Tickets are minted
// only inside this test executable; the application has no test-login endpoint.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(Composition).Assembly.FullName,
    EnvironmentName = "Development"
});
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddCivicConnect(
    new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:CivicConnect"] =
                "Host=localhost;Database=civicconnect_dev;Username=postgres"
        })
        .Build());
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddSingleton<IRequestReader, FaultInjectingReader>();
await using var app = builder.Build();
app.UseCivicConnect();
await app.StartAsync();
try
{
    var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { BaseAddress = new Uri(address) };
    var options = app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
    async Task<HttpResponseMessage> Get(string path, ClaimsPrincipal? principal = null, string? overrideCookie = null)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, path);
        if (principal is not null || overrideCookie is not null)
        {
            var ticket = new AuthenticationTicket(principal ?? requester, new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
            message.Headers.Add("Cookie", options.Cookie.Name + "=" + (overrideCookie ?? options.TicketDataFormat.Protect(ticket)));
        }
        return await client.SendAsync(message);
    }
    var path = "/requests/" + requestId;
    using var home = await Get("/");
    Check("H01 Razor landing route renders", home.StatusCode == HttpStatusCode.OK && (await home.Content.ReadAsStringAsync()).Contains("CivicConnect"));
    using var signedOut = await Get(path);
    Check("H02 signed-out request returns 401 without redirect", signedOut.StatusCode == HttpStatusCode.Unauthorized && signedOut.Headers.Location is null);
    using var invalidCookie = await Get(path, overrideCookie: "tampered");
    Check("H03 tampered cookie returns 401", invalidCookie.StatusCode == HttpStatusCode.Unauthorized);
    using var response = await Get(path, requester);
    var text = await response.Content.ReadAsStringAsync();
    var json = JsonDocument.Parse(text).RootElement;
    Check("H04 owner receives JSON status [FR-003]", response.StatusCode == HttpStatusCode.OK && json.GetProperty("status").GetString() == "New");
    Check("H05 private fields absent from serialised response", !text.Contains("Private diagnostic") && !json.TryGetProperty("requesterId", out _));
    Check("H06 protected response is not cached", response.Headers.CacheControl?.NoStore == true);
    using var other = await Get(path, User(id: "requester-2"));
    using var missing = await Get("/requests/22222222-2222-2222-2222-222222222222", requester);
    var otherJson = JsonDocument.Parse(await other.Content.ReadAsStringAsync()).RootElement;
    var missingJson = JsonDocument.Parse(await missing.Content.ReadAsStringAsync()).RootElement;
    Check("H07 missing and forbidden share status and title", other.StatusCode == HttpStatusCode.NotFound && missing.StatusCode == HttpStatusCode.NotFound && otherJson.GetProperty("title").GetString() == missingJson.GetProperty("title").GetString());
    using var allowedStaff = await Get(path, staff);
    Check("H08 category-authorised staff get private notes", allowedStaff.StatusCode == HttpStatusCode.OK && (await allowedStaff.Content.ReadAsStringAsync()).Contains("Private diagnostic"));
    using var deniedStaff = await Get(path, User("Staff", "staff-2", category: "Maintenance"));
    Check("H09 unauthorised category returns 404", deniedStaff.StatusCode == HttpStatusCode.NotFound);
    using var malformed = await Get("/requests/not-a-uuid", requester);
    using var empty = await Get("/requests/00000000-0000-0000-0000-000000000000", requester);
    Check("H10 malformed and empty IDs return 400", malformed.StatusCode == HttpStatusCode.BadRequest && empty.StatusCode == HttpStatusCode.BadRequest);
    using var spoof = await Get(path + "?role=Staff&userId=requester-1&category=IT%20Support", User(id: "requester-2"));
    Check("H11 caller cannot elevate via query parameters", spoof.StatusCode == HttpStatusCode.NotFound);
    using var fault = await Get("/requests/ffffffff-ffff-ffff-ffff-ffffffffffff", requester);
    var faultText = await fault.Content.ReadAsStringAsync();
    Check("H12 store failure returns safe 500 [NFR-008 partial]", fault.StatusCode == HttpStatusCode.InternalServerError && !faultText.Contains("sensitive-store-marker") && !faultText.Contains("Exception"));
    Check("H13 failure response is not cached", fault.Headers.CacheControl?.NoStore == true);
    using var post = new HttpRequestMessage(HttpMethod.Post, path);
    using var postResponse = await client.SendAsync(post);
    Check("H14 no write endpoint exists", postResponse.StatusCode != HttpStatusCode.OK && postResponse.StatusCode != HttpStatusCode.Created);
}
finally { await app.StopAsync(); }
Console.WriteLine($"RESULT {passed} checks passed; 0 failed. In-memory read slice only; no database/login/production acceptance claimed.");

sealed class FaultInjectingReader : IRequestReader
{
    private readonly DevelopmentRequestReader inner = new();
    public Task<RequestSnapshot?> FindAsync(Guid id, CancellationToken cancellationToken)
        => id == Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff")
            ? throw new InvalidOperationException("sensitive-store-marker") : inner.FindAsync(id, cancellationToken);
}
