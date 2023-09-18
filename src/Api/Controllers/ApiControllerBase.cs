namespace SearchService.Api.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? mediator;

    protected ISender Mediator =>
        this.mediator ??= this.HttpContext.RequestServices.GetRequiredService<ISender>();
}
