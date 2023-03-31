using DAL;
using Microsoft.AspNetCore.Identity;

namespace BAL;

public interface IAuthService
{
	Task<AuthModel> RegisterAsync(ApplicationUserDto model);
	Task<AuthModel> LoginAsync(LoginRequestModel model);
	Task<IEnumerable<ReturnUsersDto>> GetAllUsersAsync();
	Task<AuthModel> AssignRoleAsync(AssignRoleRequestModel model);
}