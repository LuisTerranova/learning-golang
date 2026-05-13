using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace invoices.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EstablishmentsController(IEstablishmentService service) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<List<Establishment>>> SearchAsync([FromQuery] string q, CancellationToken ct)
    {
        var results = await service.SearchAsync(q, ct);
        return Ok(results);
    }
}
