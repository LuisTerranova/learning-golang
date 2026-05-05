using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace invoices.api.Services.Implementations;

public class InvoiceConsumer(IChannel channel) : BackgroundService, IInvoiceConsumer
{
    private readonly string _queueName = "processed_invoices";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartListeningAsync();

        // Register a callback to close the channel as soon as the service starts stopping
        await using var reg = stoppingToken.Register(() =>
        {
            Console.WriteLine("Shutting down InvoiceConsumer...");
            StopListening();
        });

        try
        {
            // Wait until the stoppingToken is triggered (e.g., app shutdown)
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected during a graceful shutdown
            Console.WriteLine("InvoiceConsumer stopped gracefully.");
        }
    }

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

                // TODO: Save in postgres

                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception)
            {
                // In case of error, the message returns to the queue (requeue: true)
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
    }

    public void StopListening()
    {
        try
        {
            if (channel.IsOpen)
            {
                channel.CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error closing RabbitMQ channel: {ex.Message}");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        StopListening();
        await base.StopAsync(cancellationToken);
    }
}
