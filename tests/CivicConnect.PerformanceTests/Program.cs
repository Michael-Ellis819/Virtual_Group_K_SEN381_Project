
using System.Diagnostics;
using CivicConnect.Data;
using CivicConnect.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var connectionString =
    configuration.GetConnectionString("CivicConnect")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:CivicConnect is not configured.");

var options = new DbContextOptionsBuilder<CivicConnectDbContext>()
    .UseNpgsql(connectionString)
    .Options;

const int totalOperations = 1000;
const int concurrentUsers = 10;
const int testDurationMinutes = 10;

var requestId = Guid.NewGuid();
var referenceNumber = $"CC-PERF-{Guid.NewGuid():N}";

// Create one temporary test request.
await using (var setupDb = new CivicConnectDbContext(options))
{
    await setupDb.Database.OpenConnectionAsync();

    setupDb.ServiceRequests.Add(new ServiceRequestEntity
    {
        RequestId = requestId,
        ReferenceNumber = referenceNumber,
        RequesterId = "performance-test-user",
        CategoryId = "IT Support",
        Description = "Performance test request",
        Location = "Test Environment",
        Status = "New",
        UpdatedAt = DateTimeOffset.UtcNow
    });

    await setupDb.SaveChangesAsync();
}

try
{
    Console.WriteLine("Starting 2-minute warm-up...");

    var warmupTimer = Stopwatch.StartNew();

    while (warmupTimer.Elapsed < TimeSpan.FromMinutes(2))
    {
        await using var warmupDb = new CivicConnectDbContext(options);
        var warmupReader = new EfRequestReader(warmupDb);

        await warmupReader.FindAsync(requestId, CancellationToken.None);

        await Task.Delay(100);
    }

    Console.WriteLine("Warm-up complete. Starting measured test.");
    var durations = new double[totalOperations];
    var failures = 0;

    using var semaphore = new SemaphoreSlim(concurrentUsers);
    var overallTimer = Stopwatch.StartNew();

    var tasks = Enumerable.Range(0, totalOperations)
        .Select(async index =>
        {
            var scheduledTime = TimeSpan.FromTicks(
                TimeSpan.FromMinutes(testDurationMinutes).Ticks * index / totalOperations);

            var delay = scheduledTime - overallTimer.Elapsed;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }

            await semaphore.WaitAsync();

            try
            {
                var timer = Stopwatch.StartNew();

                // Each operation uses its own DbContext.
                await using var db =
                    new CivicConnectDbContext(options);

                var reader = new EfRequestReader(db);
                var result = await reader.FindAsync(
                    requestId, CancellationToken.None);

                timer.Stop();
                durations[index] = timer.Elapsed.TotalMilliseconds;

                if (result is null ||
                    result.ReferenceNumber != referenceNumber)
                {
                    Interlocked.Increment(ref failures);
                }
            }
            catch
            {
                Interlocked.Increment(ref failures);
            }
            finally
            {
                semaphore.Release();
            }
        });

    await Task.WhenAll(tasks);

    var remainingTime =
        TimeSpan.FromMinutes(testDurationMinutes) - overallTimer.Elapsed;

    if (remainingTime > TimeSpan.Zero)
    {
        await Task.Delay(remainingTime);
    }

    overallTimer.Stop();

    var sorted = durations.Where(x => x > 0)
        .OrderBy(x => x).ToArray();

    var successful = totalOperations - failures;
    var average = sorted.Length > 0 ? sorted.Average() : 0;
    var median = sorted.Length > 0
        ? sorted[(int)Math.Ceiling(sorted.Length * 0.50) - 1]
        : 0;
    var p95 = sorted.Length > 0
        ? sorted[(int)Math.Ceiling(sorted.Length * 0.95) - 1]
        : 0;

    Console.WriteLine("CIVICCONNECT PERFORMANCE TEST");
    Console.WriteLine("-----------------------------");
    Console.WriteLine($"Total operations: {totalOperations}");
    Console.WriteLine($"Maximum concurrency: {concurrentUsers}");
    Console.WriteLine($"Successful operations: {successful}");
    Console.WriteLine($"Failed operations: {failures}");
    Console.WriteLine($"Total duration: {overallTimer.Elapsed.TotalSeconds:F2} s");
    Console.WriteLine($"Average response time: {average:F2} ms");
    Console.WriteLine($"Median response time: {median:F2} ms");
    Console.WriteLine($"95th percentile: {p95:F2} ms");
    Console.WriteLine(
        $"Throughput: {totalOperations / overallTimer.Elapsed.TotalSeconds:F2} ops/sec");

    if (failures > 0)
        Environment.ExitCode = 1;
}
finally
{
    // Remove only the request created by this test.
    await using var cleanupDb =
        new CivicConnectDbContext(options);

    await cleanupDb.RequestNotes
        .Where(n => n.RequestId == requestId)
        .ExecuteDeleteAsync();

    await cleanupDb.RequestStatusHistory
        .Where(h => h.RequestId == requestId)
        .ExecuteDeleteAsync();

    await cleanupDb.ServiceRequests
        .Where(r => r.RequestId == requestId)
        .ExecuteDeleteAsync();
}
