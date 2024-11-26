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
        internal readonly IMongoCollection<Account_admin> _accountCollection;
        internal readonly IMongoCollection<Voucher> _voucherCollection;
       

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _productCollection = mongoDatabase.GetCollection<Product>("products");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _accountCollection = mongoDatabase.GetCollection<Account_admin>("Accounts");
            _voucherCollection = mongoDatabase.GetCollection<Voucher>("Vouchers");
        }
       
    }
}
