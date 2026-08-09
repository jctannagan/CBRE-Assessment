using System.Security.Claims;
using CBRE.TaskListDemo.Application.Dtos;
using CBRE.TaskListDemo.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CBRE.TaskListDemo.Api.Controllers
{
	[ApiController]
	[Route("api/users")]
	[Authorize]
	public class UserController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		[HttpGet("me")]
		public async Task<ActionResult<UserDto>> Me()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (userId == null)
				return Unauthorized();

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound();

			var roles = await _userManager.GetRolesAsync(user);
			return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles);
		}
	}
}
