using CatalogService.Domain.Common;

namespace CatalogService.Domain.Events
{
    public record ProductUpdatedDomainEvent(int ProductId, string Name, decimal Price) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
