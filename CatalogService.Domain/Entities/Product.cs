using CatalogService.Domain.Common;
using CatalogService.Domain.Events;

namespace CatalogService.Domain.Entities
{
    public class Product : AggregateRoot
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public decimal Price { get; private set; }

        private Product() { } // EF Core

        public Product(string name, decimal price)
        {
            Name = name;
            Price = price;

            AddDomainEvent(new ProductCreatedDomainEvent(Id, Name, Price));
        }

        public void Update(string name, decimal price)
        {
            Name = name;
            Price = price;

            AddDomainEvent(new ProductUpdatedDomainEvent(Id, Name, Price));
        }

        public void Delete()
        {
            AddDomainEvent(new ProductDeletedDomainEvent(Id));
        }
    }
}
