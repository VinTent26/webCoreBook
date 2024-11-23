using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class DetailProductController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly DetailProductService _detailProductService;
        public DetailProductController(DetailProductService detailProductService)
        {
            _detailProductService = detailProductService;
        }

        /*        public async Task<IActionResult> DetailProduct(string id)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        return NotFound("Product ID is required.");
                    }

                    var product = await _detailProductService.GetProductByIdAsync(id);
                    if (product == null)
                    {
                        return NotFound("Product not found.");
                    }

                    return View(product);
                }*/
        // API hoặc trang hiển thị sản phẩm theo _id
        public async Task<IActionResult> DetailProduct()
        {
            string productId = "46799a9a-343e-41cc-ab25-b83494f770af";  // ID sản phẩm cụ thể

            var product = await _detailProductService.GetProductByIdAsync(productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return View(product);  // Trả về trang View hoặc JSON nếu là API
        }
    }
}
