namespace CBRE.TaskListDemo.Application.Dtos
{
	public record RegisterRequest(string Email, string Password, string? FirstName, string? LastName);

	public record LoginRequest(string Email, string Password);

	public record GoogleLoginRequest(string IdToken);

	public record ForgotPasswordRequest(string Email);

	public record ResetPasswordRequest(string Email, string Token, string NewPassword);

	public record AuthResponse(string Token, DateTime ExpiresAtUtc, UserDto User);

	public record UserDto(string Id, string Email, string? FirstName, string? LastName, IList<string> Roles);
}
