using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Text.Json;
using CatalogService.Domain.Events;

public class KafkaConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public KafkaConsumerWorker(IServiceScopeFactory scopeFactory, ILogger<KafkaConsumerWorker> logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "inventory-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // safer: commit after successful processing
            EnablePartitionEof = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(new[]
        {
            "product-created",
            "product-updated",
            "product-deleted",
            "inventory-created",
            "notification-send"
        });
        
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel =true;
            cts.Cancel();
        };

        Log.Information("Kafka consumer started, subscribed to product-created");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    Log.Information("Raw Kafka message: {Value}", cr.Message.Value);

                    switch (cr.Topic)
                    {
                        case "product-created":
                            var created = JsonSerializer.Deserialize<ProductCreatedEvent>(cr.Message.Value);
                            Console.WriteLine($"Product created: {created.Name} at {created.Price}");
                            break;

                        case "product-updated":
                            var updated = JsonSerializer.Deserialize<ProductUpdatedEvent>(cr.Message.Value);
                            Console.WriteLine($"Product updated: {updated.ProductId} new price {updated.Price}");
                            break;

                        case "product-deleted":
                            var deleted = JsonSerializer.Deserialize<ProductDeletedEvent>(cr.Message.Value);
                            Console.WriteLine($"Product deleted: {deleted.ProductId}");
                            break;

                        case "inventory-created":
                            var inventory = JsonSerializer.Deserialize<InventoryCreatedEvent>(cr.Message.Value);
                            Console.WriteLine($"Inventory created: {inventory.InventoryId}");
                            break;

                        case "notification-sent":
                            var notification = JsonSerializer.Deserialize<NotificationSentEvent>(cr.Message.Value);
                            Console.WriteLine($"Notification sent: {notification.Message}");
                            break;
                    }
                    //var evt = JsonSerializer.Deserialize<ProductCreatedEvent>(result.Message.Value);

                    // using var scope = _scopeFactory.CreateScope();
                    // var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // db.Products.Add(new Product
                    // {
                    //     ID = evt.ProductId,
                    //     Name = evt.Name,
                    //     Price = evt.Price
                    // });

                    // await db.SaveChangesAsync(stoppingToken);

                    Log.Information("Processed ProductCreatedEvent");

                    // commit offset only after successful DB save
                    consumer.Commit(cr);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error processing Kafka message");
                    // optional: add retry/backoff logic here
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Kafka consumer stopping...");
        }
        finally
        {
            consumer.Close(); // graceful shutdown
        }
    }
}
