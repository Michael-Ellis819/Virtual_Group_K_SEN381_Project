
using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Data.Entities;
using CivicConnect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using CivicConnect.Lifecycle;

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
string? assignmentStaffId = null;
Guid? assignmentRequestId = null;

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

    var writer = new EfRequestStatusWriter(writerContext, new OwnershipRule());

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
        new EfRequestStatusWriter(rollbackContext, new OwnershipRule());

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

    // Temporary staff account for the portal assignment integration test.
    assignmentStaffId = $"test-staff-{Guid.NewGuid():N}";

    var assignmentStaff = new PortalAccountEntity
    {
        Id = assignmentStaffId,
        Username = assignmentStaffId,
        DisplayName = "Assignment Test Staff",
        PasswordHash = "integration-test-not-for-login",
        Role = "Staff",
        IsActive = true,
        Categories = new List<PortalAccountCategoryEntity>
        {
            new()
            {
                AccountId = assignmentStaffId,
                CategoryId = "IT Support"
            }
        }
    };

    dbContext.PortalAccounts.Add(assignmentStaff);
    await dbContext.SaveChangesAsync();

    Console.WriteLine("PASS: Temporary assignment staff account created.");
    
    // Verify that changing an account's role updates its security stamp.
    var originalSecurityStamp = assignmentStaff.SecurityStamp;

    assignmentStaff.Role = "Management";
    await dbContext.SaveChangesAsync();

    if (assignmentStaff.SecurityStamp == originalSecurityStamp)
    {
        throw new Exception(
            "FAIL: Security stamp did not change after role update.");
    }

