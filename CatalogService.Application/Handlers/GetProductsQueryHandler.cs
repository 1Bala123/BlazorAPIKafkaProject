using CatalogService.Application.DTOs;
using CatalogService.Domain.Repositories;
using MediatR;

namespace CatalogService.Application.Queries
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
    {
        private readonly IProductRepository _repository;

        public GetProductsQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _repository.GetAllAsync();
            return products.Select(p => new ProductDto(p.Id, p.Name, p.Price));
        }
    }
}
