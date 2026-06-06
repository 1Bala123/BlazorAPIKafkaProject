using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Outbox;
using CatalogService.Application.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Commands
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
    {
        private readonly IProductRepository _repository;
        private readonly AppDbContext _db;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(IProductRepository repository, AppDbContext db, ILogger<UpdateProductCommandHandler> logger)
        {
            _repository = repository;
            _db = db;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null) throw new Exception("Product not found");

            product.Update(request.Name, request.Price);
            await _repository.UpdateAsync(product);

            var evt = product.DomainEvents.Last();
            _db.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "product-updated",
                Payload = JsonSerializer.Serialize(evt)
            });

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product updated and outbox message queued.");

            return Unit.Value;
        }
    }
}
