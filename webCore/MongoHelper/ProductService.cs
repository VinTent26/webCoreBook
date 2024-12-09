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
                    g => GetFeaturedStatusName((FeaturedStatus)g.Key),  // Cast g.Key to FeaturedStatus
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
        public async Task<List<Product_admin>> GetProductsByCategoryIdAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return new List<Product_admin>(); // Return empty list if CategoryId is invalid
            }

            // Lọc sản phẩm theo CategoryId và Deleted là false
            var filter = Builders<Product_admin>.Filter.Eq(p => p.CategoryId, categoryId) &
                         Builders<Product_admin>.Filter.Eq(p => p.Deleted, false);

            // Truy vấn sản phẩm từ MongoDB theo filter
            return await _productCollection.Find(filter).ToListAsync();
        }
        // Hàm sắp xếp sản phẩm
        public List<Product_admin> SortProducts(List<Product_admin> products, string sortOption)
        {
            if (products == null || !products.Any()) return new List<Product_admin>();

            return sortOption switch
            {
                "price-asc" => products.OrderBy(p => p.Price).ToList(),
                "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
                "discount" => products.Where(p => p.DiscountPercentage > 0)
                                      .OrderByDescending(p => p.DiscountPercentage)
                                      .ToList(),
                _ => products // Không sắp xếp nếu sortOption không hợp lệ
            };
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
        // Lấy danh sách sản phẩm nổi bật
        public async Task<List<Product_admin>> GetFeaturedProductsAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.In(p => p.Featured, new[]
                         {
                     (int)FeaturedStatus.Highlighted,
                     (int)FeaturedStatus.New,
                     (int)FeaturedStatus.Suggested
                         });

            return await _productCollection.Find(filter).ToListAsync();
        }

        // Lấy danh sách sản phẩm bán chạy (dựa trên ngày tạo, giả định là các sản phẩm mới nhất sẽ được bán chạy)
        public async Task<List<Product_admin>> GetBestsellerProductsAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false);

            // Sắp xếp theo ngày tạo, giả sử các sản phẩm mới nhất là bán chạy
            var sort = Builders<Product_admin>.Sort.Descending(p => p.CreatedAt);

            // Lấy 10 sản phẩm mới nhất
            return await _productCollection.Find(filter).Sort(sort).Limit(10).ToListAsync();
        }
    }
}
