using CivicConnect.Application;
using CivicConnect.Data;
using CivicConnect.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace CivicConnect;
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
        services.AddSingleton<IRequestAccessPolicy, RequestAccessPolicy>();
        services.AddSingleton<IRequestReader, DevelopmentRequestReader>();
        services.AddScoped<RequestQueryService>();
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
            catch (Exception)
            {
                if (context.Response.HasStarted) throw;
                context.Response.Clear();
                context.Response.Headers.CacheControl = "no-store";
                await Results.Problem(statusCode: 500, title: "The request could not be completed.").ExecuteAsync(context);
            }
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
