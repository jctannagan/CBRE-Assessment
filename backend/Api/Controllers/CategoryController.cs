using System.Security.Claims;
using CBRE.TaskListDemo.Application.Dtos;
using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CBRE.TaskListDemo.Api.Controllers
{
	[ApiController]
	[Route("api/categories")]
	[Authorize]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
		{
			var categories = await _categoryService.GetCategoriesForUserAsync(UserId);
			return Ok(categories.Select(ToDto));
		}

		[HttpPost]
		public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request)
		{
			var created = await _categoryService.CreateCategoryAsync(request.Name, UserId);
			return CreatedAtAction(nameof(GetAll), ToDto(created));
		}

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var deleted = await _categoryService.DeleteCategoryAsync(id, UserId);
			if (!deleted)
				return NotFound();

			return NoContent();
		}

		private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

		private static CategoryDto ToDto(Category category) => new(category.Id, category.Name, category.UserId == null);
	}
}
