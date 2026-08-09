using System.ComponentModel.DataAnnotations;
using CBRE.TaskListDemo.Core.Entities;

namespace CBRE.TaskListDemo.Application.Dtos
{
	public record TaskItemDto(
		Guid Id,
		string Title,
		string Description,
		DateTimeOffset? DueDate,
		TaskPriority Priority,
		Guid? CategoryId,
		string? CategoryName,
		TaskItemStatus Status,
		DateTimeOffset CreatedAtUtc,
		DateTimeOffset? UpdatedAtUtc);

	public record CreateTaskRequest(
		[Required, MaxLength(200)] string Title,
		[Required] string Description,
		DateTimeOffset? DueDate,
		TaskPriority Priority,
		Guid? CategoryId,
		TaskItemStatus Status);

	public record UpdateTaskRequest(
		[Required, MaxLength(200)] string Title,
		[Required] string Description,
		DateTimeOffset? DueDate,
		TaskPriority Priority,
		Guid? CategoryId,
		TaskItemStatus Status);
}
