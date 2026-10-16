using CivicConnect;
using CivicConnect.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException(
        "This slice requires Development. Integrate persistent storage and identity provisioning before deployment.");

builder.Services.AddCivicConnect(builder.Configuration);

var app = builder.Build();

app.UseCivicConnect();

// Health endpoint - confirms the web application is running.
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
})).AllowAnonymous();

// Database health endpoint - checks PostgreSQL connectivity.
app.MapGet("/health/database", async (CivicConnectDbContext db) =>
{
    var connected = await db.Database.CanConnectAsync();

    return connected
        ? Results.Ok(new { status = "Healthy" })
        : Results.StatusCode(503);
}).AllowAnonymous();

app.Run();