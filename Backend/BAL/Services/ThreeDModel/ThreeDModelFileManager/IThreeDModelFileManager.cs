using Microsoft.AspNetCore.Http;

namespace BAL;

public interface IThreeDModelFileManager
{
	void DeleteFile(string filePath);
	string CreateDirectory();
	Task<string> SaveFile(IFormFile file, string directoryPath);
	string GetDirectoryPath(string filePath);
	string GetFilePath(string filePath);
	void DeleteDirectoryIfEmpty(string directoryPath);
}