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
            return await _voucherCollection
                .Find(v => v.Code == code && v.IsActive)  // Điều kiện: voucher hoạt động và mã đúng
                .FirstOrDefaultAsync();
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
        public Voucher GetVoucherById(string voucherId)
        {
            // Chuyển voucherId từ chuỗi thành ObjectId
            ObjectId parsedVoucherId;
            if (!ObjectId.TryParse(voucherId, out parsedVoucherId))
            {
                return null; // Nếu voucherId không hợp lệ, trả về null
            }

            // Tìm voucher trong collection theo voucherId
            return _voucherCollection.Find(v => v.Id == parsedVoucherId).FirstOrDefault();
        }
        public async Task<Voucher> GetVoucherByIdAsync(string voucherId)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, ObjectId.Parse(voucherId));
            return await _voucherCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task UpdateVoucherUsageCountAsync(Voucher voucher)
        {
            var filter = Builders<Voucher>.Filter.Eq(v => v.Id, voucher.Id);
            var update = Builders<Voucher>.Update.Set(v => v.UsageCount, voucher.UsageCount);

            // Cập nhật số lần sử dụng voucher trong cơ sở dữ liệu MongoDB
            await _voucherCollection.UpdateOneAsync(filter, update);
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
    }
}
