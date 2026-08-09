using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using CBRE.TaskListDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CBRE.TaskListDemo.Infrastructure.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ApplicationDbContext _db;

		public CategoryService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IReadOnlyList<Category>> GetCategoriesForUserAsync(string userId)
		{
			return await _db.Categories
				.Where(c => c.UserId == null || c.UserId == userId)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		public async Task<Category> CreateCategoryAsync(string name, string userId)
		{
			var category = new Category
			{
				Id = Guid.NewGuid(),
				Name = name,
				UserId = userId
			};

			_db.Categories.Add(category);
			await _db.SaveChangesAsync();

			return category;
		}

		public async Task<bool> DeleteCategoryAsync(Guid id, string userId)
		{
			var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
			if (category == null)
				return false;

			_db.Categories.Remove(category);
			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<bool> IsAccessibleAsync(Guid categoryId, string userId)
		{
			return await _db.Categories.AnyAsync(c => c.Id == categoryId && (c.UserId == null || c.UserId == userId));
		}
	}
}