// Restore the staff role for the assignment tests.
    var updatedSecurityStamp = assignmentStaff.SecurityStamp;

    assignmentStaff.Role = "Staff";
    await dbContext.SaveChangesAsync();

    if (assignmentStaff.SecurityStamp == updatedSecurityStamp)
    {
        throw new Exception(
            "FAIL: Security stamp did not change when role was restored.");
    }

    Console.WriteLine("PASS: Account role changes update security stamp.");
    
    // Verify that adding a category permission updates the security stamp.
    var stampBeforeCategoryChange = assignmentStaff.SecurityStamp;

    dbContext.PortalAccountCategories.Add(
        new PortalAccountCategoryEntity
        {
            AccountId = assignmentStaffId!,
            CategoryId = "Security Stamp Test"
        });

    await dbContext.SaveChangesAsync();

    if (assignmentStaff.SecurityStamp == stampBeforeCategoryChange)
    {
        throw new Exception(
            "FAIL: Security stamp did not change after category permission was added.");
    }

    Console.WriteLine("PASS: Category permission changes update security stamp.");
    
    var stampBeforeCategoryRemoval = assignmentStaff.SecurityStamp;

    var testCategory = await dbContext.PortalAccountCategories
        .SingleAsync(c =>
            c.AccountId == assignmentStaffId &&
            c.CategoryId == "Security Stamp Test");

    dbContext.PortalAccountCategories.Remove(testCategory);
    await dbContext.SaveChangesAsync();

    if (assignmentStaff.SecurityStamp == stampBeforeCategoryRemoval)
    {
        throw new Exception(
            "FAIL: Security stamp did not change after category permission was removed.");
    }

    Console.WriteLine("PASS: Category permission removal updates security stamp.");

    // Create a separate request for the assignment integration test.
    assignmentRequestId = Guid.NewGuid();

    var assignmentRequest = new ServiceRequestEntity
    {
        RequestId = assignmentRequestId.Value,
        ReferenceNumber = $"CC-ASSIGN-{Guid.NewGuid():N}",
        RequesterId = "requester-1",
        CategoryId = "IT Support",
        Description = "Assignment integration test request",
        Location = "Computer Lab",
        Status = "New",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        Version = 1
    };

    dbContext.ServiceRequests.Add(assignmentRequest);
    await dbContext.SaveChangesAsync();

    Console.WriteLine("PASS: Temporary assignment request created.");

    // Validate the New -> Assigned transition using the real lifecycle service.
    var transitionResult = new CivicConnect.Lifecycle.StatusTransitionService().Evaluate(
        new TransitionRequest(
            "New",
            "Assigned",
            assignmentStaffId,
            "Request assigned to staff.",
            assignmentStaffId));

    if (!transitionResult.Allowed || transitionResult.Change is null)
    {
        throw new Exception(
            $"FAIL: Assignment transition rejected: {transitionResult.Message}");
    }

    Console.WriteLine("PASS: Assignment transition validated.");

    // Execute the real PostgreSQL-backed assignment operation.
    var assignmentStore = new EfPortalRequestStore(
        dbContext,
        new OwnershipRule(),
        new EfRequestStatusWriter(dbContext, new OwnershipRule()));

    var staffActor = new PortalActor(
        assignmentStaffId!,
        "Staff",
        new[] { "IT Support" });

    // An actor without permission for this category must not assign the request.
    var unauthorisedActor = new PortalActor(
        assignmentStaffId!,
        "Staff",
        new[] { "Road Maintenance" });

    var unauthorisedResult = await assignmentStore.AssignAsync(
        unauthorisedActor,
        assignmentRequestId!.Value,
        assignmentStaffId!,
        1,
        transitionResult.Change,
        CancellationToken.None);

    if (unauthorisedResult.Outcome == PortalWriteOutcome.Saved)
    {
        throw new Exception(
            "FAIL: Unauthorised staff member assigned the request.");
    }

    Console.WriteLine("PASS: Unauthorised assignment rejected.");
    
    // The transactional writer must independently reject an unknown actor.
    var transactionRejected = false;

    try
    {
        await new EfRequestStatusWriter(
            dbContext,
            new OwnershipRule()).SaveAssignmentAsync(
            assignmentRequestId!.Value,
            assignmentStaffId!,
            1,
            transitionResult.Change,
            CancellationToken.None,
            "unknown-staff-account");
    }
    catch (DbUpdateConcurrencyException)
    {
        transactionRejected = true;
    }

    if (!transactionRejected)
    {
        throw new Exception(
            "FAIL: Transactional writer accepted an unauthorised actor.");
    }

    dbContext.ChangeTracker.Clear();

    var requestAfterRejection = await dbContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == assignmentRequestId!.Value);

    if (requestAfterRejection.Status != "New" ||
        requestAfterRejection.OwnerId is not null)
    {
        throw new Exception(
            "FAIL: Rejected assignment modified the request.");
    }

    Console.WriteLine("PASS: Transactional assignment rejects unknown actor.");
    
    // Revoke the temporary staff member's category permission.
    await dbContext.PortalAccountCategories
        .Where(c => c.AccountId == assignmentStaffId &&
                    c.CategoryId == "IT Support")
        .ExecuteDeleteAsync();

    dbContext.ChangeTracker.Clear();

    var revokedPermissionRejected = false;

    try
    {
        await new EfRequestStatusWriter(
            dbContext,
            new OwnershipRule()).SaveAssignmentAsync(
            assignmentRequestId!.Value,
            assignmentStaffId!,
            1,
            transitionResult.Change,
            CancellationToken.None,
            assignmentStaffId!);
    }
    catch (DbUpdateConcurrencyException)
    {
        revokedPermissionRejected = true;
    }
    finally
    {
        // Restore the permission for the remaining integration tests.
        dbContext.ChangeTracker.Clear();

        dbContext.PortalAccountCategories.Add(
            new PortalAccountCategoryEntity
            {
                AccountId = assignmentStaffId!,
                CategoryId = "IT Support"
            });

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }

    if (!revokedPermissionRejected)
    {
        throw new Exception(
            "FAIL: Assignment succeeded after staff permission was revoked.");
    }

    var requestAfterRevocation = await dbContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == assignmentRequestId!.Value);

    if (requestAfterRevocation.Status != "New" ||
        requestAfterRevocation.OwnerId is not null)
    {
        throw new Exception(
            "FAIL: Revoked-permission assignment modified the request.");
    }

    Console.WriteLine("PASS: Transactional assignment rejects revoked permission.");
    
    var assignmentResult = await assignmentStore.AssignAsync(
        staffActor,
        assignmentRequestId!.Value,
        assignmentStaffId!,
        1,
        transitionResult.Change,
        CancellationToken.None);

    if (assignmentResult.Outcome != PortalWriteOutcome.Saved)
    {
        throw new Exception(
            $"FAIL: Assignment was not saved: {assignmentResult.Outcome}");
    }

    Console.WriteLine("PASS: PostgreSQL assignment saved.");

    // Verify the owner and status persisted in PostgreSQL.
    dbContext.ChangeTracker.Clear();

    var savedAssignment = await dbContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == assignmentRequestId!.Value);

    if (savedAssignment.OwnerId != assignmentStaffId ||
        savedAssignment.Status != "Assigned")
    {
        throw new Exception(
            "FAIL: Assignment owner or status was not saved correctly.");
    }

    Console.WriteLine("PASS: Assignment owner and status persisted.");

