using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using webCore.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace webCore.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMongoCollection<Product_admin> _productCollection;

        // Constructor
        public ProductController(IMongoClient mongoClient, IConfiguration configuration)
        {
            var database = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _productCollection = database.GetCollection<Product_admin>("Product");
        }

        // Lấy tất cả sản phẩm
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _productCollection.Find(product => !product.Deleted).ToList();
            return Ok(products);
        }

        // Lấy sản phẩm theo ID
        [HttpGet("{id}")]
        public IActionResult GetProduct(string id)
        {
            var product = _productCollection.Find(p => p.Id == id).FirstOrDefault();
            if (product == null) return NotFound();
            return Ok(product);
        }

        // Tạo mới sản phẩm
        [HttpPost]
        public IActionResult CreateProduct(Product_admin newProduct)
        {
            _productCollection.InsertOne(newProduct);
            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, newProduct);
        }

        // Cập nhật sản phẩm
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(string id, Product_admin updatedProduct)
        {
            var result = _productCollection.ReplaceOne(p => p.Id == id, updatedProduct);
            if (result.MatchedCount == 0) return NotFound();
            return NoContent();
        }

        // Xóa sản phẩm
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(string id)
        {
            var result = _productCollection.DeleteOne(p => p.Id == id);
            if (result.DeletedCount == 0) return NotFound();
            return NoContent();
        }

        // Tìm kiếm sản phẩm theo tên (Title)
        [HttpGet("search")]
        public IActionResult SearchProducts([FromQuery] string name)
        {
            var filter = Builders<Product_admin>.Filter.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(name, "i"));
            var products = _productCollection.Find(filter).ToList();
            return Ok(products);
        }
    }
}
