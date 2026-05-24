using System.Text;
using System.Text.Json;
using Application.Shared.Events;
using Application.Shared.Interfaces;
using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Infrastructure.Services;

public class RabbitMqPublisher(
    IOptions<RabbitMQSettings> settingsOptions,
    ILogger<RabbitMqPublisher> logger)
    : IRabbitMqPublisher
{
    private readonly RabbitMQSettings _settings = settingsOptions.Value;

    public async Task PublishStockReservedAsync(StockReservedEvent @event, CancellationToken ct = default)
    {
        await PublishAsync("stock.reserved", @event, ct);
        logger.LogInformation("Опубликовано StockReserved: OrderId={OrderId}", @event.OrderId);
    }

    public async Task PublishStockReservationFailedAsync(StockReservationFailedEvent @event, CancellationToken ct = default)
    {
        await PublishAsync("stock.reservation.failed", @event, ct);
        logger.LogInformation("Опубликовано StockReservationFailed: OrderId={OrderId}", @event.OrderId);
    }

    private async Task PublishAsync<T>(string routingKey, T @event, CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: _settings.ExchangeName,
            type: _settings.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
        
        // 👇 Извлекаем CorrelationId через pattern matching
        var correlationId = @event switch
        {
            StockReservedEvent e => e.CorrelationId,
            StockReservationFailedEvent e => e.CorrelationId,
            _ => Guid.NewGuid().ToString("N")
        };

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            CorrelationId = correlationId,
            Headers = new Dictionary<string, object?>
            {
                ["event_type"] = typeof(T).Name,
                ["correlation_id"] = correlationId
            }
        };

        await channel.BasicPublishAsync(
            exchange: _settings.ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);
    }
}
