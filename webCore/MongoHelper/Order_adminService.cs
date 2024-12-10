using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using System;

namespace webCore.Services
{
    public class Order_adminService
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public Order_adminService(IConfiguration configuration)
        {
            // Kết nối MongoDB từ cấu hình
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _orderCollection = mongoDatabase.GetCollection<Order>("Orders"); // Tên collection là "Orders"
        }

        // Lấy tất cả đơn hàng
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderCollection.Find(order => true).ToListAsync();
        }
        // Lấy top 3 đơn hàng gần đây nhất
        public async Task<List<Order>> GetRecentOrdersAsync()
        {
            try
            {
                // Tìm tất cả đơn hàng, sắp xếp giảm dần theo CreatedAt và giới hạn 3 đơn hàng
                var recentOrders = await _orderCollection
                    .Find(order => true) // Lọc tất cả đơn hàng
                    .Sort(Builders<Order>.Sort.Descending(order => order.CreatedAt)) // Sắp xếp giảm dần theo CreatedAt
                    .Limit(3) // Lấy tối đa 3 đơn hàng
                    .ToListAsync();

                return recentOrders; // Trả về danh sách
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                Console.WriteLine($"Error fetching recent orders: {ex.Message}");
                return new List<Order>(); // Trả về danh sách trống nếu lỗi
            }
        }


        // Lấy đơn hàng theo ID
        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orderCollection.Find(order => order.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(newStatus))
            {
                return false; // Trả về false nếu đầu vào không hợp lệ
            }

            // Danh sách các trạng thái hợp lệ
            var validStatuses = new List<string> { "Đang chờ duyệt", "Đã duyệt", "Đã hủy" };

            // Kiểm tra xem trạng thái mới có hợp lệ không (kiểm tra chính xác trạng thái)
            if (!validStatuses.Contains(newStatus))
            {
                throw new ArgumentException("Invalid order status provided.");
            }

            try
            {
                // Tạo bản cập nhật với trạng thái mới
                var update = Builders<Order>.Update.Set(order => order.Status, newStatus);

                // Thực hiện cập nhật
                var result = await _orderCollection.UpdateOneAsync(
                    order => order.Id == ObjectId.Parse(orderId), // Chuyển đổi orderId thành ObjectId
                    update
                );

                // Trả về kết quả
                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (FormatException ex)
            {
                // Log error (sử dụng log nếu cần thiết)
                Console.WriteLine($"Error in updating order status: {ex.Message}");
                return false;
            }
        }
    }
}
