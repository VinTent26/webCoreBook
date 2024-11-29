using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class VoucherService
    {
        private readonly IMongoCollection<Voucher> _voucherCollection;

        public VoucherService(MongoDBService mongoDBService)
        {
            _voucherCollection = mongoDBService._voucherCollection;
        }

        // Tạo voucher mới
        public async Task CreateVoucherAsync(Voucher voucher)
        {
            await _voucherCollection.InsertOneAsync(voucher);
        }

        // Lấy danh sách tất cả các voucher
        public async Task<List<Voucher>> GetVouchers()
        {
            return await _voucherCollection.Find(_ => true).ToListAsync();
        }

        // Tìm voucher theo mã
        public async Task<Voucher> GetVoucherByCodeAsync(string code)
        {
            var voucher = await _voucherCollection.Find(v => v.Code == code && v.IsActive).FirstOrDefaultAsync();

            if (voucher == null || DateTime.Now < voucher.StartDate || DateTime.Now > voucher.EndDate || voucher.UsageCount >= voucher.UsageLimit)
            {
                return null; // Voucher không hợp lệ hoặc đã hết hạn
            }

            return voucher;
        }

        // Áp dụng mã voucher cho client
        public async Task<decimal> ApplyVoucherAsync(string code, decimal totalAmount)
        {
            var voucher = await GetVoucherByCodeAsync(code);

            if (voucher == null)
                return totalAmount; // Trả về số tiền gốc nếu voucher không hợp lệ

            // Tính giá trị giảm giá
            decimal discountAmount = voucher.DiscountValue;

            // Cập nhật số lần sử dụng
            var update = Builders<Voucher>.Update.Inc("UsageCount", 1);
            await _voucherCollection.UpdateOneAsync(v => v.Code == code, update);

            return totalAmount - discountAmount; // Tổng tiền sau giảm
        }

        // Lấy voucher theo ID
        public async Task<Voucher> GetVoucherByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            return await _voucherCollection.Find(v => v.Id == objectId).FirstOrDefaultAsync();
        }

        // Cập nhật voucher
        public async Task<bool> UpdateVoucherAsync(string id, Voucher updatedVoucher)
        {
            var objectId = new ObjectId(id);

            // Tạo định nghĩa cho các trường cần cập nhật
            var updateDefinition = Builders<Voucher>.Update
                .Set(v => v.Code, updatedVoucher.Code)
                .Set(v => v.DiscountValue, updatedVoucher.DiscountValue)
                .Set(v => v.StartDate, updatedVoucher.StartDate)
                .Set(v => v.EndDate, updatedVoucher.EndDate)
                .Set(v => v.UsageLimit, updatedVoucher.UsageLimit)
                .Set(v => v.IsActive, updatedVoucher.IsActive);

            // Gửi yêu cầu cập nhật tới MongoDB
            var updateResult = await _voucherCollection.UpdateOneAsync(
                v => v.Id == objectId,
                updateDefinition
            );

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }


        // Xóa voucher
        public async Task<bool> DeleteVoucherAsync(string id)
        {
            var objectId = new ObjectId(id);
            var deleteResult = await _voucherCollection.DeleteOneAsync(v => v.Id == objectId);

            return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
        }
    }
}
