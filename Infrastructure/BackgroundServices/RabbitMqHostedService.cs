using System.Text;
using System.Text.Json;
using Application.Shared.Events;
using Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Infrastructure.BackgroundServices;

public class RabbitMqHostedService(
    IServiceProvider serviceProvider,
    ILogger<RabbitMqHostedService> logger,
    IOptions<RabbitMQSettings> settingsOptions)
    : BackgroundService
{
    private readonly RabbitMQSettings _settings = settingsOptions.Value;
    private readonly string[] _queueNames = ["product-service.orders"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RabbitMqHostedService запущен. Очереди: {Queues}", string.Join(", ", _queueNames));

        // 🔹 Retry-логика: пытаемся подключиться, но не крашим приложение при ошибке
        const int maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(5);
    
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await InitializeRabbitMqAsync(stoppingToken);
                logger.LogInformation("Успешное подключение к RabbitMQ");
                break; // ✅ Успех — выходим из цикла, продолжаем работу
            }
            catch (BrokerUnreachableException ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "Не удалось подключиться к RabbitMQ (попытка {Attempt}/{MaxRetries}). Повтор через {Delay} сек...", 
                    attempt, maxRetries, retryDelay.TotalSeconds);
            
                try
                {
                    await Task.Delay(retryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Приложение останавливается
                }
            }
            catch (Exception ex) when (attempt == maxRetries)
            {
                // ❌ Последняя попытка не удалась — логируем ошибку, но НЕ выбрасываем исключение,
                // чтобы не убить приложение. Сервис продолжит работать, но без обработки сообщений.
                logger.LogError(ex, "Не удалось подключиться к RabbitMQ после {MaxRetries} попыток. Обработка сообщений отключена.", maxRetries);
                return; 
            }
        }

        // Если подключение не удалось вообще — просто ждём остановки сервиса
        if (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        var correlationId = ea.BasicProperties.CorrelationId ?? Guid.NewGuid().ToString("N");
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());

        try
        {
            // ✅ Создаём scope для каждого сообщения (Scoped DI работает корректно)
            using var scope = serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            logger.LogDebug("Получено: Queue={Queue}, RoutingKey={Key}, CorrelationId={CorrId}", 
                ea.ConsumerTag, ea.RoutingKey, correlationId);

            var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(body);
            if (orderCreated != null)
            {
                await mediator.Send(orderCreated, ct);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false, ct);
            logger.LogDebug("Обработано: CorrelationId={CorrId}", correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка обработки: CorrelationId={CorrId}", correlationId);
            await channel.BasicNackAsync(ea.DeliveryTag, false, true, ct);
        }
    }
    
    private async Task InitializeRabbitMqAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        var connection = await factory.CreateConnectionAsync(ct);
        var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: _settings.ExchangeName,
            type: _settings.ExchangeType,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        foreach (var queueName in _queueNames)
        {
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: _settings.ExchangeName,
                routingKey: "order.created",
                arguments: null,
                cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) => await HandleMessageAsync(channel, ea, ct);

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: ct);

            logger.LogInformation("Подписан на очередь: {Queue}", queueName);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("RabbitMqHostedService останавливается...");
        await base.StopAsync(cancellationToken);
    }
}
