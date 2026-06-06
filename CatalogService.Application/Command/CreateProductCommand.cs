using MediatR;

namespace CatalogService.Application.Commands
{
    public record CreateProductCommand(string Name, decimal Price) : IRequest<int>;
}
