namespace CivicConnect.Data.Entities;

public sealed class PortalAccountEntity
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    public List<PortalAccountCategoryEntity> Categories { get; set; } = new();
}

public sealed class PortalAccountCategoryEntity
{
    public string AccountId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;

    public PortalAccountEntity Account { get; set; } = null!;
}