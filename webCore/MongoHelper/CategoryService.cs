using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public CategoryService(MongoDBService mongoDBService)
        {
            _categoryCollection = mongoDBService._categoryCollection;
        }

        // Lấy danh mục gốc có trạng thái "Hoạt động"
        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, null)
                         & Builders<Category>.Filter.Eq(c => c.Status, "Hoạt động");

            return await _categoryCollection.Find(filter).ToListAsync();
        }

        // Lấy danh mục con theo ParentId và trạng thái "Hoạt động"
        public async Task<List<Category>> GetSubCategoriesByParentIdAsync(string parentId)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, parentId)
                         & Builders<Category>.Filter.Eq(c => c.Status, "Hoạt động");

            return await _categoryCollection.Find(filter).ToListAsync();
        }

        // Lấy tất cả danh mục có trạng thái "Hoạt động"
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.Status, "Hoạt động");

            return await _categoryCollection.Find(filter).ToListAsync();
        }

        // Lấy breadcrumb theo Id (không lọc theo trạng thái)
        public async Task<Category> GetCategoryBreadcrumbByIdAsync(string categoryId)
        {
            return await _categoryCollection.Find(c => c._id == categoryId).FirstOrDefaultAsync();
        }
    }
}
