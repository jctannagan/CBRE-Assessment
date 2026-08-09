using System.Security.Claims;
using CBRE.TaskListDemo.Application.Dtos;
using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CBRE.TaskListDemo.Api.Controllers
{
	[ApiController]
	[Route("api/tasks")]
	[Authorize]
	public class TaskController : ControllerBase
	{
		private readonly ITaskService _taskService;
		private readonly ICategoryService _categoryService;

		public TaskController(ITaskService taskService, ICategoryService categoryService)
		{
			_taskService = taskService;
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAll()
		{
			var tasks = await _taskService.GetTasksForUserAsync(UserId);
			return Ok(tasks.Select(ToDto));
		}

		[HttpGet("{id:guid}")]
		public async Task<ActionResult<TaskItemDto>> GetById(Guid id)
		{
			var task = await _taskService.GetTaskAsync(id, UserId);
			if (task == null)
				return NotFound();

			return ToDto(task);
		}

		[HttpPost]
		public async Task<ActionResult<TaskItemDto>> Create(CreateTaskRequest request)
		{
			if (request.CategoryId != null && !await _categoryService.IsAccessibleAsync(request.CategoryId.Value, UserId))
				return BadRequest(new { message = "Invalid category." });

			var task = new TaskItem
			{
				Title = request.Title,
				Description = request.Description,
				DueDate = request.DueDate,
				Priority = request.Priority,
				CategoryId = request.CategoryId,
				Status = request.Status,
				UserId = UserId
			};

			var created = await _taskService.CreateTaskAsync(task);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
		}

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request)
		{
			if (request.CategoryId != null && !await _categoryService.IsAccessibleAsync(request.CategoryId.Value, UserId))
				return BadRequest(new { message = "Invalid category." });

			var task = new TaskItem
			{
				Id = id,
				Title = request.Title,
				Description = request.Description,
				DueDate = request.DueDate,
				Priority = request.Priority,
				CategoryId = request.CategoryId,
				Status = request.Status
			};

			var updated = await _taskService.UpdateTaskAsync(task, UserId);
			if (!updated)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var deleted = await _taskService.DeleteTaskAsync(id, UserId);
			if (!deleted)
				return NotFound();

			return NoContent();
		}

		private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

		private static TaskItemDto ToDto(TaskItem task) => new(
			task.Id,
			task.Title,
			task.Description,
			task.DueDate,
			task.Priority,
			task.CategoryId,
			task.Category?.Name,
			task.Status,
			task.CreatedAtUtc,
			task.UpdatedAtUtc);
	}
}
