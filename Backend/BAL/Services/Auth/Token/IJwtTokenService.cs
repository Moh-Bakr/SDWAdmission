using System.IdentityModel.Tokens.Jwt;
using DAL;

namespace BAL;

public interface IJwtTokenService
{
	Task<JwtSecurityToken> CreateTokenAsync(ApplicationUser user);
}