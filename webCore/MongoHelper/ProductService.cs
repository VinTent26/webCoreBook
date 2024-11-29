using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ProductService
    {
        private readonly IMongoCollection<Product_admin> _productCollection;
       
        public ProductService(MongoDBService mongoDBService)
        {
            _productCollection = mongoDBService._productCollection;
        }
        // Lấy danh sách sản phẩm
        public async Task<Dictionary<string, List<Product_admin>>> GetProductsGroupedByFeaturedAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false);
            var products = await _productCollection.Find(filter).ToListAsync();

            var groupedProducts = products
                .GroupBy(p => p.Featured)
                .ToDictionary(
                    g => GetFeaturedStatusName(g.Key),
                    g => g.ToList()
                );

            return groupedProducts;
        }
        private string GetFeaturedStatusName(FeaturedStatus status)
        {
            switch (status)
            {
                case FeaturedStatus.Highlighted: return "Nổi bật";
                case FeaturedStatus.New: return "Mới";
                case FeaturedStatus.Suggested: return "Gợi ý";
                default: return "Không nổi bật";
            }
        }
        public async Task<List<Product_admin>> GetProductsByCategoryPositionAsync(int position)
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Position, position) &
                         Builders<Product_admin>.Filter.Eq(p => p.Deleted, false);

            return await _productCollection.Find(filter).ToListAsync();
        }
       
        // Lấy tất cả sản phẩm
        public async Task<List<Product_admin>> GetProductsAsync()
        {
            return await _productCollection.Find(product => true).ToListAsync();
        }
        public async Task<Product_admin> GetProductBreadcrumbByIdAsync(string productId)
        {
            return await _productCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();
        }
    }
}
