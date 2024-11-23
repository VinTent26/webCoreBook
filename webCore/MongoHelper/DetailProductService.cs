using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class DetailProductService
    {
        private readonly IMongoCollection<Product_admin> _productCollection;

        public DetailProductService(MongoDBService mongoDBService)
        {
            _productCollection = mongoDBService._detailProductCollection;
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

            return await _productCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
