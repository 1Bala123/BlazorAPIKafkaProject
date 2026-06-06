using CatalogService.Domain.Common;

namespace CatalogService.Domain.Events
{
    public record ProductDeletedDomainEvent(int ProductId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
