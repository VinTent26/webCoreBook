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
        private readonly IMongoDatabase _mongoDatabase;


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

    }
}
