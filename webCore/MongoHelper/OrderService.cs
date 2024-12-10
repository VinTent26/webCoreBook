
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class OrderService

    {
        private readonly IMongoCollection<Order> _orders;

        public OrderService(MongoDBService mongoDBService)
        {
            _orders = mongoDBService._orders;
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
        }
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _orders
                .Find(order => order.UserId == userId)
                .ToListAsync();
        }
        // Lấy đơn hàng theo ID
        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orders.Find(order => order.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        }
        public async Task<int> GetTotalOrdersAsync()
        {
            var filter = Builders<Order>.Filter.Empty; // Không áp dụng filter
            return (int)await _orders.CountDocumentsAsync(filter);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var filter = Builders<Order>.Filter.Empty;
            var orders = await _orders.Find(filter).ToListAsync();
            return orders.Sum(order => order.FinalAmount);
        }

    }
}