using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class VoucherClientService
    {
        private readonly IMongoCollection<Voucher> _voucherCollection;

        public VoucherClientService(MongoDBService mongoDBService)
        {
            _voucherCollection = mongoDBService._voucherCollection;
        }

        // Lấy tất cả các voucher đang hoạt động
        public async Task<List<Voucher>> GetActiveVouchersAsync()
        {
            return await _voucherCollection
                .Find(v => v.IsActive)  // Điều kiện: voucher đang hoạt động
                .ToListAsync();
        }

        // Lấy Voucher theo mã
        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            var voucher = await _voucherCollection.Find(v => v.Code == code && v.IsActive).FirstOrDefaultAsync();

            if (voucher == null || DateTime.Now < voucher.StartDate || DateTime.Now > voucher.EndDate || voucher.UsageCount >= voucher.UsageLimit)
            {
                return null; // Voucher không hợp lệ hoặc đã hết hạn
            }

            return voucher;
        }
        public async Task<List<Voucher>> GetAllVouchersAsync()
        {
            // Giả sử bạn đang dùng MongoDB.Driver để truy vấn dữ liệu
            return await _voucherCollection.Find(v => v.IsActive && v.EndDate > DateTime.UtcNow)
                                           .ToListAsync();
        }
        // Phương thức thêm Voucher mới (nếu cần)
        public async Task AddVoucherAsync(Voucher voucher)
        {
            await _voucherCollection.InsertOneAsync(voucher);
        }

        // Phương thức cập nhật Voucher (nếu cần)
        public async Task UpdateVoucherAsync(string voucherId, Voucher updatedVoucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, new ObjectId(voucherId));
            await _voucherCollection.ReplaceOneAsync(filter, updatedVoucher);
        }
        // Lấy voucher theo mã
        public Voucher GetVoucherByCode(string code)
        {
            return _voucherCollection.Find(v => v.Code == code).FirstOrDefault();
        }

        // Cập nhật số lần sử dụng
        public void UpdateVoucherUsageCount(Voucher voucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id);
            var update = Builders<Voucher>.Update.Set(v => v.UsageCount, voucher.UsageCount);
            _voucherCollection.UpdateOne(filter, update);
        }

        // Phương thức xóa Voucher (nếu cần)
        public async Task DeleteVoucherAsync(string voucherId)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, new ObjectId(voucherId));
            await _voucherCollection.DeleteOneAsync(filter);
        }
        public async Task<bool> ApplyVoucherAsync(string voucherCode)
        {
            // Find the voucher by code
            var voucher = await GetVoucherByCodeAsync(voucherCode);
            // Check if voucher exists and is valid
            if (voucher == null)
            {
                return false;
            }

            // Check if voucher has reached usage limit
            if (voucher.UsageCount >= voucher.UsageLimit)
            {
                return false;
            }

            // Prepare update operations
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id);
            var update = Builders<Voucher>.Update
                .Inc(v => v.UsageCount, 1)  // Increment UsageCount by 1
                .Inc(v => v.UsageLimit, -1); // Decrement UsageLimit by 1

            // Perform the update
            var result = await _voucherCollection.UpdateOneAsync(filter, update);

            // Return true if the update was successful
            return result.ModifiedCount > 0;
        }
    }
}
