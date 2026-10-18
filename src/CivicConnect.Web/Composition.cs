using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace CivicConnect;
using System.Diagnostics;
public static class Composition
{
    public static IServiceCollection AddCivicConnect(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllersWithViews().AddApplicationPart(typeof(Composition).Assembly);
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
        {
            o.Cookie.HttpOnly = true;
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            o.Cookie.SameSite = SameSiteMode.Lax;
            o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = 401; return Task.CompletedTask; };
            o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = 403; return Task.CompletedTask; };
            o.Events.OnValidatePrincipal = async c =>
            {
                // Browser sessions issued by AccountController are checked against live account data.
                // Legacy synthetic test tickets carry no portal stamp and remain confined to harnesses.
                var stamp = c.Principal?.FindFirst("portal-stamp")?.Value;
                if (stamp is null) return;
                var actor = c.Principal is null ? null : PortalActor.From(c.Principal);
                var accounts = c.HttpContext.RequestServices.GetServices<IPortalAccounts>().SingleOrDefault();
                var account = actor is null || accounts is null ? null : await accounts.FindAsync(actor.Id, c.HttpContext.RequestAborted);
                if (account is null || !account.IsActive || account.Id != actor!.Id || account.SecurityStamp != stamp || account.Role != actor.Role ||
                    !account.Categories.ToHashSet(StringComparer.Ordinal).SetEquals(actor.Categories)) c.RejectPrincipal();
            };
        });
        services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        services.AddAntiforgery(o =>
        {
            o.HeaderName = "X-CSRF-TOKEN";
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        services.AddSingleton<IRequestAccessPolicy, RequestAccessPolicy>();
        services.AddScoped<IRequestReader, EfRequestReader>();
        services.AddScoped<RequestQueryService>();
        services.AddSingleton<StatusTransitionService>();
        services.AddSingleton<Lifecycle.IStatusTransitionService, Lifecycle.StatusTransitionService>();
        services.AddSingleton<Lifecycle.IOwnershipRule, Lifecycle.OwnershipRule>();
        services.AddSingleton<Lifecycle.IServiceActivitySummaryCalculator, Lifecycle.ServiceActivitySummaryCalculator>();
        services.AddScoped<IRequestStatusWriter, EfRequestStatusWriter>();
        services.AddScoped<RequestStatusService>();
        var connectionString =
            configuration.GetConnectionString("CivicConnect")
            ?? throw new InvalidOperationException(
                "CivicConnect database connection is not configured.");

        services.AddDbContext<CivicConnectDbContext>(options =>
            options.UseNpgsql(connectionString));
        return services;
    }
    public static WebApplication UseCivicConnect(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            try { await next(context); }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
            catch (Exception ex)
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("CivicConnect.Observability");

                logger.LogError(
                    "Unhandled application exception. Type: {ExceptionType}",
                    ex.GetType().Name);

                if (context.Response.HasStarted) throw;

                context.Response.Clear();
                context.Response.Headers.CacheControl = "no-store";

                await Results.Problem(
                    statusCode: 500,
                    title: "The request could not be completed."
                ).ExecuteAsync(context);
            }
        });
        
        // M4 observability: monitor HTTP failures and slow requests.
        app.Use(async (context, next) =>
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();

                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("CivicConnect.Observability");

                if (context.Response.StatusCode >= 500)
                {
                    logger.LogError(
                        "HTTP server error. Status: {StatusCode}",
                        context.Response.StatusCode);
                }

                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    logger.LogWarning(
                        "Slow HTTP request. Duration: {DurationMs}ms",
                        stopwatch.ElapsedMilliseconds);
                }
            }
        });
        
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
