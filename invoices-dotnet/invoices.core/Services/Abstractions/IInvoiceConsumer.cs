using System.ComponentModel;
using System.Threading.Tasks;

namespace invoices.core.Services.Abstractions;

public interface IInvoiceConsumer
{
    Task StartListeningAsync();
    void StopListening();
}
