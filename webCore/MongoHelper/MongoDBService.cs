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
        private readonly IMongoCollection<Account> _accountCollection;

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);

            _productCollection = mongoDatabase.GetCollection<Product>("products");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _accountCollection = mongoDatabase.GetCollection<Account>("Accounts"); // Thêm collection Account
        }

        public async Task SaveProductAsync(Product product)
        {
            await _productCollection.InsertOneAsync(product);
        }

        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        // Thêm phương thức lưu Account
        public async Task SaveAccountAsync(Account account)
        {
            await _accountCollection.InsertOneAsync(account);
        }

        // Thêm phương thức lấy Account bằng Email (cho đăng nhập)
        public async Task<Account> GetAccountByEmailAsync(string email)
        {
            return await _accountCollection.Find(a => a.Email == email).FirstOrDefaultAsync();
        }
    }
}
