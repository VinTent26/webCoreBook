using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using webCore.MongoHelper;
using System.Linq;
using CloudinaryDotNet;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class Admin_productController : Controller
    {
        private readonly CategoryProduct_adminService _CategoryProductCollection;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<Admin_productController> _logger;

        public Admin_productController(CategoryProduct_adminService Category_adminService, CloudinaryService cloudinaryService, ILogger<Admin_productController> logger)
        {
            _CategoryProductCollection = Category_adminService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var productName = HttpContext.Session.GetString("ProductName");
            ViewBag.ProductName = productName;

            try
            {
                var products = await _CategoryProductCollection.GetProduct();
                var categories = await _CategoryProductCollection.GetCategory(); // Lấy danh sách danh mục

                // Tạo một từ điển để ánh xạ CategoryId đến CategoryTitle
                var categoryDictionary = categories.ToDictionary(c => c.Id, c => c.Title);



                // Gán CategoryTitle cho từng sản phẩm
                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.CategoryId) && categoryDictionary.TryGetValue(product.CategoryId, out var categoryTitle))
                    {
                        product.CategoryTitle = categoryTitle; // Gán title từ từ điển
                    }
                }

                // Sắp xếp sản phẩm theo vị trí
                var sortedProducts = products.OrderBy(c => c.Position).ToList();
                ViewBag.Products = sortedProducts; // Gán danh sách sản phẩm đã sắp xếp vào ViewBag
                return View(sortedProducts); // Trả về view với danh sách sản phẩm đã sắp xếp
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from MongoDB.");
                return View("Error");
            }
        }
        public IActionResult Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var productName = HttpContext.Session.GetString("ProductName");
            ViewBag.ProductName = productName;
            ViewBag.Products = _CategoryProductCollection.GetProduct().Result;
            ViewBag.Categories = _CategoryProductCollection.GetCategory().Result;
            var hierarchicalCategories = GetHierarchicalCategories(ViewBag.Categories);
            ViewBag.Categories = hierarchicalCategories;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product_admin product, IFormFile Image, string categoryid)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem có sản phẩm nào đã tồn tại với tiêu đề này không
                var existingProduct = (await _CategoryProductCollection.GetProduct())
                    .FirstOrDefault(a => a.Title == product.Title);

                if (existingProduct != null)
                {
                    ModelState.AddModelError("Tên sản phẩm", "Đã có sản phẩm này.");
                    ViewBag.Categories = await _CategoryProductCollection.GetCategory();
                    return View(product);
                }

                product.Id = Guid.NewGuid().ToString();
                product.CategoryId = categoryid;

                // Lấy CategoryTitle từ danh mục
                var category = await _CategoryProductCollection.GetCategoryByIdAsync(categoryid);
                if (category != null)
                {
                    product.CategoryTitle = category.Title; // Gán CategoryTitle
                }
                else
                {
                    ModelState.AddModelError("CategoryId", "Danh mục không hợp lệ.");
                }

                var products = await _CategoryProductCollection.GetProduct();
                int maxPosition = products.Any() ? products.Max(c => c.Position) : 0;
                product.Position = maxPosition + 1;

                if (Image != null && Image.Length > 0)
                {
                    try
                    {
                        product.Image = await _cloudinaryService.UploadImageAsync(Image);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image to Cloudinary.");
                        ViewBag.Categories = await _CategoryProductCollection.GetCategory();
                        ModelState.AddModelError("", "Failed to upload image. Please try again.");
                        return View(product);
                    }
                }

                try
                {
                    await _CategoryProductCollection.SaveProductAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving product to MongoDB.");
                    ModelState.AddModelError("", "Could not save product to database. Please try again.");
                }
            }

            // Gán lại danh sách danh mục vào ViewBag nếu ModelState không hợp lệ
            ViewBag.Categories = await _CategoryProductCollection.GetCategory();
            return View(product);
        }
        public async Task<IActionResult> Update(string id)
        {

            var product = await _CategoryProductCollection.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var productName = HttpContext.Session.GetString("ProductName");
            ViewBag.ProductName = productName;
            ViewBag.Products = await _CategoryProductCollection.GetProduct();
            ViewBag.Categories = await _CategoryProductCollection.GetCategory();
            var hierarchicalCategories = GetHierarchicalCategories(ViewBag.Categories);
            ViewBag.Categories = hierarchicalCategories;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Product_admin product, IFormFile Image, string categoryId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing category from the database to retain its Position
                    var existingProduct = await _CategoryProductCollection.GetProductByIdAsync(product.Id);
                    if (existingProduct == null)
                    {
                        return NotFound(); // If the category doesn't exist, return 404
                    }

                    // Preserve the Position from the existing category
                    product.Position = existingProduct.Position;
                    var category = await _CategoryProductCollection.GetCategoryByIdAsync(product.CategoryId);
                    product.CategoryTitle = category?.Title;

                    // Update the category in the database
                    await _CategoryProductCollection.UpdateProductAsync(product);

                    if (Image != null && Image.Length > 0)
                    {
                        try
                        {
                            product.Image = await _cloudinaryService.UploadImageAsync(Image);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading image to Cloudinary.");
                            ModelState.AddModelError("", "Failed to upload image. Please try again.");
                            ViewBag.Products = await _CategoryProductCollection.GetProduct();
                            return View(product);
                        }
                    }
                    else
                    {
                        // Nếu không có hình ảnh mới, giữ hình ảnh hiện tại
                        product.Image = existingProduct.Image;
                    }

                    // Cập nhật sản phẩm trong cơ sở dữ liệu
                    await _CategoryProductCollection.UpdateProductAsync(product);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product in MongoDB.");
                    ModelState.AddModelError("", "Could not update product in database. Please try again.");
                    return View(product);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = await _CategoryProductCollection.GetProduct();
            ViewBag.Categories = await _CategoryProductCollection.GetCategory();
            return View(product);
        }
        // POST: Admin_category/DeleteConfirmed/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("", "Product ID is invalid.");
                return RedirectToAction(nameof(Index));
            }

            try
            {

                var productToDelete = await _CategoryProductCollection.GetProductByIdAsync(id);
                if (productToDelete == null)
                {
                    return NotFound(); 
                }

                await _CategoryProductCollection.DeleteProductAsync(id);

                var remainingProducts = await _CategoryProductCollection.GetProduct();

                for (int i = 0; i < remainingProducts.Count; i++)
                {
                    remainingProducts[i].Position = i + 1; // Set position starting from 1
                    await _CategoryProductCollection.UpdateProductAsync(remainingProducts[i]); 
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Product not found or already deleted.");
                ModelState.AddModelError("", "The Product was not found or has already been deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Product from MongoDB.");
                ModelState.AddModelError("", "Could not delete Product from database. Please try again.");
            }

            return RedirectToAction(nameof(Index)); 
        }

        private List<Category_admin> GetHierarchicalCategories(List<Category_admin> categories, string parentId = null, int level = 0)
        {
            var result = new List<Category_admin>();

            foreach (var category in categories.Where(c => c.ParentId == parentId))
            {
                // Thêm dấu gạch ngang để thể hiện cấp bậc
                category.Title = new string('-', level * 2) + " " + category.Title;
                result.Add(category);
                // Đệ quy để lấy danh mục con
                result.AddRange(GetHierarchicalCategories(categories, category.Id, level + 1));
            }

            return result;
        }
    }

}

