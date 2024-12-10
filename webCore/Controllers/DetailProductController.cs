using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class DetailProductController : Controller
    {
        private readonly MongoDBService _mongoDBService;


        private readonly DetailProductService _detailProductService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public DetailProductController(MongoDBService mongoDBService, DetailProductService detailProductService, ProductService productService, CategoryService categoryService)
        {
            _mongoDBService = mongoDBService;
            _detailProductService = detailProductService;
            _productService = productService;
            _categoryService = categoryService;
        }
        // Phương thức tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("_ProductList", new List<Product_admin>());
            }

            // Tìm kiếm sản phẩm từ MongoDB
            var allProducts = await _productService.GetProductsAsync();
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_ProductList", searchResults);
        }

        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> DetailProduct(string id)
        {
            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Product ID is required.");
            }

            var product = await _detailProductService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Lấy các sản phẩm tương tự
            var similarProducts = await GetSimilarProducts(product);

            // Gán danh sách sản phẩm tương tự vào ViewBag
            ViewBag.SimilarProducts = similarProducts;

            // Lấy thông tin phiên người dùng từ session (nếu có)
            var userName = HttpContext.Session.GetString("UserName"); // Lấy tên người dùng từ session
            var userToken = HttpContext.Session.GetString("UserToken"); // Lấy token từ session

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userToken))
            {
                // Người dùng đã đăng nhập
                ViewBag.UserName = userName;
                ViewBag.UserToken = userToken;
            }
            else
            {
                // Người dùng chưa đăng nhập
                ViewBag.UserName = null;
                ViewBag.UserToken = null;
            }

            var breadcrumbs = new List<Category>();

            // Thêm "Home" vào đầu breadcrumb
            breadcrumbs.Add(new Category { Title = "Trang chủ", _id = "home" });

            string currentCategoryId = product.CategoryId;

            // Kiểm tra nếu CategoryId là null hoặc rỗng
            if (string.IsNullOrEmpty(currentCategoryId))
            {
                return NotFound("Category not found.");
            }

            // Lấy breadcrumb của các danh mục cha
            while (!string.IsNullOrEmpty(currentCategoryId))
            {
                var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                if (category != null)
                {
                    breadcrumbs.Insert(1, category); // Thêm vào **sau "Trang chủ"**
                    currentCategoryId = category.ParentId; // Lấy danh mục cha
                }
                else
                {
                    return NotFound("Category breadcrumb not found.");
                }
            }

            // Lưu danh sách breadcrumb vào ViewBag
            ViewBag.Breadcrumbs = breadcrumbs;

            // Trả về view và truyền thông tin sản phẩm
            return View(product);
        }


        [HttpGet("api/product/breadcrumbs/{productId}")]
        public async Task<IActionResult> GetProductBreadcrumbs(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest("Product ID is required.");
            }

            // Lấy thông tin sản phẩm
            var product = await _detailProductService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Truy vấn danh mục breadcrumb từ categoryId của sản phẩm
            var breadcrumbs = new List<Category>();

            string currentCategoryId = product.CategoryId;

            // Lấy breadcrumb của các danh mục cha
            while (!string.IsNullOrEmpty(currentCategoryId))
            {
                var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                if (category != null)
                {
                    breadcrumbs.Insert(0, category); // Thêm vào **đầu danh sách**
                    currentCategoryId = category.ParentId; // Lấy danh mục cha
                }
                else
                {
                    break;
                }
            }

            return Ok(new
            {
                Product = new
                {
                    product.Id,
                    product.Title,
                    product.Description,
                    product.Price,
                    product.Image
                },
                Breadcrumbs = breadcrumbs
            });
        }
        // Phương thức để lấy các sản phẩm tương tự
        private async Task<List<Product_admin>> GetSimilarProducts(Product_admin product)
        {
            // Lấy tất cả sản phẩm cùng danh mục
            var similarProducts = await _detailProductService.GetProductsByCategoryAsync(product.CategoryId);

            // Loại bỏ sản phẩm hiện tại khỏi danh sách
            similarProducts = similarProducts.Where(p => p.Id != product.Id).ToList();

            // Lấy tối đa 10 sản phẩm tương tự
            return similarProducts.Take(10).ToList();
        }

    }
}
