using FluentValidation;
using System.Text.Json;
using DAL;
using Microsoft.AspNetCore.Identity;

public class ApplicationUserDto
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string Username { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
}

public class RegisterModelValidator : AbstractValidator<ApplicationUserDto>
{
	private readonly UserManager<ApplicationUser> _userManager;

	public RegisterModelValidator(UserManager<ApplicationUser> userManager)
	{
		_userManager = userManager;

		RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.")
			.MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required.")
			.Matches(@"^[a-zA-Z0-9_-]{4,16}$")
			.WithMessage("Username must be 4-16 characters and can only contain letters, numbers, underscores, and dashes.")
			.MustAsync(IsUsernameUnique).WithMessage("{PropertyName} is already taken.");

		RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.")
			.MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.")
			.MustAsync(IsEmailUnique).WithMessage("Email is already registered.");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(8).WithMessage("Password must be at least 8 characters.")
			.Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
			.WithMessage(
				"Password must contain at least 1 uppercase letter, 1 lowercase letter, 1 number, and 1 special character.")
			.NotEqual(x => x.Username).WithMessage("Password must not be the same as username.");
	}

	private async Task<bool> IsEmailUnique(string email, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByEmailAsync(email);
		return user == null;
	}

	private async Task<bool> IsUsernameUnique(string username, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByNameAsync(username);
		return user == null;
	}
}

public static class RegisterModelExtensions
{
	public static async Task<string> ValidateToJsonAsync(this ApplicationUserDto model,
		UserManager<ApplicationUser> userManager)
	{
		var validator = new RegisterModelValidator(userManager);
		var result = await validator.ValidateAsync(model);
		var errors = result.Errors.Select(x => x.ErrorMessage);
		return JsonSerializer.Serialize(errors, new JsonSerializerOptions
		{
			WriteIndented = true,
		}).Replace("\r\n", "\n");
	}
}