// Verify the status history.
    var assignmentHistory = await dbContext.RequestStatusHistory
        .AsNoTracking()
        .SingleOrDefaultAsync(h =>
            h.RequestId == assignmentRequestId!.Value &&
            h.PreviousStatus == "New" &&
            h.NewStatus == "Assigned");

    if (assignmentHistory is null ||
        assignmentHistory.ChangedByUserId != assignmentStaffId)
    {
        throw new Exception(
            "FAIL: Assignment status history was not saved correctly.");
    }

    Console.WriteLine("PASS: Assignment history persisted.");

// Verify requester-visible feedback.
    var assignmentFeedback = await dbContext.RequestNotes
        .AsNoTracking()
        .SingleOrDefaultAsync(n =>
            n.RequestId == assignmentRequestId!.Value &&
            n.Content == "Request assigned to staff.");

    if (assignmentFeedback is null ||
        !assignmentFeedback.IsRequesterVisible ||
        assignmentFeedback.AuthorId != assignmentStaffId)
    {
        throw new Exception(
            "FAIL: Assignment feedback was not saved correctly.");
    }

    Console.WriteLine("PASS: Assignment feedback persisted.");

    // A stale version must not overwrite the successful assignment.
    var staleAssignmentResult = await assignmentStore.AssignAsync(
        staffActor,
        assignmentRequestId!.Value,
        assignmentStaffId!,
        1,
        transitionResult.Change,
        CancellationToken.None);

    if (staleAssignmentResult.Outcome != PortalWriteOutcome.Conflict)
    {
        throw new Exception(
            $"FAIL: Stale assignment was not rejected. Outcome: {staleAssignmentResult.Outcome}");
    }

    Console.WriteLine("PASS: Stale assignment rejected.");

    // Verify that scheduling changes create a durable audit record.
    dbContext.ChangeTracker.Clear();

    var beforeSchedule = await dbContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == assignmentRequestId!.Value);

    var scheduleDueAt = new DateTimeOffset(
        2026, 10, 25, 14, 30, 0, TimeSpan.Zero);

    var scheduleResult = await assignmentStore.SetScheduleAsync(
        staffActor,
        assignmentRequestId!.Value,
        "High",
        scheduleDueAt,
        beforeSchedule.Version,
        CancellationToken.None);

    if (scheduleResult.Outcome != PortalWriteOutcome.Saved)
    {
        throw new Exception(
            $"FAIL: Scheduling update was not saved: {scheduleResult.Outcome}");
    }

    dbContext.ChangeTracker.Clear();

    var savedSchedule = await dbContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == assignmentRequestId!.Value);

    var scheduleAudit = await dbContext.RequestScheduleAudits
        .AsNoTracking()
        .SingleOrDefaultAsync(a =>
            a.RequestId == assignmentRequestId!.Value);

    if (savedSchedule.Priority != "High" ||
        savedSchedule.DueAt != scheduleDueAt ||
        scheduleAudit is null ||
        scheduleAudit.ChangedByUserId != assignmentStaffId ||
        scheduleAudit.PreviousPriority != "Normal" ||
        scheduleAudit.NewPriority != "High" ||
        scheduleAudit.PreviousDueAt is not null ||
        scheduleAudit.NewDueAt != scheduleDueAt ||
        scheduleAudit.ChangedAt == default)
    {
        throw new Exception(
            "FAIL: Scheduling changes or audit details were not persisted.");
    }

    Console.WriteLine("PASS: PostgreSQL scheduling audit persisted.");

    // A stale scheduling update must not create another audit record.
    var auditCountBefore = await dbContext.RequestScheduleAudits
        .CountAsync(a => a.RequestId == assignmentRequestId!.Value);

    var staleScheduleResult = await assignmentStore.SetScheduleAsync(
        staffActor,
        assignmentRequestId!.Value,
        "Low",
        null,
        beforeSchedule.Version,
        CancellationToken.None);

    dbContext.ChangeTracker.Clear();

    var auditCountAfter = await dbContext.RequestScheduleAudits
        .CountAsync(a => a.RequestId == assignmentRequestId!.Value);

    var scheduleAfterStale = await dbContext.ServiceRequests
        .AsNoTracking()
        .SingleAsync(r => r.RequestId == assignmentRequestId!.Value);

    if (staleScheduleResult.Outcome != PortalWriteOutcome.Conflict ||
        auditCountAfter != auditCountBefore ||
        scheduleAfterStale.Priority != "High" ||
        scheduleAfterStale.DueAt != scheduleDueAt)
    {
        throw new Exception(
            "FAIL: Stale scheduling update modified data or created an audit record.");
    }

    Console.WriteLine("PASS: Stale scheduling update rejected without audit.");
    
    // Verify concurrent submissions with the same key are idempotent.
