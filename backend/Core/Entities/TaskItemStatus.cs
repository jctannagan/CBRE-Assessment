namespace CBRE.TaskListDemo.Core.Entities
{
	// Named TaskItemStatus (not TaskStatus) to avoid colliding with System.Threading.Tasks.TaskStatus,
	// which is in scope via ImplicitUsings on the Web SDK.
	public enum TaskItemStatus
	{
		ToDo,
		InProgress,
		Completed
	}
}
