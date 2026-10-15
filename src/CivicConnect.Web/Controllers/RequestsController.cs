using CivicConnect.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
namespace CivicConnect.Controllers;
[Authorize]
[Route("requests")]
public sealed class RequestsController(RequestQueryService service, RequestStatusService statuses,
    IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet("antiforgery")]
    public IActionResult Token()
        => Ok(new { requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    [HttpPost("{id}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] StatusUpdateInput? input,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var requestId) || requestId == Guid.Empty ||
            input is null || !ModelState.IsValid)
            return Problem(statusCode: 400, title: "Valid status change information is required.");
        var result = await statuses.ChangeAsync(User, requestId, input.ExpectedStatus,
            input.TargetStatus, input.Feedback, cancellationToken);
        return result switch
        {
            StatusChangeResult.Saved => NoContent(),
            StatusChangeResult.NotFound => Problem(statusCode: 404, title: "Request not found."),
            StatusChangeResult.Conflict => Problem(statusCode: 409, title: "Request changed. Refresh before retrying."),
            _ => Problem(statusCode: 400, title: "Invalid transition or feedback.")
        };
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var requestId) || requestId == Guid.Empty)
            return Problem(statusCode: 400, title: "A non-empty request UUID is required.");
        var result = await service.ReadAsync(User, requestId, cancellationToken);
        // Missing and inaccessible records have the same public response.
        return result is null ? Problem(statusCode: 404, title: "Request not found.") : Ok(result);
    }
}

public sealed record StatusUpdateInput(string? ExpectedStatus, string? TargetStatus, string? Feedback);
