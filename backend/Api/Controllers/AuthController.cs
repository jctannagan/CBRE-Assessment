using CBRE.TaskListDemo.Application.Dtos;
using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CBRE.TaskListDemo.Api.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ITokenService _tokenService;
		private readonly IConfiguration _config;

		public AuthController(
			UserManager<ApplicationUser> userManager,
			ITokenService tokenService,
			IConfiguration config)
		{
			_userManager = userManager;
			_tokenService = tokenService;
			_config = config;
		}

		[HttpPost("register")]
		public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
		{
			var existing = await _userManager.FindByEmailAsync(request.Email);
			if (existing != null)
				return Conflict(new { message = "An account with this email already exists." });

			var user = new ApplicationUser
			{
				UserName = request.Email,
				Email = request.Email,
				FirstName = request.FirstName,
				LastName = request.LastName
			};

			var result = await _userManager.CreateAsync(user, request.Password);
			if (!result.Succeeded)
				return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

			// Optional: assign a default role on signup
			await _userManager.AddToRoleAsync(user, "User");

			return await BuildAuthResponse(user);
		}

		[HttpPost("login")]
		public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null)
				return Unauthorized(new { message = "Invalid credentials." });

			// CheckPasswordAsync is used directly (rather than SignInManager) since
			// this is an API and we don't want cookie-based sign-in behavior.
			var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
			if (!passwordValid)
				return Unauthorized(new { message = "Invalid credentials." });

			return await BuildAuthResponse(user);
		}

		[HttpPost("google")]
		public async Task<ActionResult<AuthResponse>> GoogleSSOLogin(GoogleLoginRequest request)
		{
			GoogleJsonWebSignature.Payload payload;
			try
			{
				payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
				{
					Audience = new[] { _config["Google:ClientId"] }
				});
			}
			catch (InvalidJwtException)
			{
				return Unauthorized(new { message = "Invalid Google token." });
			}

			var user = await _userManager.FindByEmailAsync(payload.Email);
			if (user == null)
			{
				user = new ApplicationUser
				{
					UserName = payload.Email,
					Email = payload.Email,
					EmailConfirmed = payload.EmailVerified,
					FirstName = payload.GivenName,
					LastName = payload.FamilyName
				};

				// No password: this account can only be accessed via Google login
				// unless the user later sets a password through a "set password" flow.
				var createResult = await _userManager.CreateAsync(user);
				if (!createResult.Succeeded)
					return BadRequest(new { errors = createResult.Errors.Select(e => e.Description) });

				await _userManager.AddToRoleAsync(user, "User");
			}

			// Link the external login if not already linked, so FindByLoginAsync works later
			var logins = await _userManager.GetLoginsAsync(user);
			if (!logins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == payload.Subject))
			{
				await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", payload.Subject, "Google"));
			}

			return await BuildAuthResponse(user);
		}

		[HttpPost("refresh")]
		public async Task<ActionResult<AuthResponse>> Refresh()
		{
			var incomingToken = Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(incomingToken))
				return Unauthorized(new { message = "No refresh token provided." });

			var existing = await _tokenService.GetRefreshTokenAsync(incomingToken);
			if (existing == null || !existing.IsActive)
				return Unauthorized(new { message = "Invalid or expired refresh token." });

			var user = await _userManager.FindByIdAsync(existing.UserId);
			if (user == null)
				return Unauthorized(new { message = "Invalid refresh token." });

			var roles = await _userManager.GetRolesAsync(user);
			var (accessToken, expiresAtUtc) = _tokenService.GenerateAccessToken(user, roles);

			// Rotate: issue a new refresh token and revoke the one just used.
			var newRefreshToken = await _tokenService.IssueRefreshTokenAsync(user.Id);
			await _tokenService.RevokeRefreshTokenAsync(existing, newRefreshToken.Token);
			SetRefreshTokenCookie(newRefreshToken.Token, newRefreshToken.ExpiresAtUtc);

			var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles);
			return new AuthResponse(accessToken, expiresAtUtc, userDto);
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			var incomingToken = Request.Cookies["refreshToken"];
			if (!string.IsNullOrEmpty(incomingToken))
			{
				var existing = await _tokenService.GetRefreshTokenAsync(incomingToken);
				if (existing != null && existing.RevokedAtUtc == null)
					await _tokenService.RevokeRefreshTokenAsync(existing);
			}

			Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });
			return NoContent();
		}

		private async Task<AuthResponse> BuildAuthResponse(ApplicationUser user)
		{
			var roles = await _userManager.GetRolesAsync(user);
			var (accessToken, expiresAtUtc) = _tokenService.GenerateAccessToken(user, roles);

			var refreshToken = await _tokenService.IssueRefreshTokenAsync(user.Id);
			SetRefreshTokenCookie(refreshToken.Token, refreshToken.ExpiresAtUtc);

			var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles);
			return new AuthResponse(accessToken, expiresAtUtc, userDto);
		}

		private void SetRefreshTokenCookie(string token, DateTime expiresAtUtc)
		{
			Response.Cookies.Append("refreshToken", token, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = expiresAtUtc,
				Path = "/api/auth",
			});
		}
	}
}
