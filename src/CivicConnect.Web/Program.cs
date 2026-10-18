using CivicConnect;
using CivicConnect.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment() && !builder.Environment.IsStaging())
    throw new InvalidOperationException(
        "This release is limited to Development or controlled Staging. Production release has not been approved.");

builder.Services.AddCivicConnect(builder.Configuration);

var app = builder.Build();

app.UseCivicConnect();

// Health endpoint - confirms the web application is running.
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
})).AllowAnonymous();



// Database health endpoint - checks PostgreSQL connectivity.
app.MapGet("/health/database", async (
    CivicConnectDbContext db,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("CivicConnect.Observability");

    try
    {
        var connected = await db.Database.CanConnectAsync();

        if (!connected)
        {
            logger.LogError("PostgreSQL health check failed.");
            return Results.StatusCode(503);
        }

        return Results.Ok(new { status = "Healthy" });
    }
    catch (Exception ex)
    {
        logger.LogError(
            "PostgreSQL health check failed. Exception type: {ExceptionType}",
            ex.GetType().Name);

        return Results.StatusCode(503);
    }
}).AllowAnonymous();


app.Run();