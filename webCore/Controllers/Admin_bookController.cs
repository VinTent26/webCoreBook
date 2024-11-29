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

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class Admin_bookController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly ILogger<Admin_bookController> _logger;

        public Admin_bookController(MongoDBService mongoDBService, ILogger<Admin_bookController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var bookName = HttpContext.Session.GetString("BookName");
            ViewBag.BookName = bookName;

            try
            {
                var books = await _mongoDBService.GetBook();
                foreach (var book in books)
                {
                    if (!string.IsNullOrEmpty(book.CategoryId))
                    {
                        var parentCategory = books.FirstOrDefault(c => c.Id == book.CategoryId);
                        book.CategoryTitle = parentCategory?.Title;
                    }
                }

                var sortedBooks = books.OrderBy(c => c.Position).ToList();
                return View(sortedBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching books from MongoDB.");
                return View("Error");
            }
        }

        public IActionResult Create()
        {

            var bookName = HttpContext.Session.GetString("BooksName");
            ViewBag.BookName = bookName;
            ViewBag.Books = _mongoDBService.GetBook().Result;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book_admin book, IFormFile Avatar, string categoryid)
        {
            if (ModelState.IsValid)
            {
                var existingBook = (await _mongoDBService.GetBook())
                    .FirstOrDefault(a => a.Title == book.Title);

                if (existingBook != null)
                {
                    ModelState.AddModelError("Tên sản phẩm", "Đã có sản phẩm này.");
                    return View(book);
                }

                book.Id = Guid.NewGuid().ToString();
                book.CategoryId = categoryid;

                var books = await _mongoDBService.GetBook();
                int maxPosition = books.Any() ? books.Max(c => c.Position) : 0;
                book.Position = maxPosition + 1;

                try
                {
                    await _mongoDBService.SaveBookAsync(book);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving book to MongoDB.");
                    ModelState.AddModelError("", "Could not save category to database. Please try again.");
                    return View(book);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _mongoDBService.GetCategory();
            return View(book);
        }
    }
}

