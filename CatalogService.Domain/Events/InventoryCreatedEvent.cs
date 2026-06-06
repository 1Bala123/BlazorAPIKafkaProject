namespace CatalogService.Domain.Events
{
    public class InventoryCreatedEvent
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
