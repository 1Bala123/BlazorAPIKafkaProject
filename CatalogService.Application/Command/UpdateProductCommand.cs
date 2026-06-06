using MediatR;

namespace CatalogService.Application.Commands
{
    public record UpdateProductCommand(int Id, string Name, decimal Price) : IRequest<Unit>;
}
