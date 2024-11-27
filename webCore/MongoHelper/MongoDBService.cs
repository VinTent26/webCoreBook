using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webCore.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Product_admin> _productCollection;
        private readonly IMongoCollection<User> _userCollection;
        internal IMongoCollection<Product_admin> _detailProductCollection;
        private readonly IMongoCollection<Category> _categoryCollection;
        internal readonly IMongoCollection<Cart> _cartCollection;
        internal readonly IMongoCollection<Voucher> _voucherCollection;


        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);

            _productCollection = mongoDatabase.GetCollection<Product_admin>("Product");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _categoryCollection = mongoDatabase.GetCollection<Category>("Category");
            _detailProductCollection= mongoDatabase.GetCollection<Product_admin>("Product");
            _cartCollection= mongoDatabase.GetCollection<Cart>("Cart");
            _voucherCollection = mongoDatabase.GetCollection<Voucher>("Vouchers");

        }
        public async Task<Product_admin> GetProductByIdAsync(string id)
        {
            return await _productCollection.Find(p => p.Id == id && !p.Deleted).FirstOrDefaultAsync();
        }
        // Lấy danh sách sản phẩm
        public async Task<List<Product_admin>> GetProductsAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false); // Lấy sản phẩm chưa bị xóa
            return await _productCollection.Find(filter).ToListAsync();
        }

        // Lưu người dùng
        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        // Lấy thông tin người dùng theo email (Bất đồng bộ)
        public async Task<User> GetAccountByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Email, email);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        // Lấy người dùng theo tên
        public async Task<User> GetUserByNameAsync(string userName)
        {
            return await _userCollection.Find(user => user.Name == userName).FirstOrDefaultAsync();
        }

        // Cập nhật thông tin người dùng
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

            await _userCollection.UpdateOneAsync(filter, update);
        }

        // Xóa người dùng (thay đổi trạng thái thay vì xóa cứng)
        public async Task DeleteUserAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.Deleted, true);

            await _userCollection.UpdateOneAsync(filter, update);
        }
        // Lấy danh mục gốc (không có ParentId)
        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, null);
            return await _categoryCollection.Find(filter).ToListAsync();
        }

        // Lấy danh mục con theo ParentId
        public async Task<List<Category>> GetSubCategoriesByParentIdAsync(string parentId)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, parentId);
            return await _categoryCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false);
            return await _categoryCollection.Find(filter).ToListAsync();
        }

        public async Task<Category> GetCategoryBreadcrumbByIdAsync(string categoryId)
        {
            return await _categoryCollection.Find(c => c._id == categoryId).FirstOrDefaultAsync();
        }
        public async Task<Product_admin> GetProductBreadcrumbByIdAsync(string productId)
        {
            return await _productCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();
        }

    }
}
