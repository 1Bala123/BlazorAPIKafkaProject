using MediatR;
using CatalogService.Domain.Common;

namespace CatalogService.Domain.Events
{
    public record ProductCreatedDomainEvent(int ProductId, string Name, decimal Price) 
        : IDomainEvent, INotification
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
