using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using CBRE.TaskListDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CBRE.TaskListDemo.Infrastructure.Services
{
	public class TaskService : ITaskService
	{
		private readonly ApplicationDbContext _db;

		public TaskService(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IReadOnlyList<TaskItem>> GetTasksForUserAsync(string userId)
		{
			return await _db.Tasks
				.Include(t => t.Category)
				.Where(t => t.UserId == userId)
				.OrderBy(t => t.DueDate)
				.ToListAsync();
		}

		public async Task<TaskItem?> GetTaskAsync(Guid id, string userId)
		{
			return await _db.Tasks
				.Include(t => t.Category)
				.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
		}

		public async Task<TaskItem> CreateTaskAsync(TaskItem task)
		{
			task.Id = Guid.NewGuid();
			task.CreatedAtUtc = DateTimeOffset.UtcNow;
			task.UpdatedAtUtc = null;

			_db.Tasks.Add(task);
			await _db.SaveChangesAsync();

			if (task.CategoryId != null)
				await _db.Entry(task).Reference(t => t.Category).LoadAsync();

			return task;
		}

		public async Task<bool> UpdateTaskAsync(TaskItem task, string userId)
		{
			var existing = await GetTaskAsync(task.Id, userId);
			if (existing == null)
				return false;

			existing.Title = task.Title;
			existing.Description = task.Description;
			existing.DueDate = task.DueDate;
			existing.Priority = task.Priority;
			existing.CategoryId = task.CategoryId;
			existing.Status = task.Status;
			existing.UpdatedAtUtc = DateTimeOffset.UtcNow;

			await _db.SaveChangesAsync();
			return true;
		}

		public async Task<bool> DeleteTaskAsync(Guid id, string userId)
		{
			var existing = await GetTaskAsync(id, userId);
			if (existing == null)
				return false;

			_db.Tasks.Remove(existing);
			await _db.SaveChangesAsync();
			return true;
		}
	}
}
