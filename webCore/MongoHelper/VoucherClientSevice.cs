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

        public async Task<List<Voucher>> GetActiveVouchersAsync()
        {
            return await _voucherCollection
                .Find(v => v.IsActive)
                .ToListAsync();
        }
    }
}