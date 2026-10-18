using System.Security.Claims;
using CivicConnect.Application;
using CivicConnect.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicConnect.Controllers;

[Route("account")]
public sealed class AccountController(IEnumerable<IPortalAccounts> adapters) : Controller
{
    [AllowAnonymous, HttpGet("sign-in")]
    public IActionResult SignIn(string? returnUrl) => View(new SignInInput { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost("sign-in"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(SignInInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(input);
        var accounts = adapters.SingleOrDefault();
        if (accounts is null) return Unavailable(input);
        var result = await accounts.AuthenticateAsync(input.Username.Trim(), input.Password, cancellationToken);
        if (result.Outcome == SignInOutcome.Unavailable) return Unavailable(input);
        var account = result.Account;
        if (result.Outcome != SignInOutcome.Authenticated || account is null || !account.IsActive ||
            string.IsNullOrWhiteSpace(account.Id) || account.Id.Length > 100 ||
            account.Role is not ("Requester" or "Staff" or "Management") || string.IsNullOrWhiteSpace(account.SecurityStamp))
        {
            ModelState.AddModelError("", "Sign-in failed. Check your details and try again.");
            return View(input);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id), new(ClaimTypes.Name, account.DisplayName),
            new(ClaimTypes.Role, account.Role), new("active", "true"), new("portal-stamp", account.SecurityStamp)
        };
        claims.AddRange(account.Categories.Distinct().Select(category => new Claim("category", category)));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = false, AllowRefresh = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) });
        return Url.IsLocalUrl(input.ReturnUrl) ? LocalRedirect(input.ReturnUrl!) : Redirect("/portal/requests");
    }

    [HttpPost("sign-out"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SignOutAccount()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private IActionResult Unavailable(SignInInput input)
    {
        Response.StatusCode = 503;
        ModelState.AddModelError("", "Sign-in is temporarily unavailable. Please try again later.");
        return View("SignIn", input);
    }
}
