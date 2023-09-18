namespace SearchService.Api.Controllers;

using Application.Commands;
using Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/search")]
[Authorize(Policy = PolicyConstants.CoordinatorAssistantAccess)]
public class SearchController : ApiControllerBase
{
    [HttpPost]
    [Route("", Name = nameof(Search))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Search(
        [FromBody] SearchCommand query,
        CancellationToken cancellationToken) =>
        this.Ok(await this.Mediator.Send(query, cancellationToken));
}
