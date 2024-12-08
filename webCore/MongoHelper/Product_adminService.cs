using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace webCore.Services
{
    public class Product_adminService
    {
        private readonly IMongoCollection<Product_admin> _ProductCollection;

        public Product_adminService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _ProductCollection = mongoDatabase.GetCollection<Product_admin>("Product");
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
        public async Task DeleteProductAsync(string id)
        {
            var filter = Builders<Product_admin>.Filter.Eq(c => c.Id, id);
            await _ProductCollection.DeleteOneAsync(filter);
        }
    }
}
