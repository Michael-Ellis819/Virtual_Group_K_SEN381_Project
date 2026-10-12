using CivicConnect.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CivicConnect.Application;
using CivicConnect.Security;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
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

var reader = new EfRequestReader(dbContext);

var requestId = Guid.Parse("11111111-1111-1111-1111-111111111111");

var request = await reader.FindAsync(requestId, CancellationToken.None);

if (request is null)
{
    throw new Exception("FAIL: Test request was not found.");
}

if (request.ReferenceNumber != "CC-TEST-001")
{
    throw new Exception("FAIL: Incorrect reference number.");
}

Console.WriteLine("PASS: PostgreSQL connection successful.");
Console.WriteLine("PASS: Request retrieved from database.");
Console.WriteLine("PASS: Reference number is correct.");

var missingRequest = await reader.FindAsync(Guid.Empty, CancellationToken.None);

if (missingRequest is not null)
{
    throw new Exception("FAIL: Non-existent request should return null.");
}

Console.WriteLine("PASS: Non-existent request returns null.");
// Verify that the request contains both database notes.
if (request.Notes.Count != 2)
{
    throw new Exception(
        $"FAIL: Expected 2 notes, but found {request.Notes.Count}.");
}

Console.WriteLine("PASS: Both request notes retrieved.");

// Verify the requester-visible note.
var publicNote = request.Notes.FirstOrDefault(
    n => n.Content == "Your request has been received.");

if (publicNote is null || !publicNote.IsRequesterVisible)
{
    throw new Exception(
        "FAIL: Requester-visible note is missing or incorrectly marked.");
}

Console.WriteLine("PASS: Requester-visible note is correctly marked.");

// Verify the staff-only note.
var privateNote = request.Notes.FirstOrDefault(
    n => n.Content == "Internal staff investigation note.");

if (privateNote is null || privateNote.IsRequesterVisible)
{
    throw new Exception(
        "FAIL: Staff-only note is missing or incorrectly marked.");
}

Console.WriteLine("PASS: Staff-only note is correctly marked.");

// Create the real access policy and query service.
var accessPolicy = new RequestAccessPolicy();
var queryService = new RequestQueryService(reader, accessPolicy);

// Helper method to create an authenticated test user.
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
    {
        claims.Add(new Claim("category", category));
    }

    return new ClaimsPrincipal(
        new ClaimsIdentity(claims, "TestAuthentication"));
}

// TEST 1: The requester can access their own request.
var owner = CreateUser("requester-1", "Requester");

var ownerResult = await queryService.ReadAsync(
    owner, requestId);

if (ownerResult is null)
{
    throw new Exception(
        "FAIL: Request owner could not access their request.");
}

Console.WriteLine("PASS: Request owner can access their request.");

// TEST 2: The requester must not see private notes.
if (ownerResult.Notes.Count != 1 ||
    !ownerResult.Notes.Contains("Your request has been received.") ||
    ownerResult.Notes.Contains("Internal staff investigation note."))
{
    throw new Exception(
        "FAIL: Requester note visibility is incorrect.");
}

Console.WriteLine("PASS: Requester cannot see internal staff notes.");

// TEST 3: Another requester must be denied.
var otherRequester = CreateUser("requester-2", "Requester");

var otherResult = await queryService.ReadAsync(
    otherRequester, requestId);

if (otherResult is not null)
{
    throw new Exception(
        "FAIL: Another requester accessed a private request.");
}

Console.WriteLine("PASS: Other requester is denied access.");

// TEST 4: Assigned staff can access the request and private notes.
var assignedStaff = CreateUser(
    "staff-1", "Staff", "IT Support");

var staffResult = await queryService.ReadAsync(
    assignedStaff, requestId);

if (staffResult is null ||
    staffResult.Notes.Count != 2 ||
    !staffResult.Notes.Contains("Internal staff investigation note."))
{
    throw new Exception(
        "FAIL: Assigned staff access or note visibility is incorrect.");
}

Console.WriteLine("PASS: Assigned staff can see internal notes.");

// TEST 5: Staff from another category must be denied.
var otherStaff = CreateUser(
    "staff-2", "Staff", "Facilities");

var otherStaffResult = await queryService.ReadAsync(
    otherStaff, requestId);

if (otherStaffResult is not null)
{
    throw new Exception(
        "FAIL: Staff from another category accessed the request.");
}

Console.WriteLine("PASS: Staff from another category is denied.");

// TEST 6: Unauthenticated users must be denied.
var anonymousUser = new ClaimsPrincipal(
    new ClaimsIdentity());

var anonymousResult = await queryService.ReadAsync(
    anonymousUser, requestId);

if (anonymousResult is not null)
{
    throw new Exception(
        "FAIL: Unauthenticated user accessed the request.");
}

Console.WriteLine("PASS: Unauthenticated user is denied.");

Console.WriteLine("All database integration checks passed.");
