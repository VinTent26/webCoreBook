using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(MongoDBService mongoDBService)
        {
            _userCollection = mongoDBService._userCollection;
        }
        public async Task SaveUserAsync(User user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        // Lấy thông tin người dùng theo email (Bất đồng bộ)
        public async Task<User> GetAccountByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(user => user.Email, email);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        // Lấy thông tin người dùng theo Username
        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            var user = await _userCollection.Find(u => u.Name == userName).FirstOrDefaultAsync();
            return user;
        }

        // Cập nhật thông tin người dùng
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                // Tạo filter để tìm người dùng cần cập nhật theo Id (để đảm bảo cập nhật đúng người dùng)
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

                // Tạo update với các trường cần thay đổi
                var update = Builders<User>.Update
                    .Set(u => u.Name, user.Name)          // Cập nhật tên người dùng
                    .Set(u => u.Phone, user.Phone)        // Cập nhật số điện thoại
                    .Set(u => u.Gender, user.Gender)      // Cập nhật giới tính
                    .Set(u => u.Birthday, user.Birthday)  // Cập nhật ngày sinh
                    .Set(u => u.Address, user.Address)    // Cập nhật địa chỉ
                    .Set(u => u.Password, user.Password)  // Cập nhật mật khẩu
                    .Set(u => u.ProfileImage, user.ProfileImage);  // Cập nhật ảnh đại diện

                // Thực hiện cập nhật
                var result = await _userCollection.UpdateOneAsync(filter, update);

                // Kiểm tra kết quả
                if (result.MatchedCount == 0)
                {
                    Console.WriteLine("Không tìm thấy người dùng để cập nhật.");
                    return false;
                }

                if (result.ModifiedCount > 0)
                {
                    return true;  // Cập nhật thành công
                }
                else
                {
                    Console.WriteLine("Không có thay đổi nào được thực hiện.");
                    return false;  // Không có thay đổi
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật người dùng: {ex.Message}");
                return false;
            }
        }
        // Xóa người dùng (thay đổi trạng thái thay vì xóa cứng)
        public async Task DeleteUserAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.Deleted, true);

            await _userCollection.UpdateOneAsync(filter, update);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var guid))
            {
                return null; // Trả về null nếu userId không hợp lệ
            }

            var user = await _userCollection.Find(u => u.Id == guid).FirstOrDefaultAsync();
            return user;
        }
        // Save user
        public async Task<bool> UpdatePasswordAsync(string email, string newPassword)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var update = Builders<User>.Update.Set(u => u.Password, newPassword);

            var result = await _userCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
    }
}
