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

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _productCollection = mongoDatabase.GetCollection<Product>("products");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _AccountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
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
    }
}
