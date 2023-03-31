using Microsoft.AspNetCore.Http;

namespace DAL;

public class CreateThreeDModelDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public IFormFile Photo { get; set; }
	public IFormFile Model { get; set; }
}
