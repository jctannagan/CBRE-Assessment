using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CBRE.TaskListDemo.Application.Interfaces;
using CBRE.TaskListDemo.Core.Entities;
using CBRE.TaskListDemo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CBRE.TaskListDemo.Infrastructure.Services
{
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _config;
		private readonly ApplicationDbContext _db;

		public TokenService(IConfiguration config, ApplicationDbContext db)
		{
			_config = config;
			_db = db;
		}

		public (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(ApplicationUser user, IList<string> roles)
		{
			var jwtSection = _config.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, user.Id),
				new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			};

			// Roles go in as standard ClaimTypes.Role entries so [Authorize(Roles = "Admin")] works.
			claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

			var expiresAtUtc = DateTime.UtcNow.AddMinutes(double.Parse(jwtSection["ExpiryMinutes"] ?? "60"));

			var token = new JwtSecurityToken(
				issuer: jwtSection["Issuer"],
				audience: jwtSection["Audience"],
				claims: claims,
				expires: expiresAtUtc,
				signingCredentials: creds
			);

			return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
		}

		public async Task<RefreshToken> IssueRefreshTokenAsync(string userId)
		{
			var expiryDays = double.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "14");

			var refreshToken = new RefreshToken
			{
				Id = Guid.NewGuid(),
				Token = GenerateRefreshTokenValue(),
				UserId = userId,
				CreatedAtUtc = DateTime.UtcNow,
				ExpiresAtUtc = DateTime.UtcNow.AddDays(expiryDays),
			};

			_db.RefreshTokens.Add(refreshToken);
			await _db.SaveChangesAsync();

			return refreshToken;
		}

		public Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			return _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
		}

		public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? replacedByToken = null)
		{
			refreshToken.RevokedAtUtc = DateTime.UtcNow;
			refreshToken.ReplacedByToken = replacedByToken;
			await _db.SaveChangesAsync();
		}

		private static string GenerateRefreshTokenValue()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
		}
	}
}
