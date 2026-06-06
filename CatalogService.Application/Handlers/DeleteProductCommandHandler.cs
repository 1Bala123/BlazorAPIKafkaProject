using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Outbox;
using CatalogService.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CatalogService.Application.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IProductRepository _repository;
        private readonly AppDbContext _db;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(IProductRepository repository, AppDbContext db, ILogger<DeleteProductCommandHandler> logger)
        {
            _repository = repository;
            _db = db;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null) throw new Exception("Product not found");

            product.Delete();
            await _repository.DeleteAsync(product);

            var evt = product.DomainEvents.Last();
            _db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "product-deleted",
                Payload = JsonSerializer.Serialize(evt)
            });

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product deleted and outbox message queued.");

            return Unit.Value;
        }
    }
}
