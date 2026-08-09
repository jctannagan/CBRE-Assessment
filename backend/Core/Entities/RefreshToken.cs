using System.ComponentModel.DataAnnotations.Schema;

namespace CBRE.TaskListDemo.Core.Entities
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public string Token { get; set; } = null!;
		public string UserId { get; set; } = null!;
		public ApplicationUser User { get; set; } = null!;
		public DateTime CreatedAtUtc { get; set; }
		public DateTime ExpiresAtUtc { get; set; }
		public DateTime? RevokedAtUtc { get; set; }
		public string? ReplacedByToken { get; set; }

		[NotMapped]
		public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
	}
}
