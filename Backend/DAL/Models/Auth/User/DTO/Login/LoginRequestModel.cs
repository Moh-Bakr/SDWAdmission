using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DAL;

public class LoginRequestModel
{
	public string Email { get; set; }
	public string Password { get; set; }
}

public class LoginModelValidator : AbstractValidator<LoginRequestModel>
{
	private readonly UserManager<ApplicationUser> _userManager;

	public LoginModelValidator(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;
		RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.");

		RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
		// RuleFor(x => x).CustomAsync(async (model, context, cancellationToken) =>
		// {
		// 	var user = await _userManager.FindByEmailAsync(model.Email);
		// 	if (user == null)
		// 	{
		// 		context.AddFailure(nameof(model.Email), "Invalid email or password");
		// 		return;
		// 	}
		//
		// 	var passwordIsValid = await _userManager.CheckPasswordAsync(user, model.Password);
		// 	if (!passwordIsValid)
		// 	{
		// 		context.AddFailure(nameof(model.Password), "Invalid email or password");
		// 		return;
		// 	}
		// });
	}
}

public static class LoginModelExtensions
{
	public static async Task<string> ValidateToJsonAsync(this LoginRequestModel model,
		UserManager<ApplicationUser> userManager)
	{
		var validator = new LoginModelValidator(userManager);
		var result = await validator.ValidateAsync(model);
		var errors = result.Errors.Select(x => x.ErrorMessage);
		return JsonSerializer.Serialize(errors, new JsonSerializerOptions
		{
			WriteIndented = true,
		}).Replace("\r\n", "\n");
	}
}