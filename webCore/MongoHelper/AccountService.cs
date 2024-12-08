using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class AccountService
    {
        private readonly IMongoCollection<Account_admin> _accountCollection;

        public AccountService(MongoDBService mongoDBService)
        {
            _accountCollection = mongoDBService._accountCollection;
        }
        public async Task CreateAccountAsync(Account_admin account)
        {
            await _accountCollection.InsertOneAsync(account);
        }
        public async Task<List<Account_admin>> GetAccounts()
        {
            return await _accountCollection.Find(_ => true).ToListAsync();
        }
        internal async Task SaveAccountAsync(Account_admin account)
        {
            await _accountCollection.InsertOneAsync(account);
        }
        // Get account by ID
        public async Task<Account_admin> GetAccountByIdAsync(string id)
        {
            return await _accountCollection.Find(account => account.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAccountAsync(Account_admin updatedAccount)
        {
            await _accountCollection.ReplaceOneAsync(account => account.Id == updatedAccount.Id, updatedAccount);
        }

        // Delete an account by ID
        public async Task DeleteAccountAsync(string id)
        {
            var filter = Builders<Account_admin>.Filter.Eq(a => a.Id, id);
            await _accountCollection.DeleteOneAsync(filter);
        }
    }
}
