using System.ComponentModel.DataAnnotations;

namespace CBRE.TaskListDemo.Application.Dtos
{
	public record CategoryDto(Guid Id, string Name, bool IsGlobal);

	public record CreateCategoryRequest([Required, MaxLength(100)] string Name);
}
