using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;

namespace webCore.Services
{
    public class MongoDBService
    {
        internal readonly IMongoCollection<Product_admin> _productCollection;
        internal readonly IMongoCollection<User> _userCollection;
        internal IMongoCollection<Product_admin> _detailProductCollection;
        internal readonly IMongoCollection<Cart> _cartCollection;
        internal readonly IMongoCollection<Voucher> _voucherCollection;
        internal readonly IMongoCollection<Category> _categoryCollection;
        internal readonly IMongoCollection<Order> _orders;
        private readonly IMongoDatabase _mongoDatabase;
        internal readonly IMongoCollection<Account_admin> _accountCollection;

        public IMongoCollection<ForgotPassword> ForgotPasswords { get; internal set; }

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _accountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
            _productCollection = mongoDatabase.GetCollection<Product_admin>("Product");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            ForgotPasswords = mongoDatabase.GetCollection<ForgotPassword>("ForgotPasswords");
            _categoryCollection = mongoDatabase.GetCollection<Category>("Category");
            _detailProductCollection = mongoDatabase.GetCollection<Product_admin>("Product");
            _cartCollection = mongoDatabase.GetCollection<Cart>("Cart");
            _orders = mongoDatabase.GetCollection<Order>("Orders");
            _voucherCollection = mongoDatabase.GetCollection<Voucher>("Vouchers");
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
        // Save user
        public async Task<bool> UpdatePasswordAsync(string email, string newPassword)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.Password, newPassword);

            var result = await _userCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        // Get user by email
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
