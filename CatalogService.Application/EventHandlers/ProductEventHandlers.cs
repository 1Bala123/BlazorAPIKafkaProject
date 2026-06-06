using CatalogService.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CatalogService.Application.EventHandlers
{
    public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedDomainEvent>
    {
        private readonly ILogger<ProductCreatedEventHandler> _logger;

        public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Domain Event handled: Product created {ProductId}", notification.ProductId);
            return Task.CompletedTask;
        }
    }
}
