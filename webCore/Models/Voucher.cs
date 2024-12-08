using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class Voucher
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("Code")]
        public string Code { get; set; }

        [BsonElement("DiscountValue")]
        public decimal DiscountValue { get; set; }

        [BsonElement("StartDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("EndDate")]
        public DateTime EndDate { get; set; }

        [BsonElement("UsageLimit")]
        public int UsageLimit { get; set; }

        [BsonElement("UsageCount")]
        public int UsageCount { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; }
    }
}
