using CatalogService.Domain.Entities;
using CatalogService.Domain.Repositories;
using CatalogService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;

        private readonly ICacheService _cache;

        public ProductRepository(AppDbContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<Product?> GetByIdAsync(int id){
            try
            {
                Log.Information("GetByIdAsync Methods Starts");   

                string cacheKey = $"product:{id}";

                var cached = await _cache.GetAsync<Product>(cacheKey);

                if (cached != null)
                    return cached;

                    return await _db.Products.FindAsync(id);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at GetByIdAsync");
                return default;
            }
            finally
            {
                Log.Information("GetByIdAsync Methods Ends");   
            }
            
        }

        public async Task<IEnumerable<Product>> GetAllAsync(){
            try
            {
                 Log.Information("GetAllAsync Methods Starts");   

                 var products = await _cache.GetAsync<List<Product>>("products:all");

                 if(products != null)
                {
                    return products;
                }

                return await _db.Products.ToListAsync();
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at GetAllAsync");
                return default;
            }
            finally
            {
                Log.Information("GetAllAsync Methods Ends");   
            }
           
        }

        public async Task AddAsync(Product product)
        {
            try{
                 Log.Information("AddAsync Methods Starts");   

                _db.Products.Add(product);
                 await _db.SaveChangesAsync();
                 
                 await _cache.SetAsync($"product:{product.Id}", product);

                // Remove products list cache if exists
                await _cache.RemoveAsync("products:all");

                Log.Information( "Product {ProductId} cached successfully", product.Id);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at AddAsync");
            }
            finally
            {
                Log.Information("AddAsync Methods Ends");   
            }

        }

        public async Task UpdateAsync(Product product)
        {
            try
            {

                 Log.Information("UpdateAsync Methods Starts");  

                _db.Products.Update(product);
                await _db.SaveChangesAsync();

                await _cache.RemoveAsync($"product:{product.Id}");

                await _cache.SetAsync($"product:{product.Id}", product);
            }
             catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at UpdateAsync");
            }
            finally
            {
                Log.Information("UpdateAsync Methods Ends");   
            }
        }

        public async Task DeleteAsync(Product product)
        {
             try
            {

                Log.Information("DeleteAsync Methods Starts");  

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                await _cache.RemoveAsync($"product:{product.Id}");
            }
             catch(Exception ex)
            {
                Log.Error(ex, "Exception Occured at DeleteAsync");
            }
            finally
            {
                Log.Information("DeleteAsync Methods Ends");   
            }
        }
    }
}
