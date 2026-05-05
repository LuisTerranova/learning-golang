using System.ComponentModel;
using System.Threading.Tasks;

namespace frontend_avalonia.Services.Abstractions;

public interface IInvoiceConsumer
{
    Task StartListeningAsync();
    void StopListening();
}
