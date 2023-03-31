using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using DAL;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BAL
{
	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IJwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;
		private readonly RegisterModelValidator _registerModelValidator;
		private readonly LoginModelValidator _loginModelValidator;
		private readonly AssignRoleRequestModelValidator _assignRoleRequestModelValidator;

		public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
			IJwtTokenService jwtTokenService,
			IMapper mapper, RegisterModelValidator registerModelValidator, LoginModelValidator loginModelValidator,
			AssignRoleRequestModelValidator assignRoleRequestModelValidator)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
			_registerModelValidator = registerModelValidator;
			_loginModelValidator = loginModelValidator;
			_assignRoleRequestModelValidator = assignRoleRequestModelValidator;
		}

		public async Task<AuthModel> RegisterAsync(ApplicationUserDto model)
		{
			ValidationResult validationResult = await _registerModelValidator.ValidateAsync(model);
			if (!validationResult.IsValid)
			{
				var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return new AuthModel { Errors = errorMessages };
			}

			var user = _mapper.Map<ApplicationUser>(model);

			var result = await _userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				var errors = result.Errors.Select(e => e.Description).ToArray();
				return new AuthModel { Errors = errors };
			}

			await _userManager.AddToRoleAsync(user, "User");

			var jwtSecurityToken = await _jwtTokenService.CreateTokenAsync(user);

			var authModel = _mapper.Map<AuthModel>(user);
			authModel.IsAuthenticated = true;
			authModel.Roles = new List<string> { "User" };
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.ExpiresOn = jwtSecurityToken.ValidTo;

			return authModel;
		}

		public async Task<AuthModel> LoginAsync(LoginRequestModel model)
		{
			ValidationResult validationResult = await _loginModelValidator.ValidateAsync(model);
			if (!validationResult.IsValid)
			{
				var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return new AuthModel { Errors = errorMessages };
			}

			var authModel = new AuthModel();
			var user = await _userManager.FindByEmailAsync(model.Email);

			if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
			{
				authModel.Errors = new[] { "Invalid email or password" };
				return authModel;
			}

			var jwtSecurityToken = await _jwtTokenService.CreateTokenAsync(user);
			var roles = await _userManager.GetRolesAsync(user);

			authModel.IsAuthenticated = true;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			authModel.Email = user.Email;
			authModel.Username = user.UserName;
			authModel.Roles = roles.ToList();

			return authModel;
		}

		public async Task<IEnumerable<ReturnUsersDto>> GetAllUsersAsync()
		{
			var users = await _userManager.Users.ToListAsync();
			var userDtos = _mapper.Map<IEnumerable<ReturnUsersDto>>(users);
			return userDtos;
		}

		public async Task<AuthModel> AssignRoleAsync(AssignRoleRequestModel model)
		{
			ValidationResult validationResult = await _assignRoleRequestModelValidator.ValidateAsync(model);
			if (!validationResult.IsValid)
			{
				var errorMessages = validationResult.Errors.Select(e => e.ErrorMessage).ToArray();
				return new AuthModel { Errors = errorMessages };
			}

			var user = await _userManager.FindByIdAsync(model.UserId);

			if (user == null || !await _roleManager.RoleExistsAsync(model.Role))
				return new AuthModel { Errors = new[] { "User or role not found" } };

			if (await _userManager.IsInRoleAsync(user, model.Role))
				return new AuthModel { Errors = new[] { "User already has this role" } };

			var result = await _userManager.AddToRoleAsync(user, model.Role);

			return result.Succeeded
				? new AuthModel { IsAuthenticated = true }
				: new AuthModel { Errors = result.Errors.Select(e => e.Description).ToArray() };
		}
	}
}