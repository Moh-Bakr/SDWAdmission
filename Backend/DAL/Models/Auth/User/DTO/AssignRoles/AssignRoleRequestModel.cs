using System.Text.Json;
using DAL;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace DAL;

public class AssignRoleRequestModel
{
	public string UserId { get; set; }
	public string Role { get; set; }
}

public class AssignRoleRequestModelValidator : AbstractValidator<AssignRoleRequestModel>
{
	public AssignRoleRequestModelValidator()
	{
		RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
		RuleFor(x => x.Role).NotEmpty().WithMessage("Role is required.");
	}
}

public static class AddRoleRequestModelExtensions
{
	public static async Task<string> ValidateToJsonAsync(this AssignRoleRequestModel model)
	{
		var validator = new AssignRoleRequestModelValidator();
		var result = await validator.ValidateAsync(model);
		var errors = result.Errors.Select(x => x.ErrorMessage);
		return JsonSerializer.Serialize(errors, new JsonSerializerOptions
		{
			WriteIndented = true,
		}).Replace("\r\n", "\n");
	}
}