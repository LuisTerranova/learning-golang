using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace invoices.api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController(
    IInvoiceService _invoiceService,
    IInvoiceRepository _invoiceRepo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Invoice>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = false,
        CancellationToken ct = default)
    {
        return await _invoiceService.GetAllAsync(page, pageSize, search, sortBy, ascending, ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Invoice>> GetById(Guid id, CancellationToken ct = default)
    {
        var invoice = await _invoiceService.GetByIdAsync(id, ct);

        if (invoice is null)
            return NotFound();

        return invoice;
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount(
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        return await _invoiceService.GetCountAsync(search, ct);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _invoiceService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Invoice invoice, CancellationToken ct = default)
    {
        if (id != invoice.Id)
            return BadRequest("Route id does not match invoice id.");

        try
        {
            await _invoiceService.UpdateAsync(invoice, ct);
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
        var raw = await _invoiceRepo.GetRawInvoiceByInvoiceIdAsync(id, ct);

        if (raw?.ImageData is null)
            return NotFound();

        var fileName = string.IsNullOrWhiteSpace(raw.FileName)
            ? "original.pdf"
            : raw.FileName;

        return File(raw.ImageData, "application/pdf", fileName);
    }

    [HttpPost("process")]
    public async Task<IActionResult> SendToProcess(RawInvoice raw, CancellationToken ct = default)
    {
        await _invoiceService.SendInvoicesToProcessAsync(raw, ct);
        return Accepted();
    }
}
