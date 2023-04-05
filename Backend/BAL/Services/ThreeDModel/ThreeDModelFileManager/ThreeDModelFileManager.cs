using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BAL;

public class ThreeDModelFileManager : IThreeDModelFileManager
{
	private readonly IWebHostEnvironment _env;

	public ThreeDModelFileManager(IWebHostEnvironment env)
	{
		_env = env;
	}

	public void DeleteFile(string filePath)
	{
		if (string.IsNullOrEmpty(filePath))
		{
			return;
		}

		var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
		if (File.Exists(fullPath))
		{
			File.Delete(fullPath);
		}
	}

	public string CreateDirectory()
	{
		var directoryPath = Path.Combine(_env.ContentRootPath ?? string.Empty, "wwwroot", "Uploads",
			Guid.NewGuid().ToString());

		if (!Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}

		return directoryPath;
	}

	public async Task<string> SaveFile(IFormFile file, string directoryPath)
	{
		var fileName = Path.GetFileNameWithoutExtension(file.FileName);
		var extension = Path.GetExtension(file.FileName);
		var newFileName =
			$"{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff")}_{Guid.NewGuid().ToString("N")
				.Substring(0, 6)}_{fileName.Replace("-", "_")}{extension}";
		var filePath = Path.Combine(directoryPath, newFileName);

		using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}

		return filePath;
	}
	// public async Task<string> SaveFile(IFormFile file, string directoryPath)
	// {
	// 	var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
	// 	var filePath = Path.Combine(directoryPath, fileName);
	// 	using (var stream = new FileStream(filePath, FileMode.Create))
	// 	{
	// 		await file.CopyToAsync(stream);
	// 	}
	//
	// 	return Path.Combine("/Uploads", Path.GetFileName(directoryPath), fileName).Replace("\\", "/");
	// }

	public string GetDirectoryPath(string filePath)
	{
		return Path.Combine(_env.ContentRootPath ?? string.Empty, "wwwroot", "Uploads",
			Path.GetFileName(Path.GetDirectoryName(filePath)));
	}

	public string GetFilePath(string filePath)
	{
		return Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
	}

	public void DeleteDirectoryIfEmpty(string directoryPath)
	{
		if (Directory.Exists(directoryPath) && !Directory.EnumerateFileSystemEntries(directoryPath).Any())
		{
			Directory.Delete(directoryPath);
		}
	}
}