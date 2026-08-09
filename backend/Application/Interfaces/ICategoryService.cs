using CBRE.TaskListDemo.Core.Entities;

namespace CBRE.TaskListDemo.Application.Interfaces
{
	public interface ICategoryService
	{
		// Returns global categories plus the given user's own.
		Task<IReadOnlyList<Category>> GetCategoriesForUserAsync(string userId);
		Task<Category> CreateCategoryAsync(string name, string userId);

		// Only the owning user can delete their own category; global categories cannot be deleted this way.
		Task<bool> DeleteCategoryAsync(Guid id, string userId);

		// True if the category is global or owned by the given user.
		Task<bool> IsAccessibleAsync(Guid categoryId, string userId);
	}
}
