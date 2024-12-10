﻿using MongoDB.Driver;
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
        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            var user = await _userCollection.Find(u => u.Name == userName).FirstOrDefaultAsync();

            // Kiểm tra nếu tài khoản bị khóa
            if (user != null && user.Status == 0)
            {
                throw new InvalidOperationException("Tài khoản đã bị khóa");
            }

            return user;
        }

        // Cập nhật thông tin người dùng
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

                // Đảm bảo ngày sinh luôn ở dạng UTC trước khi lưu
                var update = Builders<User>.Update
                    .Set(u => u.Name, user.Name)
                    .Set(u => u.Phone, user.Phone)
                    .Set(u => u.Gender, user.Gender)
                    .Set(u => u.Birthday, user.Birthday.HasValue
                        ? DateTime.SpecifyKind(user.Birthday.Value, DateTimeKind.Utc) // Lưu dưới dạng UTC
                        : (DateTime?)null)
                    .Set(u => u.Address, user.Address)
                    .Set(u => u.Password, user.Password)
                    .Set(u => u.ProfileImage, user.ProfileImage);

                var result = await _userCollection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0;
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
