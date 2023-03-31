using BAL;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend
{
	[ApiController]
	[Route("api/[controller]")]
	public class ThreeDModelsController : ControllerBase
	{
		private readonly IThreeDModelService _threeDModelService;

		public ThreeDModelsController(IThreeDModelService threeDModelService)
		{
			_threeDModelService = threeDModelService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ThreeDModelDto>>> GetAllAsync()
		{
			var models = await _threeDModelService.GetAllAsync();
			return Ok(models);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetByIdAsync(int id)
		{
			var modelDto = await _threeDModelService.GetByIdAsync(id);
			if (modelDto == null)
				return NotFound();

			return Ok(modelDto);
		}

		[HttpGet("search/{searchTerm}")]
		public async Task<ActionResult<IEnumerable<ThreeDModel>>> SearchModels(string searchTerm)
		{
			var models = await _threeDModelService.SearchModels(searchTerm);
			if (models == null)
			{
				return NotFound(new { message = "Model Not found" });
			}

			return models.ToList();
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public async Task<ActionResult<int>> CreateAsync([FromForm] CreateThreeDModelDto dto)
		{
			if (dto == null)
				return BadRequest(new { message = "Model Created Successfully" });

			if (dto.Photo == null || dto.Model == null)
				return BadRequest(new { message = "Please upload both a photo and a file." });

			await _threeDModelService.CreateAsync(dto);
			return Ok(new { message = "Model Created Successfully" });
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateModel(int id, [FromForm] CreateThreeDModelDto dto)
		{
			var result = await _threeDModelService.UpdateAsync(id, dto);

			if (!result)
				return NotFound(new { message = "Model with this id not found" });

			return Ok(new { message = "Model Updated Successfully" });
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAsync(int id)
		{
			var result = await _threeDModelService.DeleteAsync(id);
			if (!result)
				return NotFound();

			return Ok(new { message = "Model Deleted Successfully" });
		}
	}
}