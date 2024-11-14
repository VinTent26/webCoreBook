using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace webCore.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<User> _userCollection;

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);

            _productCollection = mongoDatabase.GetCollection<Product>("products");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
        }

        public async Task SaveProductAsync(Product product)
        {
            await _productCollection.InsertOneAsync(product);
        }

        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        // Thêm phương thức lấy Account bằng Email (cho đăng nhập)
        public async Task<User> GetAccountByEmailAsync(string email)
        {
            return await _userCollection.Find(a => a.Email == email).FirstOrDefaultAsync();
        }
    }
}