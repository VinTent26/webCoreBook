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
        public async Task<List<Category>> GetRootCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, null);
            return await _categoryCollection.Find(filter).ToListAsync();
        }

        // Lấy danh mục con theo ParentId
        public async Task<List<Category>> GetSubCategoriesByParentIdAsync(string parentId)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false)
                         & Builders<Category>.Filter.Eq(c => c.ParentId, parentId);
            return await _categoryCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Deleted, false);
            return await _categoryCollection.Find(filter).ToListAsync();
        }
        public async Task<Category> GetCategoryBreadcrumbByIdAsync(string categoryId)
        {
            return await _categoryCollection.Find(c => c._id == categoryId).FirstOrDefaultAsync();
        }
    }
}
