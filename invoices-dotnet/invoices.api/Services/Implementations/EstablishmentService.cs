using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using invoices.api.Data.Context;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace invoices.api.Services.Implementations;

public class EstablishmentService(AppDbContext db) : IEstablishmentService
{
    public async Task<List<Establishment>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Establishment>();

        var term = query.Trim().ToLowerInvariant();

        return await db.Establishments
            .AsNoTracking()
            .Where(e => e.Name.ToLower().Contains(term) || (e.Cnpj != null && e.Cnpj.Contains(term)))
            .Take(20)
            .ToListAsync(ct);
    }
}
