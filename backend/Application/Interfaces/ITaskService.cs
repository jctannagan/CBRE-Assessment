using CBRE.TaskListDemo.Core.Entities;

namespace CBRE.TaskListDemo.Application.Interfaces
{
	public interface ITaskService
	{
		Task<IReadOnlyList<TaskItem>> GetTasksForUserAsync(string userId);
		Task<TaskItem?> GetTaskAsync(Guid id, string userId);
		Task<TaskItem> CreateTaskAsync(TaskItem task);
		Task<bool> UpdateTaskAsync(TaskItem task, string userId);
		Task<bool> DeleteTaskAsync(Guid id, string userId);
	}
}
