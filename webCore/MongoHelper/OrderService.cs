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
    }
}
