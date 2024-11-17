using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

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

        // Lưu sản phẩm
        public async Task SaveProductAsync(Product product)
        {
            await _productCollection.InsertOneAsync(product);
        }

        // Lưu người dùng
        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        // Lấy thông tin người dùng theo email (Bất đồng bộ)
        public async Task<User> GetAccountByEmailAsync(string email)
        {
            // Tạo filter tìm kiếm theo email
            var filter = Builders<User>.Filter.Eq(user => user.Email, email);

            // Tìm người dùng theo email
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            return user; // Trả về người dùng tìm được (hoặc null nếu không tìm thấy)
        }
        // Phương thức lấy người dùng theo tên
        public async Task<User> GetUserByNameAsync(string userName)
        {
            return await _userCollection.Find(user => user.Name == userName).FirstOrDefaultAsync();
        }

        // Cập nhật thông tin người dùng (Bất đồng bộ)
        public async Task UpdateUserAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, user.Email);
            var update = Builders<User>.Update
                .Set(u => u.Name, user.Name)
                .Set(u => u.Password, user.Password)
                .Set(u => u.Birthday, user.Birthday)
                .Set(u => u.Phone, user.Phone)
                .Set(u => u.Gender, user.Gender)
                .Set(u => u.Address, user.Address);

            await _userCollection.UpdateOneAsync(filter, update); // Dùng bất đồng bộ
        }
    }
}
