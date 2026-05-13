using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;

namespace invoices.core.Services.Abstractions;

public interface IEstablishmentService
{
    Task<List<Establishment>> SearchAsync(string query, CancellationToken ct = default);
}
