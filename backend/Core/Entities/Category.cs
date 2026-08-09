namespace CBRE.TaskListDemo.Core.Entities
{
	public class Category
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;

		// Null = global/default category, visible to everyone.
		// Set = a personal category owned by that user.
		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }
	}
}
