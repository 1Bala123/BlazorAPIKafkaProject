using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Product?> GetByIdAsync(int id) =>
            await _db.Products.FindAsync(id);

        public async Task<IEnumerable<Product>> GetAllAsync() =>
            await _db.Products.ToListAsync();

        public async Task AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();
        }
    }
}
