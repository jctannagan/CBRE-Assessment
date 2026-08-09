using CBRE.TaskListDemo.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CBRE.TaskListDemo.Infrastructure.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
		public DbSet<TaskItem> Tasks => Set<TaskItem>();
		public DbSet<Category> Categories => Set<Category>();

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder); // required - sets up Identity's own table mappings

			builder.Entity<RefreshToken>(entity =>
			{
				entity.HasIndex(rt => rt.Token).IsUnique();
				entity.HasOne(rt => rt.User)
					.WithMany()
					.HasForeignKey(rt => rt.UserId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<TaskItem>(entity =>
			{
				entity.Property(t => t.Title).IsRequired();
				entity.Property(t => t.Description).IsRequired();
				entity.Property(t => t.Priority).HasConversion<string>().IsRequired();
				entity.Property(t => t.Status).HasConversion<string>().IsRequired();

				entity.HasOne(t => t.User)
					.WithMany()
					.HasForeignKey(t => t.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(t => t.Category)
					.WithMany()
					.HasForeignKey(t => t.CategoryId)
					.OnDelete(DeleteBehavior.SetNull);

				entity.HasIndex(t => t.UserId);
			});

			builder.Entity<Category>(entity =>
			{
				entity.Property(c => c.Name).IsRequired();

				entity.HasOne(c => c.User)
					.WithMany()
					.HasForeignKey(c => c.UserId)
					.OnDelete(DeleteBehavior.Cascade);

				// Per-user category names must be unique; global (UserId == null) rows are exempt
				// since Postgres treats NULL as distinct in a unique index.
				entity.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
			});
		}
	}
}
