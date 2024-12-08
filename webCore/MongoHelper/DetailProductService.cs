using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class DetailProductService
    {
        private readonly IMongoCollection<Product_admin> _detailProductCollection;

        public DetailProductService(MongoDBService mongoDBService)
        {
            _detailProductCollection = mongoDBService._detailProductCollection;
        }

        /*        public async Task<Product> GetProductByIdAsync(string id)
                {
                    return await _productCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
                }*/
        // Phương thức lấy sản phẩm theo _id
        public async Task<Product_admin> GetProductByIdAsync(string productId)
        {
            FilterDefinition<Product_admin> filter;

            // Kiểm tra nếu _id là dạng ObjectId
            if (ObjectId.TryParse(productId, out var objectId))
            {
                filter = Builders<Product_admin>.Filter.Eq("_id", objectId);
            }
            else
            {
                filter = Builders<Product_admin>.Filter.Eq("_id", productId);
            }

            return await _detailProductCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<List<Product_admin>> GetProductsByCategoryAsync(string categoryId)
        {
            // Truy vấn các sản phẩm theo categoryId từ cơ sở dữ liệu MongoDB
            var filter = Builders<Product_admin>.Filter.Eq(p => p.CategoryId, categoryId);
            var products = await _detailProductCollection.Find(filter).ToListAsync();  // Sử dụng Find thay vì gọi phương thức không tồn tại

            return products;
        }
    }
}
