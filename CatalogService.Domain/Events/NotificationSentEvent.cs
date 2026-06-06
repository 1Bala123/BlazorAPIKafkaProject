namespace CatalogService.Domain.Events
{
    public class NotificationSentEvent
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
