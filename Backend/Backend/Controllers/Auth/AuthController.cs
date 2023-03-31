using BAL;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("register")]
	public async Task<IActionResult> RegisterAsync([FromBody] ApplicationUserDto model)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _authService.RegisterAsync(model);

		if (result.HasErrors)
			return BadRequest(new { errors = result.Errors });

		return Ok(new { message = "User Created Successfully", token = result.Token });
	}

	[HttpPost("login")]
	public async Task<IActionResult> LoginAsync([FromBody] LoginRequestModel model)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _authService.LoginAsync(model);

		if (result.HasErrors)
			return BadRequest(new { errors = result.Errors });

		return Ok(new { message = "User Logged in Successfully", token = result.Token });
	}

	[Authorize(Roles = "Admin")]
	[HttpPost("addrole")]
	public async Task<IActionResult> AssignRoleAsync([FromBody] AssignRoleRequestModel model)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var result = await _authService.AssignRoleAsync(model);

		if (result.HasErrors)
			return BadRequest(new { errors = result.Errors });

		return Ok(new { message = "Role Assigned Successfully" });
	}

	[Authorize(Roles = "Admin")]
	[HttpGet("users")]
	public async Task<IActionResult> GetAllUsers()
	{
		var users = await _authService.GetAllUsersAsync();
		return Ok(users);
	}
}