var concurrentKey = Guid.NewGuid();
var concurrentActor = new PortalActor(
    $"concurrent-test-{Guid.NewGuid():N}",
    "Requester",
    Array.Empty<string>());

var concurrentSubmission = new PortalSubmission(
    "IT Support",
    "Concurrent submission integration test",
    "Computer Lab",
    concurrentKey);

try
{
    await using var contextA = new CivicConnectDbContext(options);
    await using var contextB = new CivicConnectDbContext(options);

    var storeA = new EfPortalRequestStore(
        contextA,
        new OwnershipRule(),
        new EfRequestStatusWriter(contextA, new OwnershipRule()));

    var storeB = new EfPortalRequestStore(
        contextB,
        new OwnershipRule(),
        new EfRequestStatusWriter(contextB, new OwnershipRule()));

    var results = await Task.WhenAll(
        storeA.SubmitAsync(
            concurrentActor, concurrentSubmission, CancellationToken.None),
        storeB.SubmitAsync(
            concurrentActor, concurrentSubmission, CancellationToken.None));

    await using var concurrentVerificationContext = new CivicConnectDbContext(options);

    var savedRequests = await concurrentVerificationContext.ServiceRequests
        .AsNoTracking()
        .Where(r => r.RequesterId == concurrentActor.Id &&
                    r.SubmissionKey == concurrentKey)
        .Select(r => r.RequestId)
        .ToListAsync();

    if (savedRequests.Count != 1 ||
        results.Any(r =>
            r.Outcome != PortalWriteOutcome.Saved ||
            r.RequestId != savedRequests[0]))
    {
        throw new Exception(
            "FAIL: Concurrent submissions did not resolve to one request.");
    }

    Console.WriteLine("PASS: Concurrent submissions created one request.");
    
    var changedSubmission = concurrentSubmission with
    {
        Description = "Different description using the same key"
    };

    var changedResult = await storeA.SubmitAsync(
        concurrentActor,
        changedSubmission,
        CancellationToken.None);

    if (changedResult.Outcome != PortalWriteOutcome.Conflict)
    {
        throw new Exception(
            "FAIL: Reusing a submission key with different details was accepted.");
    }

    Console.WriteLine("PASS: Changed payload with reused key rejected.");
}
finally
{
    await using var cleanupContext = new CivicConnectDbContext(options);

    var concurrentRequestIds = await cleanupContext.ServiceRequests
        .Where(r => r.RequesterId == concurrentActor.Id &&
                    r.SubmissionKey == concurrentKey)
        .Select(r => r.RequestId)
        .ToListAsync();

    await cleanupContext.RequestNotes
        .Where(n => concurrentRequestIds.Contains(n.RequestId))
        .ExecuteDeleteAsync();

    await cleanupContext.RequestStatusHistory
        .Where(h => concurrentRequestIds.Contains(h.RequestId))
        .ExecuteDeleteAsync();

    await cleanupContext.ServiceRequests
        .Where(r => concurrentRequestIds.Contains(r.RequestId))
        .ExecuteDeleteAsync();
}

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

    if (assignmentStaffId is not null)
    {
        await dbContext.PortalAccounts
            .Where(a => a.Id == assignmentStaffId)
            .ExecuteDeleteAsync();
    }

    if (assignmentRequestId is Guid testRequestId)
    {
        await dbContext.RequestScheduleAudits
            .Where(a => a.RequestId == testRequestId)
            .ExecuteDeleteAsync();

        await dbContext.RequestNotes
            .Where(n => n.RequestId == testRequestId)
            .ExecuteDeleteAsync();

        await dbContext.RequestStatusHistory
            .Where(h => h.RequestId == testRequestId)
            .ExecuteDeleteAsync();

        await dbContext.ServiceRequests
            .Where(r => r.RequestId == testRequestId)
            .ExecuteDeleteAsync();
    }
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
