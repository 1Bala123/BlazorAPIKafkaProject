using System.Text.Json;
using Confluent.Kafka;
using Serilog;


public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<string,string> _producer;

    public KafkaProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5,
            LingerMs = 5
        };

        _producer =
            new ProducerBuilder<string,string>(config)
            .Build();
    }

    public async Task PublishAsync<T>(
        string topic,
        T message)
    {
        Log.Information("Publishing to Kafka topic {Topic}", topic);
        var deliveryResult = await _producer.ProduceAsync("product-created", new Message<string,string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = JsonSerializer.Serialize(message)
        });
        Log.Information("Delivered to {PartitionOffset}", deliveryResult.TopicPartitionOffset);
        _producer.Flush(TimeSpan.FromSeconds(10));
    }
}