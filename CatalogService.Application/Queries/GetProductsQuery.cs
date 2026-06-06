using CatalogService.Application.DTOs;
using MediatR;

namespace CatalogService.Application.Queries
{
    public record GetProductsQuery() : IRequest<IEnumerable<ProductDto>>;
}
