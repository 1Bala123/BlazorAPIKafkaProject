namespace CatalogService.Infrastructure.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";

        public string GroupId {get; set;}
        public bool EnableAutoCommit {get; set;}
        public bool EnablePartitionEof {get; set;}
    }
}
