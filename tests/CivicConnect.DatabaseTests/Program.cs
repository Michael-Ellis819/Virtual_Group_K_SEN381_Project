
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
    
    
    // TEST: Two database contexts attempt to update the same request.
    await using var firstContext = new CivicConnectDbContext(options);
    await using var secondContext = new CivicConnectDbContext(options);

    var firstRequest = await firstContext.ServiceRequests
        .SingleAsync(r => r.RequestId == requestId);

    var secondRequest = await secondContext.ServiceRequests
        .SingleAsync(r => r.RequestId == requestId);

    var originalVersion = firstRequest.Version;

    // First user updates the request.
    firstRequest.Status = "In Progress";
    await firstContext.SaveChangesAsync();

    if (firstRequest.Version != originalVersion + 1)
    {
        throw new Exception(
            "FAIL: Request version did not increase.");
    }

    Console.WriteLine("PASS: Request version increases after update.");

    // Second user tries to save an outdated copy.
    secondRequest.Status = "Resolved";

    var conflictDetected = false;

    try
    {
        await secondContext.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        conflictDetected = true;
    }

    if (!conflictDetected)
    {
        throw new Exception(
            "FAIL: Concurrent update was not rejected.");
    }

    Console.WriteLine("PASS: Conflicting update is rejected.");

    // Confirm the second update did not overwrite the first.
    await using var verificationContext =
        new CivicConnectDbContext(options);

    var savedRequest = await verificationContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == requestId);

    if (savedRequest.Status != "In Progress")
    {
        throw new Exception(
            "FAIL: Conflicting update overwrote the saved status.");
    }

    Console.WriteLine("PASS: First successful update is preserved.");

    
    // TEST: Save and retrieve a status-history record.
    var historyId = Guid.NewGuid();

    var historyEntry = new RequestStatusHistoryEntity
    {
        HistoryId = historyId,
        RequestId = requestId,
        PreviousStatus = "New",
        NewStatus = "In Progress",
        ChangedByUserId = "staff-1",
        ChangedAt = DateTimeOffset.UtcNow
    };

    dbContext.RequestStatusHistory.Add(historyEntry);
    await dbContext.SaveChangesAsync();

    await using var historyContext =
        new CivicConnectDbContext(options);

    var savedHistory = await historyContext.RequestStatusHistory
        .AsNoTracking()
        .SingleOrDefaultAsync(h => h.HistoryId == historyId);

    if (savedHistory is null)
    {
        throw new Exception(
            "FAIL: Status-history record was not saved.");
    }

    Console.WriteLine("PASS: Status history saved to PostgreSQL.");

    if (savedHistory.RequestId != requestId ||
        savedHistory.PreviousStatus != "New" ||
        savedHistory.NewStatus != "In Progress" ||
        savedHistory.ChangedByUserId != "staff-1")
    {
        throw new Exception(
            "FAIL: Status-history information is incorrect.");
    }

    Console.WriteLine("PASS: Status-history information is correct.");

    
    await using var writerContext =
        new CivicConnectDbContext(options);

    var writer = new EfRequestStatusWriter(writerContext);

    await writer.SaveStatusChangeAsync(
        requestId,
        "In Progress",
        "Resolved",
        "staff-1",
        "Your request has been resolved.");

    await using var transactionCheckContext =
        new CivicConnectDbContext(options);

    var updatedRequest = await transactionCheckContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == requestId);

    if (updatedRequest.Status != "Resolved")
    {
        throw new Exception(
            "FAIL: Transaction did not save the new status.");
    }

    Console.WriteLine("PASS: Transaction saved request status.");

    var savedTransition = await transactionCheckContext.RequestStatusHistory
        .AsNoTracking()
        .SingleOrDefaultAsync(h =>
            h.RequestId == requestId &&
            h.PreviousStatus == "In Progress" &&
            h.NewStatus == "Resolved");

    if (savedTransition is null)
    {
        throw new Exception(
            "FAIL: Transaction did not save status history.");
    }

    Console.WriteLine("PASS: Transaction saved status history.");

    var savedFeedback = await transactionCheckContext.RequestNotes
        .AsNoTracking()
        .SingleOrDefaultAsync(n =>
            n.RequestId == requestId &&
            n.Content == "Your request has been resolved.");

    if (savedFeedback is null ||
        !savedFeedback.IsRequesterVisible)
    {
        throw new Exception(
            "FAIL: Transaction did not save requester feedback.");
    }

    Console.WriteLine("PASS: Transaction saved requester feedback.");

    
    // TEST: A failed transaction must roll back every change.
    await using var rollbackContext =
        new CivicConnectDbContext(options);

    var rollbackWriter =
        new EfRequestStatusWriter(rollbackContext);

    var historyCountBefore =
        await rollbackContext.RequestStatusHistory
            .CountAsync(h => h.RequestId == requestId);

    var noteCountBefore =
        await rollbackContext.RequestNotes
            .CountAsync(n => n.RequestId == requestId);

    var rollbackDetected = false;

    try
    {
        // The feedback exceeds the database's 2000-character limit.
        // This forces a database write failure.
        await rollbackWriter.SaveStatusChangeAsync(
            requestId,
            "Resolved",
            "Closed",
            "staff-1",
            new string('X', 2500));
    }
    catch (DbUpdateException)
    {
        rollbackDetected = true;
    }

    if (!rollbackDetected)
    {
        throw new Exception(
            "FAIL: Expected database write failure did not occur.");
    }

    Console.WriteLine("PASS: Database write failure detected.");

    // Use a fresh context to verify the actual database state.
    await using var rollbackCheckContext =
        new CivicConnectDbContext(options);

    var requestAfterFailure =
        await rollbackCheckContext.ServiceRequests
            .AsNoTracking()
            .SingleAsync(r => r.RequestId == requestId);

    if (requestAfterFailure.Status != "Resolved")
    {
        throw new Exception(
            "FAIL: Request status was not rolled back.");
    }

    Console.WriteLine("PASS: Request status rolled back.");

    var historyCountAfter =
        await rollbackCheckContext.RequestStatusHistory
            .CountAsync(h => h.RequestId == requestId);

    if (historyCountAfter != historyCountBefore)
    {
        throw new Exception(
            "FAIL: Failed transaction left a history record.");
    }

    Console.WriteLine("PASS: Status history rolled back.");

    var noteCountAfter =
        await rollbackCheckContext.RequestNotes
            .CountAsync(n => n.RequestId == requestId);

    if (noteCountAfter != noteCountBefore)
    {
        throw new Exception(
            "FAIL: Failed transaction left requester feedback.");
    }

    Console.WriteLine("PASS: Requester feedback rolled back.");

    
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
