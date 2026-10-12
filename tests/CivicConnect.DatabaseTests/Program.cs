
using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Data.Entities;
using CivicConnect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("CivicConnect");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string is missing.");
}

var options = new DbContextOptionsBuilder<CivicConnectDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var dbContext = new CivicConnectDbContext(options);

// Apply migrations to the configured test database.
await dbContext.Database.MigrateAsync();

Console.WriteLine("PASS: PostgreSQL connection successful.");

// Generate unique IDs to avoid conflicting with existing records.
var requestId = Guid.NewGuid();
var referenceNumber = $"CC-TEST-{Guid.NewGuid():N}";

try
{
    // Create test data automatically.
    var testRequest = new ServiceRequestEntity
    {
        RequestId = requestId,
        ReferenceNumber = referenceNumber,
        RequesterId = "requester-1",
        CategoryId = "IT Support",
        Description = "Test network connection issue",
        Location = "Computer Lab",
        Status = "New",
        UpdatedAt = DateTimeOffset.UtcNow
    };

    var publicNoteEntity = new RequestNoteEntity
    {
        NoteId = Guid.NewGuid(),
        RequestId = requestId,
        Content = "Your request has been received.",
        IsRequesterVisible = true
    };

    var privateNoteEntity = new RequestNoteEntity
    {
        NoteId = Guid.NewGuid(),
        RequestId = requestId,
        Content = "Internal staff investigation note.",
        IsRequesterVisible = false
    };

    dbContext.ServiceRequests.Add(testRequest);
    dbContext.RequestNotes.AddRange(
        publicNoteEntity,
        privateNoteEntity);

    await dbContext.SaveChangesAsync();

    var reader = new EfRequestReader(dbContext);

    var request = await reader.FindAsync(
        requestId, CancellationToken.None);

    if (request is null)
        throw new Exception("FAIL: Test request was not found.");

    Console.WriteLine("PASS: Request retrieved from database.");

    if (request.ReferenceNumber != referenceNumber)
        throw new Exception("FAIL: Incorrect reference number.");

    Console.WriteLine("PASS: Reference number is correct.");

    var missingRequest = await reader.FindAsync(
        Guid.Empty, CancellationToken.None);

    if (missingRequest is not null)
        throw new Exception(
            "FAIL: Non-existent request should return null.");

    Console.WriteLine("PASS: Non-existent request returns null.");

    if (request.Notes.Count != 2)
        throw new Exception(
            $"FAIL: Expected 2 notes, but found {request.Notes.Count}.");

    Console.WriteLine("PASS: Both request notes retrieved.");

    var publicNote = request.Notes.FirstOrDefault(
        n => n.Content == "Your request has been received.");

    if (publicNote is null || !publicNote.IsRequesterVisible)
        throw new Exception(
            "FAIL: Requester-visible note is missing or incorrectly marked.");

    Console.WriteLine("PASS: Requester-visible note is correctly marked.");

    var privateNote = request.Notes.FirstOrDefault(
        n => n.Content == "Internal staff investigation note.");

    if (privateNote is null || privateNote.IsRequesterVisible)
        throw new Exception(
            "FAIL: Staff-only note is missing or incorrectly marked.");

    Console.WriteLine("PASS: Staff-only note is correctly marked.");

    var accessPolicy = new RequestAccessPolicy();
    var queryService = new RequestQueryService(reader, accessPolicy);

    var owner = CreateUser("requester-1", "Requester");
    var ownerResult = await queryService.ReadAsync(owner, requestId);

    if (ownerResult is null)
        throw new Exception(
            "FAIL: Request owner could not access their request.");

    Console.WriteLine("PASS: Request owner can access their request.");

    if (ownerResult.Notes.Count != 1 ||
        !ownerResult.Notes.Contains("Your request has been received.") ||
        ownerResult.Notes.Contains("Internal staff investigation note."))
        throw new Exception(
            "FAIL: Requester note visibility is incorrect.");

    Console.WriteLine("PASS: Requester cannot see internal staff notes.");

    var otherRequester = CreateUser("requester-2", "Requester");
    var otherResult = await queryService.ReadAsync(
        otherRequester, requestId);

    if (otherResult is not null)
        throw new Exception(
            "FAIL: Another requester accessed a private request.");

    Console.WriteLine("PASS: Other requester is denied access.");

    var assignedStaff = CreateUser(
        "staff-1", "Staff", "IT Support");

    var staffResult = await queryService.ReadAsync(
        assignedStaff, requestId);

    if (staffResult is null ||
        staffResult.Notes.Count != 2 ||
        !staffResult.Notes.Contains("Internal staff investigation note."))
        throw new Exception(
            "FAIL: Assigned staff access or note visibility is incorrect.");

    Console.WriteLine("PASS: Assigned staff can see internal notes.");

    var otherStaff = CreateUser(
        "staff-2", "Staff", "Facilities");

    var otherStaffResult = await queryService.ReadAsync(
        otherStaff, requestId);

    if (otherStaffResult is not null)
        throw new Exception(
            "FAIL: Staff from another category accessed the request.");

    Console.WriteLine("PASS: Staff from another category is denied.");

    var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());

    var anonymousResult = await queryService.ReadAsync(
        anonymousUser, requestId);

    if (anonymousResult is not null)
        throw new Exception(
            "FAIL: Unauthenticated user accessed the request.");

    Console.WriteLine("PASS: Unauthenticated user is denied.");

    Console.WriteLine("All database integration checks passed.");
}
finally
{
    // Remove only the records created by this test run.
    await dbContext.RequestNotes
        .Where(n => n.RequestId == requestId)
        .ExecuteDeleteAsync();

    await dbContext.ServiceRequests
        .Where(r => r.RequestId == requestId)
        .ExecuteDeleteAsync();
}

static ClaimsPrincipal CreateUser(
    string userId,
    string role,
    string? category = null)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, userId),
        new(ClaimTypes.Role, role),
        new("active", "true")
    };

    if (category is not null)
        claims.Add(new Claim("category", category));

    return new ClaimsPrincipal(
        new ClaimsIdentity(claims, "TestAuthentication"));
}
