using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace webCore.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Product> _productCollection;
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Account_admin> _AccountCollection;
        private readonly IMongoCollection<Category_admin> _CategoryCollection;
        private readonly IMongoCollection<Book_admin> _BookCollection;

        internal readonly IMongoCollection<Account_admin> _accountCollection;
        internal readonly IMongoCollection<Voucher> _voucherCollection;
       

        private readonly IMongoCollection<Account_admin> _AccountCollection;
        private readonly IMongoCollection<Category_admin> _CategoryCollection;
        private readonly IMongoCollection<Product_admin> _ProductCollection;

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _productCollection = mongoDatabase.GetCollection<Product>("products");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _accountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
            _voucherCollection = mongoDatabase.GetCollection<Voucher>("Vouchers");
            _AccountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
            _CategoryCollection = mongoDatabase.GetCollection<Category_admin>("Category");
            _ProductCollection = mongoDatabase.GetCollection<Product_admin>("Product");
        }
        public async Task<List<Account_admin>> GetAccounts() 
        {
            return await _AccountCollection.Find(_ => true).ToListAsync();
        }
        public async Task SaveProductAsync(Product product)
        {
            await _productCollection.InsertOneAsync(product);
        }

        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }
       

        internal async Task SaveAccountAsync(Account_admin account)
        {
            await _AccountCollection.InsertOneAsync(account);
        }
        //Category
        public async Task<List<Category_admin>> GetCategory()
        {
            return await _CategoryCollection.Find(_ => true).ToListAsync();
        }
        internal async Task SaveCatelogyAsync(Category_admin category)
        {
            await _CategoryCollection.InsertOneAsync(category);
        }
        public async Task<Category_admin> GetCategoryByIdAsync(string id)
        {
            return await _CategoryCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }
        public async Task UpdateCategoryAsync(Category_admin category)
        {
            var filter = Builders<Category_admin>.Filter.Eq(c => c.Id, category.Id);
            await _CategoryCollection.ReplaceOneAsync(filter, category);
        }
        public async Task DeleteCategoryAsync(string id)
        {
            var filter = Builders<Category_admin>.Filter.Eq(c => c.Id, id);
            await _CategoryCollection.DeleteOneAsync(filter);
        }
        //Product
        public async Task<List<Product_admin>> GetProduct()
        {
            return await _ProductCollection.Find(_ => true).ToListAsync();
        }
        internal async Task SaveProductAsync(Product_admin book)
        {
            await _ProductCollection.InsertOneAsync(book);
        }
        public async Task<Product_admin> GetProductByIdAsync(string id)
        {
            return await _ProductCollection.Find(c => c.Id == id).FirstOrDefaultAsync();
        }
        public async Task UpdateProductAsync(Product_admin product)
        {
            var filter = Builders<Product_admin>.Filter.Eq(c => c.Id, product.Id);
            await _ProductCollection.ReplaceOneAsync(filter, product);
        }
    }
}
