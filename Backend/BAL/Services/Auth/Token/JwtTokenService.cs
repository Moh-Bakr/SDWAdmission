using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DAL;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BAL;

public class JwtTokenService : IJwtTokenService
{
	private readonly JWT _jwt;
	private readonly UserManager<ApplicationUser> _userManager;


	public JwtTokenService(IOptions<JWT> jwt, UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
		_jwt = jwt.Value;
	}

	public async Task<JwtSecurityToken> CreateTokenAsync(ApplicationUser user)
	{
		var userClaims = await _userManager.GetClaimsAsync(user);
		var roles = await _userManager.GetRolesAsync(user);
		var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();

		var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim("uid", user.Id)
			}
			.Union(userClaims)
			.Union(roleClaims);

		var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
		var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

		var jwtSecurityToken = new JwtSecurityToken(
			issuer: _jwt.Issuer,
			audience: _jwt.Audience,
			claims: claims,
			expires: DateTime.Now.AddDays(_jwt.DurationInDays),
			signingCredentials: signingCredentials);

		return jwtSecurityToken;
	}
}