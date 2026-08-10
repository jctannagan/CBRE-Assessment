using CBRE.TaskListDemo.Core.Enums;

namespace CBRE.TaskListDemo.Core.Entities
{
	public class TaskItem
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public DateTimeOffset? DueDate { get; set; }
		public TaskPriority Priority { get; set; }
		public Guid? CategoryId { get; set; }
		public Category? Category { get; set; }
		public TaskItemStatus Status { get; set; }

		public string UserId { get; set; } = null!;
		public ApplicationUser User { get; set; } = null!;

		public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
		public DateTimeOffset? UpdatedAtUtc { get; set; }
	}
}
