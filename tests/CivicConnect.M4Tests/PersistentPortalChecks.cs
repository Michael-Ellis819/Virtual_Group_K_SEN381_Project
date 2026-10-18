using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CivicConnect;
using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Data.Entities;
using CivicConnect.Lifecycle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Npgsql;

internal static class PersistentPortalChecks
{
    public static async Task RunAsync(bool preview = false)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__CivicConnect")
            ?? throw new InvalidOperationException("Supply an explicit disposable test database.");
        if (!(new NpgsqlConnectionStringBuilder(connection).Database ?? "").EndsWith("_test", StringComparison.Ordinal))
            throw new InvalidOperationException("The database name must end in _test.");
        var options = new DbContextOptionsBuilder<CivicConnectDbContext>().UseNpgsql(connection).Options;
        var prefix = "portal-test-" + Guid.NewGuid().ToString("N");
        const string password = "Synthetic-portal-check-2026!";
        var accounts = new[] { ("requester", "Requester", ""), ("other", "Requester", ""),
            ("staff", "Staff", "IT Support"), ("staff2", "Staff", "IT Support"),
            ("wrong", "Staff", "Maintenance"), ("manager", "Management", ""),
            ("participant1", "Requester", ""), ("participant2", "Requester", ""),
            ("participant3", "Requester", ""), ("participant4", "Requester", ""), ("participant5", "Requester", "") };
        var passed = 0;
        void Check(string name, bool result)
        {
            if (!result) throw new InvalidOperationException("FAIL " + name);
            Console.WriteLine("PASS " + name); passed++;
        }
        await using (var db = new CivicConnectDbContext(options))
        {
            await db.Database.MigrateAsync();
            foreach (var (name, role, category) in accounts)
            {
                var account = new PortalAccountEntity { Id = prefix + name, Username = preview ? "civic-" + prefix[^6..] + "-" + name : prefix + name,
                    DisplayName = "Synthetic " + name, Role = role, IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString("N") };
                account.PasswordHash = new PasswordHasher<PortalAccountEntity>().HashPassword(account, password);
                if (category.Length > 0) account.Categories.Add(new() { AccountId = account.Id, CategoryId = category });
                db.PortalAccounts.Add(account);
            }
            await db.SaveChangesAsync();
        }
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var transient = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddHours(2));
        using var certificate = new X509Certificate2(transient.Export(X509ContentType.Pfx), "", X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);
        var logs = new SafeLogCollector();
        async Task<WebApplication> Start()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
                ApplicationName = typeof(Composition).Assembly.FullName, EnvironmentName = "Staging" });
            builder.Configuration["ConnectionStrings:CivicConnect"] = connection;
            builder.Logging.ClearProviders(); builder.Logging.AddProvider(logs);
            builder.Services.AddCivicConnect(builder.Configuration);
            builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
            if (preview)
            {
                // Isolated loopback usability host: synthetic accounts and disposable PostgreSQL only.
                // HTTPS acceptance is measured separately by the pinned-certificate test host.
                builder.WebHost.UseUrls("http://127.0.0.1:5089");
                builder.Services.PostConfigure<CookieAuthenticationOptions>(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                    o => o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None);
                builder.Services.Configure<AntiforgeryOptions>(o => o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None);
            }
            else builder.WebHost.ConfigureKestrel(s => s.Listen(IPAddress.Loopback, 0, l => l.UseHttps(certificate)));
            var app = builder.Build(); app.UseCivicConnect(); await app.StartAsync(); return app;
        }
        HttpClient Client(WebApplication app) => new(new HttpClientHandler { AllowAutoRedirect = false,
            CookieContainer = new(), ServerCertificateCustomValidationCallback = (_, cert, _, _) => cert?.Thumbprint == certificate.Thumbprint })
            { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        static string Field(string html, string name) => WebUtility.HtmlDecode(Regex.Matches(html, "<input\\b[^>]*>")
            .Select(m => m.Value).Where(v => v.Contains("name=\"" + name + "\""))
            .Select(v => Regex.Match(v, "value=\"([^\"]*)\"").Groups[1].Value).First());
        async Task<HttpResponseMessage> Form(HttpClient client, string path, Dictionary<string, string> fields, string? page = null)
        {
            if (page is not null) fields["__RequestVerificationToken"] = Field(await client.GetStringAsync(page), "__RequestVerificationToken");
            return await client.PostAsync(path, new FormUrlEncodedContent(fields));
        }
        async Task Login(HttpClient client, string name)
        {
            using var result = await Form(client, "/account/sign-in", new() { ["Username"] = prefix + name,
                ["Password"] = password, ["role"] = "Management" }, "/account/sign-in");
            if (result.StatusCode != HttpStatusCode.Redirect) throw new InvalidOperationException("Real adapter sign-in failed for " + name);
            if (name == "manager" && result.Headers.Location?.ToString() != "/portal/management")
                throw new InvalidOperationException("Management sign-in must land on the permitted overview.");
        }
        async Task<ServiceRequestEntity> Saved(Guid id)
        {
            await using var db = new CivicConnectDbContext(options);
            return await db.ServiceRequests.AsNoTracking().SingleAsync(r => r.RequestId == id);
        }
        WebApplication? app = null;
        try
        {
            app = await Start();
            if (preview)
            {
                Console.WriteLine("LOCAL USABILITY PREVIEW: http://127.0.0.1:5089; real PostgreSQL adapters; synthetic accounts only.");
                Console.WriteLine("Account prefix: " + prefix);
                Console.WriteLine("Temporary fixture password: " + password);
                foreach (var (name, role, _) in accounts) Console.WriteLine(role + " username: civic-" + prefix[^6..] + "-" + name + "; account ID: " + prefix + name);
                await app.WaitForShutdownAsync(); return;
            }
            using var requester = Client(app); using var other = Client(app); using var staff = Client(app);
            using var staff2 = Client(app); using var wrong = Client(app); using var manager = Client(app);
            using var anonymous = Client(app);
            using (var invalid = await Form(anonymous, "/account/sign-in", new() { ["Username"] = prefix + "requester", ["Password"] = "wrong" }, "/account/sign-in"))
            using (var protectedPage = await anonymous.GetAsync("/portal/requests"))
                Check("P01 invalid real credentials grant no session [FR-016]", invalid.StatusCode == HttpStatusCode.OK && (await invalid.Content.ReadAsStringAsync()).Contains("Sign-in failed") && protectedPage.StatusCode == HttpStatusCode.Unauthorized);
            await Login(requester, "requester"); await Login(other, "other"); await Login(staff, "staff");
            await Login(staff2, "staff2"); await Login(wrong, "wrong"); await Login(manager, "manager");
            using (var denied = await requester.GetAsync("/portal/management"))
                Check("P02 client role field cannot grant management [FR-016/NFR-001]", denied.StatusCode == HttpStatusCode.Forbidden);
            var createPage = await requester.GetStringAsync("/portal/requests/new");
            var fields = new Dictionary<string, string> { ["Category"] = "IT Support", ["Description"] = "Persistent portal journey",
                ["Location"] = "Synthetic laboratory", ["SubmissionKey"] = Field(createPage, "SubmissionKey"),
                ["__RequestVerificationToken"] = Field(createPage, "__RequestVerificationToken") };
            using (var invalid = await Form(requester, "/portal/requests/new", new(fields) { ["Category"] = "Unlisted" }))
            {
                await using var db = new CivicConnectDbContext(options);
                Check("P03 invalid submission saves nothing [FR-001/002]", invalid.StatusCode == HttpStatusCode.BadRequest && !await db.ServiceRequests.AnyAsync(r => r.RequesterId == prefix + "requester"));
            }
            using (var csrf = await Form(requester, "/portal/requests/new", new(fields.Where(f => f.Key != "__RequestVerificationToken"))))
                Check("P04 real submission requires antiforgery", csrf.StatusCode == HttpStatusCode.BadRequest);
            await using (var db = new CivicConnectDbContext(options))
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" ADD CONSTRAINT portal_submit_fault CHECK (\"Description\" <> 'Forced submission fault') NOT VALID");
            try
            {
                using var failed = await Form(requester, "/portal/requests/new", new(fields) { ["Description"] = "Forced submission fault" });
                await using var db = new CivicConnectDbContext(options);
                Check("P04a failed submission rolls back request/history/feedback [NFR-003]", failed.StatusCode == HttpStatusCode.InternalServerError && !await db.ServiceRequests.AnyAsync(r => r.RequesterId == prefix + "requester") && !await db.RequestStatusHistory.AnyAsync(h => h.ChangedByUserId == prefix + "requester") && !await db.RequestNotes.AnyAsync(n => n.AuthorId == prefix + "requester"));
            }
            finally
            {
                await using var db = new CivicConnectDbContext(options);
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS portal_submit_fault");
            }
            using var submitted = await Form(requester, "/portal/requests/new", new(fields));
            var path = submitted.Headers.Location?.ToString() ?? throw new InvalidOperationException("Submission has no detail URL");
            var id = Guid.Parse(path.Split('/').Last());
            var saved = await Saved(id);
            Check("P05 submission persists reference/category/date/New/default priority [FR-001/002/017]", submitted.StatusCode == HttpStatusCode.Redirect && saved.Status == "New" && saved.Priority == "Normal" && saved.CategoryId == "IT Support" && saved.CreatedAt > DateTimeOffset.UtcNow.AddMinutes(-5) && saved.ReferenceNumber.Length > 0);
            using (var duplicate = await Form(requester, "/portal/requests/new", new(fields)))
            using (var changed = await Form(requester, "/portal/requests/new", new(fields) { ["Description"] = "Changed payload" }))
            {
                await using var db = new CivicConnectDbContext(options);
                Check("P06 repeat submission is durable and changed key reuse conflicts [NFR-003]", duplicate.Headers.Location?.ToString() == path && changed.StatusCode == HttpStatusCode.Conflict && await db.ServiceRequests.CountAsync(r => r.RequesterId == prefix + "requester") == 1);
            }
            using (var foreign = await other.GetAsync(path))
            using (var category = await wrong.GetAsync(path))
                Check("P07 direct foreign requester/category access denied [FR-003/006/008]", foreign.StatusCode == HttpStatusCode.NotFound && category.StatusCode == HttpStatusCode.NotFound);
            Check("P08 lists include own/permitted and exclude others [FR-004/006]", (await requester.GetStringAsync("/portal/requests")).Contains(saved.ReferenceNumber) && (await staff.GetStringAsync("/portal/requests")).Contains(saved.ReferenceNumber) && !(await other.GetStringAsync("/portal/requests")).Contains(saved.ReferenceNumber) && !(await wrong.GetStringAsync("/portal/requests")).Contains(saved.ReferenceNumber));
            var details = await staff.GetStringAsync(path);
            Check("P09 staff detail includes original fields and missing-value labels [FR-008]", details.Contains("Persistent portal journey") && details.Contains("Synthetic laboratory") && details.Contains(prefix + "requester") && details.Contains("Unassigned") && details.Contains("No due date") && details.Contains("Submitted (UTC)"));
            using (var legacy = await Form(staff, path + "/status", new() { ["ExpectedStatus"] = "New", ["TargetStatus"] = "Assigned", ["Feedback"] = "No owner" }, path))
                Check("P10 status-only assignment rejected without a write [FR-009/010]", legacy.StatusCode == HttpStatusCode.BadRequest && (await Saved(id)).OwnerId is null);
            using (var apiRequest = new HttpRequestMessage(HttpMethod.Post, "/requests/" + id + "/status"))
            {
                apiRequest.Headers.Add("X-CSRF-TOKEN", Field(await staff.GetStringAsync(path), "__RequestVerificationToken"));
                apiRequest.Content = JsonContent.Create(new { ExpectedStatus = "New", TargetStatus = "Assigned", Feedback = "No owner" });
                using var apiResult = await staff.SendAsync(apiRequest);
                Check("P10a legacy JSON API also rejects ownerless assignment", apiResult.StatusCode == HttpStatusCode.BadRequest && (await Saved(id)).Status == "New");
            }
            await using (var db = new CivicConnectDbContext(options))
            {
                var rejected = false;
                try { await new EfRequestStatusWriter(db, new OwnershipRule()).SaveStatusChangeAsync(id, "New", "Closed", prefix + "staff", "Bypass", default, saved.Version); }
                catch (ArgumentException) { rejected = true; }
                Check("P10b persistence boundary rejects a direct illegal lifecycle write [R-011]", rejected && (await Saved(id)).Status == "New");
            }
            using (var ineligible = await Form(staff, path + "/assign", new() { ["ExpectedVersion"] = saved.Version.ToString(), ["OwnerId"] = prefix + "wrong" }, path))
                Check("P11 ineligible stored owner rejected [FR-009]", ineligible.StatusCode == HttpStatusCode.BadRequest && (await Saved(id)).OwnerId is null);
            await using (var db = new CivicConnectDbContext(options))
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" ADD CONSTRAINT portal_assign_fault CHECK (\"Description\" <> 'Persistent portal journey' OR \"Status\" <> 'Assigned') NOT VALID");
            try
            {
                using var failed = await Form(staff, path + "/assign", new() { ["ExpectedVersion"] = saved.Version.ToString(), ["OwnerId"] = prefix + "staff" }, path);
                await using var db = new CivicConnectDbContext(options);
                Check("P11a failed assignment leaves previous owner/status/history/feedback [NFR-003]", failed.StatusCode == HttpStatusCode.InternalServerError && (await Saved(id)).OwnerId is null && (await Saved(id)).Status == "New" && !await db.RequestStatusHistory.AnyAsync(h => h.RequestId == id && h.NewStatus == "Assigned") && !await db.RequestNotes.AnyAsync(n => n.RequestId == id && n.Content == "Request assigned."));
            }
            finally
            {
                await using var db = new CivicConnectDbContext(options);
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS portal_assign_fault");
            }
            var a = new Dictionary<string, string> { ["ExpectedVersion"] = saved.Version.ToString(), ["OwnerId"] = prefix + "staff" };
            var b = new Dictionary<string, string> { ["ExpectedVersion"] = saved.Version.ToString(), ["OwnerId"] = prefix + "staff2" };
            // Fetch both CSRF tokens before the race, so both commands carry the same observed version.
            a["__RequestVerificationToken"] = Field(await staff.GetStringAsync(path), "__RequestVerificationToken");
            b["__RequestVerificationToken"] = Field(await staff2.GetStringAsync(path), "__RequestVerificationToken");
            var races = await Task.WhenAll(Form(staff, path + "/assign", a), Form(staff2, path + "/assign", b));
            await using (var db = new CivicConnectDbContext(options))
                Check("P12 simultaneous HTTP claims save exactly one owner/history/feedback [FR-009/NFR-003]", races.Count(r => r.StatusCode == HttpStatusCode.Redirect) == 1 && races.Count(r => r.StatusCode is HttpStatusCode.Conflict or HttpStatusCode.BadRequest) == 1 && (await Saved(id)).OwnerId is not null && await db.RequestStatusHistory.CountAsync(h => h.RequestId == id && h.NewStatus == "Assigned") == 1 && await db.RequestNotes.CountAsync(n => n.RequestId == id && n.Content == "Request assigned.") == 1);
            foreach (var response in races) response.Dispose();
            saved = await Saved(id);
            using (var note = await Form(staff, path + "/notes", new() { ["ExpectedVersion"] = saved.Version.ToString(), ["Content"] = "PRIVATE-PORTAL-MARKER" }, path))
                Check("P13 internal note persists [FR-011]", note.StatusCode == HttpStatusCode.Redirect);
            saved = await Saved(id);
            using (var note = await Form(staff, path + "/notes", new() { ["ExpectedVersion"] = saved.Version.ToString(), ["Content"] = "Public correction preserves earlier updates", ["IsPublic"] = "true" }, path))
            using (var blank = await Form(staff, path + "/notes", new() { ["ExpectedVersion"] = (await Saved(id)).Version.ToString(), ["Content"] = "   " }, path))
                Check("P14 public correction appends; blank note rejected [FR-011]", note.StatusCode == HttpStatusCode.Redirect && blank.StatusCode == HttpStatusCode.BadRequest);
            var requesterHtml = await requester.GetStringAsync(path);
            Check("P15 public feedback/history persists without internal note disclosure [FR-005/011/015]", requesterHtml.Contains("Public correction preserves earlier updates") && requesterHtml.Contains("Request history") && !requesterHtml.Contains("PRIVATE-PORTAL-MARKER") && !requesterHtml.Contains("Internal note added"));
            saved = await Saved(id);
            using (var schedule = await Form(staff, path + "/schedule", new() { ["ExpectedVersion"] = saved.Version.ToString(), ["Priority"] = "High", ["DueAt"] = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm") }, path))
                Check("P16 priority/due update saves UTC audit [FR-017/018]", schedule.StatusCode == HttpStatusCode.Redirect && (await Saved(id)).Priority == "High" && (await Saved(id)).DueAt < DateTimeOffset.UtcNow);
            using (var stale = await Form(staff, path + "/schedule", new() { ["ExpectedVersion"] = saved.Version.ToString(), ["Priority"] = "Low" }, path))
            using (var invalid = await Form(staff, path + "/schedule", new() { ["ExpectedVersion"] = (await Saved(id)).Version.ToString(), ["Priority"] = "Urgent", ["DueAt"] = "not-a-date" }, path))
            using (var invalidDate = await Form(staff, path + "/schedule", new() { ["ExpectedVersion"] = (await Saved(id)).Version.ToString(), ["Priority"] = "High", ["DueAt"] = "not-a-date" }, path))
            using (var denied = await Form(requester, path + "/schedule", new() { ["ExpectedVersion"] = (await Saved(id)).Version.ToString(), ["Priority"] = "Low" }, path))
                Check("P17 stale/invalid/unauthorised scheduling rejected [FR-017/018]", stale.StatusCode == HttpStatusCode.Conflict && invalid.StatusCode == HttpStatusCode.BadRequest && invalidDate.StatusCode == HttpStatusCode.BadRequest && denied.StatusCode == HttpStatusCode.Forbidden && (await Saved(id)).Priority == "High");
            details = await staff.GetStringAsync(path);
            Check("P18 staff audit projects priority/due/note/ownership events [FR-015]", details.Contains("Priority Normal") && details.Contains("Internal note added") && details.Contains("Assigned"));
            var boundaries = new List<Guid>();
            foreach (var label in new[] { "Lower boundary", "Upper boundary" })
            {
                using var result = await Form(requester, "/portal/requests/new", new() { ["Category"] = "IT Support", ["Description"] = label, ["Location"] = "Synthetic laboratory", ["SubmissionKey"] = Guid.NewGuid().ToString() }, "/portal/requests/new");
                boundaries.Add(Guid.Parse(result.Headers.Location!.ToString().Split('/').Last()));
            }
            await using (var db = new CivicConnectDbContext(options))
            {
                var lower = await db.ServiceRequests.SingleAsync(r => r.RequestId == boundaries[0]);
                var upper = await db.ServiceRequests.SingleAsync(r => r.RequestId == boundaries[1]);
                lower.Priority = "Low";
                lower.CreatedAt = new DateTimeOffset(saved.CreatedAt.UtcDateTime.Date, TimeSpan.Zero);
                upper.CreatedAt = lower.CreatedAt.AddDays(1).AddTicks(-10);
                await db.SaveChangesAsync();
            }
            var date = saved.CreatedAt.UtcDateTime.ToString("yyyy-MM-dd");
            Check("P19 combined authorised search and inclusive same-day bounds [FR-007]", (await staff.GetStringAsync("/portal/requests?Search=" + saved.ReferenceNumber + "&Category=IT%20Support&Status=Assigned&Priority=High&From=" + date + "&To=" + date)).Contains(saved.ReferenceNumber) && !(await staff.GetStringAsync("/portal/requests?Search=NO-SUCH-PORTAL-REFERENCE")).Contains(saved.ReferenceNumber));
            using (var scope = app.Services.CreateScope())
            {
                var store = scope.ServiceProvider.GetRequiredService<IPortalRequestStore>();
                var actor = new PortalActor(prefix + "staff", "Staff", ["IT Support"]);
                var asc = await store.ListAsync(actor, new(null, null, null, Sort: "priority-asc"), default);
                var desc = await store.ListAsync(actor, new(null, null, null, Sort: "priority-desc"), default);
                static int Rank(string p) => p == "High" ? 3 : p == "Normal" ? 2 : 1;
                Check("P20 both numeric priority sort directions on a mixed dataset [FR-007/017]", asc.Select(r => r.Priority).Distinct().Count() == 3 && asc.Select(r => Rank(r.Priority)).SequenceEqual(asc.Select(r => Rank(r.Priority)).Order()) && desc.Select(r => Rank(r.Priority)).SequenceEqual(desc.Select(r => Rank(r.Priority)).OrderDescending()));
                var start = new DateTimeOffset(saved.CreatedAt.UtcDateTime.Date, TimeSpan.Zero);
                var bounded = await store.ListAsync(actor, new(null, null, null, start, start.AddDays(1).AddTicks(-1)), default);
                Check("P20a real query includes both exact UTC date boundaries [FR-007]", boundaries.All(boundary => bounded.Any(r => r.Id == boundary)));
            }
            var report = await manager.GetStringAsync("/portal/management?Category=IT%20Support&Status=Assigned");
            Check("P21 management reads saved selected requests and overdue count [FR-013/014/018]", report.Contains(saved.ReferenceNumber) && report.Contains("Overdue<strong>1</strong>"));
            using (var empty = await Form(staff, path + "/status", new() { ["ExpectedStatus"] = "Assigned", ["TargetStatus"] = "In Progress", ["Feedback"] = "" }, path))
                Check("P22 empty feedback rejected without status change [FR-010]", empty.StatusCode == HttpStatusCode.BadRequest && (await Saved(id)).Status == "Assigned");
            // Force a real database error only for this fixture's next transition.
            await using (var db = new CivicConnectDbContext(options))
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" ADD CONSTRAINT portal_final_fault CHECK (\"Description\" <> 'Persistent portal journey' OR \"Status\" <> 'In Progress') NOT VALID");
            try
            {
                using var fault = await Form(staff, path + "/status", new() { ["ExpectedStatus"] = "Assigned", ["TargetStatus"] = "In Progress", ["Feedback"] = "DUMMY-SECRET-PORTAL" }, path);
                var body = await fault.Content.ReadAsStringAsync();
                await using var db = new CivicConnectDbContext(options);
                Check("P23 failed status transaction rolls back with safe response/logs [NFR-003/008]", fault.StatusCode == HttpStatusCode.InternalServerError && (await Saved(id)).Status == "Assigned" && !await db.RequestNotes.AnyAsync(n => n.RequestId == id && n.Content == "DUMMY-SECRET-PORTAL") && !body.Contains("portal_final_fault") && !body.Contains("DUMMY-SECRET-PORTAL") && !logs.Text.Contains("DUMMY-SECRET-PORTAL") && !logs.Text.Contains("PRIVATE-PORTAL-MARKER"));
            }
            finally
            {
                await using var db = new CivicConnectDbContext(options);
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS portal_final_fault");
            }
            foreach (var (from, to) in new[] { ("Assigned", "In Progress"), ("In Progress", "Resolved"), ("Resolved", "Closed") })
            {
                using var result = await Form(staff, path + "/status", new() { ["ExpectedStatus"] = from, ["TargetStatus"] = to, ["Feedback"] = "Public summary: " + to }, path);
                Check("P24 " + from + " to " + to + " persists", result.StatusCode == HttpStatusCode.Redirect && (await Saved(id)).Status == to);
            }
            using (var terminal = await Form(staff, path + "/status", new() { ["ExpectedStatus"] = "Closed", ["TargetStatus"] = "New", ["Feedback"] = "Reopen" }, path))
                Check("P25 terminal request cannot reopen [FR-010/012]", terminal.StatusCode == HttpStatusCode.BadRequest && (await Saved(id)).Status == "Closed");
            Check("P26 requester reads saved completion and completed history/list [FR-003/004/005]", (await requester.GetStringAsync(path)).Contains("Public summary: Closed") && (await requester.GetStringAsync("/portal/requests")).Contains(saved.ReferenceNumber));
            Check("P27 refreshed management reflects saved terminal status and zero selections [FR-013/014]", !(await manager.GetStringAsync("/portal/management?Status=Assigned")).Contains(saved.ReferenceNumber) && (await manager.GetStringAsync("/portal/management?Category=Lost%20Property")).Contains("No requests match these selections"));
            await app.StopAsync(); await app.DisposeAsync(); app = await Start();
            using var afterRestart = Client(app); await Login(afterRestart, "requester");
            Check("P28 request, notes and history survive application restart [FR-015/NFR-003]", (await afterRestart.GetStringAsync(path)).Contains("Public summary: Closed") && (await Saved(id)).Status == "Closed");
            await using (var db = new CivicConnectDbContext(options))
            {
                var account = await db.PortalAccounts.SingleAsync(a => a.Id == prefix + "requester");
                account.IsActive = false; await db.SaveChangesAsync();
            }
            using (var disabled = await afterRestart.GetAsync(path))
                Check("P29 disabling a stored account invalidates its session [FR-016]", disabled.StatusCode == HttpStatusCode.Unauthorized);
            Console.WriteLine($"RESULT {passed} persistent HTTPS portal checks passed; 0 failed. Real PostgreSQL adapters and password verification; generated fixture accounts and pinned test certificate. Not participant usability or public production deployment.");
        }
        finally
        {
            if (app is not null) { await app.StopAsync(); await app.DisposeAsync(); }
            await using var db = new CivicConnectDbContext(options);
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS portal_final_fault");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS portal_submit_fault");
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE \"ServiceRequests\" DROP CONSTRAINT IF EXISTS portal_assign_fault");
            await db.ServiceRequests.Where(r => r.RequesterId.StartsWith(prefix)).ExecuteDeleteAsync();
            await db.PortalAccounts.Where(a => a.Id.StartsWith(prefix)).ExecuteDeleteAsync();
        }
    }

    private sealed class SafeLogCollector : ILoggerProvider, ILogger
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<string> entries = new();
        public string Text => string.Join("\n", entries);
        public ILogger CreateLogger(string categoryName) => this;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        { if (IsEnabled(logLevel)) entries.Enqueue(formatter(state, exception) + (exception is null ? "" : " " + exception)); }
        public void Dispose() { }
    }
}
