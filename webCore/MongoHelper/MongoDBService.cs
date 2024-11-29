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
       

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _productCollection = mongoDatabase.GetCollection<Product>("products");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _AccountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
            _CategoryCollection = mongoDatabase.GetCollection<Category_admin>("Category");
            _BookCollection = mongoDatabase.GetCollection<Book_admin>("Book");
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
            _accountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
            _voucherCollection = mongoDatabase.GetCollection<Voucher>("Vouchers");
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
        //Book
        public async Task<List<Book_admin>> GetBook()
        {
            return await _BookCollection.Find(_ => true).ToListAsync();
        }
        internal async Task SaveBookAsync(Book_admin book)
        {
            await _BookCollection.InsertOneAsync(book);
        }
    }
}
