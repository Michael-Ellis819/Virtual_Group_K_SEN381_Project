using CivicConnect.Application;
using CivicConnect.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SignInResult = CivicConnect.Application.SignInResult;

namespace CivicConnect.Data;

public sealed class EfPortalAccounts : IPortalAccounts
{
    private readonly CivicConnectDbContext _db;
    private readonly IPasswordHasher<PortalAccountEntity> _hasher;

    public EfPortalAccounts(
        CivicConnectDbContext db,
        IPasswordHasher<PortalAccountEntity> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<SignInResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrEmpty(password))
        {
            return new SignInResult(SignInOutcome.InvalidCredentials);
        }

        var account = await _db.PortalAccounts
            .AsNoTracking()
            .Include(a => a.Categories)
            .SingleOrDefaultAsync(
                a => a.Username == username,
                cancellationToken);

        if (account is null || !account.IsActive)
        {
            return new SignInResult(SignInOutcome.InvalidCredentials);
        }

        var result = _hasher.VerifyHashedPassword(
            account,
            account.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
        {
            return new SignInResult(SignInOutcome.InvalidCredentials);
        }

        return new SignInResult(
            SignInOutcome.Authenticated,
            ToPortalAccount(account));
    }

    public async Task<PortalAccount?> FindAsync(
        string id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var account = await _db.PortalAccounts
            .AsNoTracking()
            .Include(a => a.Categories)
            .SingleOrDefaultAsync(
                a => a.Id == id && a.IsActive,
                cancellationToken);

        return account is null ? null : ToPortalAccount(account);
    }

    private static PortalAccount ToPortalAccount(
        PortalAccountEntity account)
    {
        return new PortalAccount(
            account.Id,
            account.DisplayName,
            account.Role,
            account.IsActive,
            account.Categories
                .Select(c => c.CategoryId)
                .ToArray(),
            account.SecurityStamp);
    }
}