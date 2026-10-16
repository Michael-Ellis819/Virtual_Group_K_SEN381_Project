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
