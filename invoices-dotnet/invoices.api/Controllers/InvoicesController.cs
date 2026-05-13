using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace invoices.api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController(IInvoiceService invoiceService, IInvoiceRepository invoiceRepo)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Invoice>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default
    )
    {
        return await invoiceService.GetAllAsync(page, pageSize, search, sortBy, ascending, year, month, ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Invoice>> GetById(Guid id, CancellationToken ct = default)
    {
        var invoice = await invoiceService.GetByIdAsync(id, ct);

        if (invoice is null)
            return NotFound();

        return invoice;
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(
        [FromQuery] string? search = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken ct = default
    )
    {
        return await invoiceService.GetCountAsync(search, year, month, ct);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        try
        {
            await invoiceService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("batch-delete")]
    public async Task<IActionResult> DeleteMany([FromBody] BatchDeleteRequest request, CancellationToken ct = default)
    {
        await invoiceService.DeleteManyAsync(request.Ids, ct);
        return NoContent();
    }

    [HttpGet("groups")]
    public async Task<ActionResult<List<YearMonthGroup>>> GetGroups(CancellationToken ct = default)
    {
        return await invoiceService.GetGroupsAsync(ct);
    }

    [HttpGet("by-month")]
    public async Task<ActionResult<List<Invoice>>> GetByMonth(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct = default)
    {
        return await invoiceService.GetByMonthAsync(year, month, ct);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        Invoice invoice,
        CancellationToken ct = default
    )
    {
        if (id != invoice.Id)
            return BadRequest("Route id does not match invoice id.");

        try
        {
            await invoiceService.UpdateAsync(invoice, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}/raw-image")]
    public async Task<IActionResult> GetRawImage(Guid id, CancellationToken ct = default)
    {
        var raw = await invoiceRepo.GetRawInvoiceByInvoiceIdAsync(id, ct);

        if (raw?.ImageData is null)
            return NotFound();

        var fileName = string.IsNullOrWhiteSpace(raw.FileName) ? "original.pdf" : raw.FileName;

        return File(raw.ImageData, "application/pdf", fileName);
    }

    [HttpPost("process")]
    public async Task<IActionResult> SendToProcess(RawInvoice raw, CancellationToken ct = default)
    {
        await invoiceService.SendInvoicesToProcessAsync(raw, ct);
        return Accepted();
    }
}
