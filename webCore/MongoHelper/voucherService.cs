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

        // Phương thức để tạo voucher mới
        public async Task CreateVoucherAsync(Voucher voucher)
        {
            await _voucherCollection.InsertOneAsync(voucher);
        }
        public async Task<List<Voucher>> GetVouchers()
        {
            return await _voucherCollection.Find(_ => true).ToListAsync();
        }
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

            decimal discountAmount = voucher.DiscountType == "Percentage"
                ? totalAmount * (voucher.DiscountValue / 100)
                : voucher.DiscountValue;

            // Cập nhật số lần sử dụng
            var update = Builders<Voucher>.Update.Inc("UsageCount", 1);
            await _voucherCollection.UpdateOneAsync(v => v.Code == code, update);

            return totalAmount - discountAmount; // Tổng tiền sau giảm
        }
    }
}
