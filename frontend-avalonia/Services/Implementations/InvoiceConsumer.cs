using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using frontend_avalonia.Models;
using frontend_avalonia.Services.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace frontend_avalonia.Services.Implementations;

public class InvoiceConsumer(IChannel channel) : IInvoiceConsumer
{
    private readonly string _queueName = "processed_invoices";

    public async Task StartListeningAsync()
    {
        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var result = JsonSerializer.Deserialize<Invoice>(message);

                if (result is null)
                {
                    Console.WriteLine("Received message is either null or invalid.");

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                // TODO Salvar no postgres e avisar UI

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
    }

    public void StopListening()
    {
        channel.CloseAsync();
    }
}
