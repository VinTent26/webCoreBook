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
        /*public async Task UpdateAccountAsync(Account_admin account)
        {
            var filter = Builders<Account_admin>.Filter.Eq(a => a.Id, account.Id);
            await _AccountCollection.ReplaceOneAsync(filter, account);
        }
        // Phương thức để tạo voucher mới
        public async Task CreateVoucherAsync(Voucher voucher)
        {
            await _voucherCollection.InsertOneAsync(voucher);
        }
        //Lấy dữ liệu voucher
        public async Task<List<Voucher>> GetVouchers()
        {
            return await _voucherCollection.Find(_ => true).ToListAsync();
        }
        // Lấy dữ liệu tài khoản admin
        public async Task<List<Account_admin>> GetAccounts()
        {
            return await _AccountCollection.Find(_ => true).ToListAsync();
        }

       /* public async Task SaveProductAsync(Product product)
        {
            await _productCollection.InsertOneAsync(product);
        }

        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }*/

        /*internal async Task SaveAccountAsync(Account_admin account)
        {
            await _AccountCollection.InsertOneAsync(account);
        }*/
    }
}
