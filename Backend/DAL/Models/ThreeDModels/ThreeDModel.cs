using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DAL
{
	public class ThreeDModel
	{
		[Key] public int Id { get; set; }

		[Required(ErrorMessage = "The Name field is required.")]
		[MaxLength(100, ErrorMessage = "The Name field cannot exceed 100 characters.")]
		[Display(Name = "Name")]
		[UniqueName(ErrorMessage = "The Name field must be unique.")]
		public string Name { get; set; }

		[Required(ErrorMessage = "The Description field is required.")]
		[Display(Name = "Description")]
		public string Description { get; set; }

		[NotMapped]
		[AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" },
			ErrorMessage = "The Photo field must be a JPG, JPEG, or PNG image.")]
		[MaxFileSize(5 * 1024 * 1024, ErrorMessage = "The Photo field must be no larger than 5 MB.")]
		[Display(Name = "Photo")]
		public IFormFile Photo { get; set; }

		public string PhotoPath { get; set; }

		[NotMapped]
		[AllowedExtensions(new string[] { ".fbx", ".obj", ".stl" },
			ErrorMessage = "The Model field must be an FBX, OBJ, or STL model.")]
		[MaxFileSize(50 * 1024 * 1024, ErrorMessage = "The Model field must be no larger than 50 MB.")]
		[Display(Name = "Model")]
		public IFormFile Model { get; set; }

		public string ModelPath { get; set; }
	}

	public class UniqueNameAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var db = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));
			var entity = db.ThreeDModels.SingleOrDefault(e => e.Name == (string)value);

			if (entity != null)
			{
				return new ValidationResult(ErrorMessage);
			}

			return ValidationResult.Success;
		}
	}

	public class AllowedExtensionsAttribute : ValidationAttribute
	{
		private readonly string[] _extensions;

		public AllowedExtensionsAttribute(string[] extensions)
		{
			_extensions = extensions;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value is IFormFile file)
			{
				var extension = Path.GetExtension(file.FileName).ToLower();

				if (!_extensions.Contains(extension))
				{
					return new ValidationResult(ErrorMessage);
				}
			}

			return ValidationResult.Success;
		}
	}

	public class MaxFileSizeAttribute : ValidationAttribute
	{
		private readonly int _maxFileSize;

		public MaxFileSizeAttribute(int maxFileSize)
		{
			_maxFileSize = maxFileSize;
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value is IFormFile file)
			{
				if (file.Length > _maxFileSize)
				{
					return new ValidationResult(ErrorMessage);
				}
			}

			return ValidationResult.Success;
		}
	}
}