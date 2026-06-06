using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CatalogService.Infrastructure.Persistence;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IProducer<string, string> _producer;

    public OutboxPublisherWorker(
        ILogger<OutboxPublisherWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5,
            LingerMs = 5
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pending = await db.OutboxMessages
                .Where(m => !m.Processed)
                .ToListAsync(stoppingToken);

            foreach (var msg in pending)
            {
                await _producer.ProduceAsync(msg.EventType, new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = msg.Payload
                }, stoppingToken);

                msg.Processed = true;
                _logger.LogInformation("Published {Topic} event: {Payload}", msg.EventType, msg.Payload);
            }

            await db.SaveChangesAsync(stoppingToken);

            // Wait 5 seconds before next batch
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
