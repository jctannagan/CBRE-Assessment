using Microsoft.AspNetCore.Identity;

namespace CBRE.TaskListDemo.Core.Entities
{
	public class ApplicationUser : IdentityUser
	{
		/// <summary>
		/// User's first name
		/// </summary>
		public string? FirstName { get; set; }

		/// <summary>
		/// User's last name
		/// </summary>
		public string? LastName { get; set; }

		/// <summary>
		/// User's full display name
		/// </summary>
		public string? FullName
		{
			get
			{
				if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName))
				{
					return UserName;
				}
				return $"{FirstName} {LastName}".Trim();
			}
		}

		/// <summary>
		/// Date the user account was created
		/// </summary>
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
