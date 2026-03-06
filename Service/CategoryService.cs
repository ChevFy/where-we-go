using Microsoft.EntityFrameworkCore;

using where_we_go.Database;
using where_we_go.DTO;
using where_we_go.Models;

namespace where_we_go.Service
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(Guid id);
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
        Task<CategoryDto?> UpdateAsync(Guid id, CreateCategoryDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

    public class CategoryService : BaseService, ICategoryService
    {
        private readonly AppDbContext _dbContext;

        public CategoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(Guid id)
        {
            var category = await _dbContext.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return null;

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            // Check if category name already exists
            var existingCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (existingCategory != null)
            {
                throw new InvalidOperationException("A category with this name already exists");
            }

            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto?> UpdateAsync(Guid id, CreateCategoryDto dto)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return null;
            }

            // Check if category name already exists (excluding current category)
            var duplicateCategory = await _dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.CategoryId != id);

            if (duplicateCategory != null)
            {
                throw new InvalidOperationException("A category with this name already exists");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _dbContext.SaveChangesAsync();

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            // Remove category from all related posts (cascade delete from join table)
            var postsWithCategory = await _dbContext.Posts
                .Include(p => p.Categories)
                .Where(p => p.Categories.Any(c => c.CategoryId == id))
                .ToListAsync();

            foreach (var post in postsWithCategory)
            {
                var categoryToRemove = post.Categories.FirstOrDefault(c => c.CategoryId == id);
                if (categoryToRemove != null)
                {
                    post.Categories.Remove(categoryToRemove);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Now delete the category
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            return true;
        }
    }
}
