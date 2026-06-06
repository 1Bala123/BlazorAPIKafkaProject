using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Outbox;
using CatalogService.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CatalogService.Application.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRepository _repository;
        private readonly AppDbContext _db;
        private readonly ILogger<CreateProductCommandHandler> _logger;

        public CreateProductCommandHandler(IProductRepository repository, AppDbContext db, ILogger<CreateProductCommandHandler> logger)
        {
            _repository = repository;
            _db = db;
            _logger = logger;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product(request.Name, request.Price);
            await _repository.AddAsync(product);

            // Add to Outbox
            var evt = product.DomainEvents.Last();
            _db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "product-created",
                Payload = JsonSerializer.Serialize(evt)
            });

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product created and outbox message queued.");

            return product.Id;
        }
    }
}
