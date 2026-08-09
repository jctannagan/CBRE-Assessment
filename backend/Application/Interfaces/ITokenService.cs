using CBRE.TaskListDemo.Core.Entities;

namespace CBRE.TaskListDemo.Application.Interfaces
{
	public interface ITokenService
	{
		(string Token, DateTime ExpiresAtUtc) GenerateAccessToken(ApplicationUser user, IList<string> roles);
		Task<RefreshToken> IssueRefreshTokenAsync(string userId);
		Task<RefreshToken?> GetRefreshTokenAsync(string token);
		Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? replacedByToken = null);
	}
}
