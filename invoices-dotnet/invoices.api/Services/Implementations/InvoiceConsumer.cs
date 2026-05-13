using System.Text;
using System.Text.Json;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace invoices.api.Services.Implementations;

public class InvoiceConsumer(
    IConnection connection,
    IServiceScopeFactory scopeFactory,
    JsonSerializerOptions jsonOptions,
    ILogger<InvoiceConsumer> logger
) : BackgroundService, IInvoiceConsumer
{
    private readonly string _queueName = "processed_invoices";
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartListeningAsync();

        stoppingToken.Register(() =>
        {
            logger.LogInformation("Shutting down InvoiceConsumer...");
            StopListening();
        });

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("InvoiceConsumer stopped gracefully.");
        }
    }

    public async Task StartListeningAsync()
    {
        _channel = await connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                var result = JsonSerializer.Deserialize<Invoice>(message, jsonOptions);

                if (result is null)
                {
                    logger.LogWarning(
                        "Received message is either null or invalid. DeliveryTag: {DeliveryTag}",
                        ea.DeliveryTag
                    );
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                result.Items ??= [];
                result.ParseErrors ??= [];

                using (var scope = scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                    await repo.AddAsync(result, CancellationToken.None);
                    await repo.SaveChangesAsync(CancellationToken.None);
                }

                logger.LogInformation("Successfully processed invoice {InvoiceId}.", result.Id);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to process message. DeliveryTag: {DeliveryTag}. Requeueing.",
                    ea.DeliveryTag
                );
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
    }

    public void StopListening()
    {
        try
        {
            if (_channel is { IsOpen: true })
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error closing RabbitMQ channel.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        StopListening();
        await base.StopAsync(cancellationToken);
    }
}
