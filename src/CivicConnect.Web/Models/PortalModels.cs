using System.ComponentModel.DataAnnotations;
using CivicConnect.Application;
using CivicConnect.Lifecycle;

namespace CivicConnect.Models;

public static class PortalOptions
{
    public static readonly string[] Categories = ["Facility Faults", "Damaged Equipment", "Security Concerns", "IT Support", "Maintenance", "Lost Property", "Other"];
    public static readonly string[] Priorities = ["Low", "Normal", "High"];
    public static readonly string[] Statuses = ["New", "Assigned", "In Progress", "Resolved", "Closed", "Rejected"];
}
public sealed class SignInInput
{
    [Required, StringLength(100)] public string Username { get; set; } = "";
    [Required, StringLength(1024), DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public sealed class SubmissionInput
{
    [Required] public string Category { get; set; } = "";
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    [Required, StringLength(250)] public string Location { get; set; } = "";
    public Guid SubmissionKey { get; set; } = Guid.NewGuid();
}
public sealed class ListInput
{
    [StringLength(200)] public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    [DataType(DataType.Date)] public DateTime? From { get; set; }
    [DataType(DataType.Date)] public DateTime? To { get; set; }
    public string? Priority { get; set; }
    public string Sort { get; set; } = "date-desc";
}
public sealed record ListPage(ListInput Filter, IReadOnlyList<PortalRow> Requests, bool Available);
public sealed record DetailPage(RequestDetailsDto Request, PortalMetadata? Metadata, long Version,
    bool CanChange, IReadOnlyList<string> NextStatuses, bool ExtensionsAvailable);
public sealed record ManagementPage(string? Category, string? Status, ServiceActivitySummary Summary,
    IReadOnlyList<PortalActivity> Requests, bool Available);
public sealed class StatusInput
{
    [Required] public string ExpectedStatus { get; set; } = "";
    [Required] public string TargetStatus { get; set; } = "";
    [Required, StringLength(2000)] public string Feedback { get; set; } = "";
}
public sealed class AssignmentInput
{
    [Required, StringLength(100)] public string OwnerId { get; set; } = "";
    public long ExpectedVersion { get; set; }
}
public sealed class NoteInput
{
    [Required, StringLength(2000)] public string Content { get; set; } = "";
    public bool IsPublic { get; set; }
    public long ExpectedVersion { get; set; }
}
public sealed class ScheduleInput
{
    [Required] public string Priority { get; set; } = "Normal";
    [DataType(DataType.DateTime)] public DateTime? DueAt { get; set; }
    public long ExpectedVersion { get; set; }
}
