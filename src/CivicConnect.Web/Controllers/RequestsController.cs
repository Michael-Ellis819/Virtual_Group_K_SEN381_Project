using CivicConnect.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CivicConnect.Controllers;
[Authorize]
[Route("requests")]
public sealed class RequestsController(RequestQueryService service) : ControllerBase
{
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
