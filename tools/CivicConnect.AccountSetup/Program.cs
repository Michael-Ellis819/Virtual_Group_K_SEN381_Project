using CivicConnect.Data;
using CivicConnect.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var connectionString = configuration.GetConnectionString("CivicConnect")
    ?? throw new InvalidOperationException("Database connection is missing.");

Console.Write("Username: ");
var username = Console.ReadLine()?.Trim() ?? "";

Console.Write("Display name: ");
var displayName = Console.ReadLine()?.Trim() ?? "";

Console.Write("Password: ");
var password = ReadPassword();

if (string.IsNullOrWhiteSpace(username) ||
    username.Length > 100 ||
    string.IsNullOrWhiteSpace(displayName) ||
    displayName.Length > 150 ||
    password.Length < 12)
{
    Console.WriteLine("Invalid details. Password must be at least 12 characters.");
    return;
}

var options = new DbContextOptionsBuilder<CivicConnectDbContext>()
    .UseNpgsql(connectionString)
    .Options;

await using var db = new CivicConnectDbContext(options);

if (await db.PortalAccounts.AnyAsync(a => a.Username == username))
{
    Console.WriteLine("Username already exists.");
    return;
}

var account = new PortalAccountEntity
{
    Id = Guid.NewGuid().ToString("N"),
    Username = username,
    DisplayName = displayName,
    Role = "Requester",
    IsActive = true,
    SecurityStamp = Guid.NewGuid().ToString("N")
};

var hasher = new PasswordHasher<PortalAccountEntity>();
account.PasswordHash = hasher.HashPassword(account, password);

db.PortalAccounts.Add(account);
await db.SaveChangesAsync();

Console.WriteLine("Development requester account created successfully.");

static string ReadPassword()
{
    var password = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
                password.Length--;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }

    return password.ToString();
}
