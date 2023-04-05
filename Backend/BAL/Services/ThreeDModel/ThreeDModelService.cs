using System.Reflection;
using AutoMapper;
using DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BAL
{
	public class ThreeDModelService : IThreeDModelService
	{
		private readonly IMapper _mapper;
		private readonly ApplicationDbContext _dbContext;
		private readonly IThreeDModelFileManager _fileManager;

		public ThreeDModelService(ApplicationDbContext dbContext, IMapper mapper, IWebHostEnvironment env,
			IThreeDModelFileManager fileManager)
		{
			_dbContext = dbContext;
			_mapper = mapper;
			_fileManager = fileManager;
		}

		public async Task<IEnumerable<ThreeDModelDto>> GetAllAsync()
		{
			var models = await _dbContext.ThreeDModels.ToListAsync();
			return _mapper.Map<IEnumerable<ThreeDModelDto>>(models);
		}

		public async Task<ThreeDModelDto> GetByIdAsync(int id)
		{
			var model = await _dbContext.ThreeDModels.FindAsync(id);
			if (model == null)
			{
				return null;
			}

			var dto = _mapper.Map<ThreeDModelDto>(model);
			return dto;
		}

		public async Task<IEnumerable<ThreeDModel>> SearchModels(string searchTerm)
		{
			return await _dbContext.ThreeDModels
				.Where(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm))
				.ToListAsync();
		}

		public async Task<int> CreateAsync(CreateThreeDModelDto dto)
		{
			var model = _mapper.Map<ThreeDModel>(dto);

			var directoryPath = _fileManager.CreateDirectory();

			// var photoFilePath = await _fileManager.SaveFile(dto.Photo, directoryPath);
			// model.PhotoPath = Path.Combine("/Uploads", Path.GetFileName(directoryPath), Path.GetFileName(photoFilePath))
			// 	.Replace("\\", "/");
			//
			// var modelFilePath = await _fileManager.SaveFile(dto.Model, directoryPath);
			// model.ModelPath = Path.Combine("/Uploads", Path.GetFileName(directoryPath), Path.GetFileName(modelFilePath))
			// 	.Replace("\\", "/");
			var properties = typeof(CreateThreeDModelDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				var value = property.GetValue(dto);

				if (value != null)
				{
					if (value is IFormFile file)
					{
						var filePath = await _fileManager.SaveFile(file, directoryPath);
						var propertyName = property.Name;
						var propertyToUpdate = typeof(ThreeDModel).GetProperty(propertyName);
						var updatedFilePath =
							Path.Combine("/Uploads", Path.GetFileName(directoryPath), Path.GetFileName(filePath))
								.Replace("\\", "/");
						propertyToUpdate.SetValue(model, updatedFilePath);
					}
					else
					{
						var stringValue = value?.ToString();
						var propertyName = property.Name;
						var propertyToUpdate = typeof(ThreeDModel).GetProperty(propertyName);
						propertyToUpdate.SetValue(model, stringValue);
					}
				}
			}

			_dbContext.ThreeDModels.Add(model);
			return await _dbContext.SaveChangesAsync();
		}

		public async Task<bool> UpdateAsync(int id, CreateThreeDModelDto dto)
		{
			var model = await _dbContext.ThreeDModels.FindAsync(id);

			if (model == null)
			{
				return false;
			}

			// var ThreeModel = _mapper.Map<ThreeDModel>(dto);

			var directoryPath = _fileManager.GetDirectoryPath(model.PhotoPath);
			var properties = typeof(CreateThreeDModelDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				var value = property.GetValue(model);

				if (value != null)
				{
					if (value is IFormFile file)
					{
						var filePath = await _fileManager.SaveFile(file, directoryPath);
						var propertyName = property.Name;
						var propertyToUpdate = typeof(ThreeDModel).GetProperty(propertyName);
						var oldFilePath = (string)propertyToUpdate.GetValue(model);

						if (!string.IsNullOrEmpty(oldFilePath))
						{
							_fileManager.DeleteFile(oldFilePath);
						}

						var updatedFilePath =
							Path.Combine("/Papers", Path.GetFileName(directoryPath), Path.GetFileName(filePath))
								.Replace("\\", "/");
						propertyToUpdate.SetValue(model, updatedFilePath);
					}
					else if (value is DateTime date)
					{
						var propertyName = property.Name;
						var propertyToUpdate = typeof(ThreeDModel).GetProperty(propertyName);
						propertyToUpdate.SetValue(model, date);
					}
				}
			}

			// if (!Directory.Exists(directoryPath))
			// {
			// 	Directory.CreateDirectory(directoryPath);
			// }
			//
			// if (dto.Photo != null)
			// {
			// 	var oldPhotoFilePath = _fileManager.GetFilePath(model.PhotoPath);
			// 	if (File.Exists(oldPhotoFilePath))
			// 	{
			// 		File.Delete(oldPhotoFilePath);
			// 	}
			//
			// 	model.PhotoPath = await _fileManager.SaveFile(dto.Photo, directoryPath);
			// }
			//
			// if (dto.Model != null)
			// {
			// 	var oldModelFilePath = _fileManager.GetFilePath(model.ModelPath);
			// 	if (File.Exists(oldModelFilePath))
			// 	{
			// 		File.Delete(oldModelFilePath);
			// 	}
			//
			// 	model.ModelPath = await _fileManager.SaveFile(dto.Model, directoryPath);
			// }

			_dbContext.Entry(model).State = EntityState.Modified;
			await _dbContext.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var model = await _dbContext.ThreeDModels.FindAsync(id);
			if (model == null)
			{
				return false;
			}

			_fileManager.DeleteFile(model.PhotoPath);
			_fileManager.DeleteFile(model.ModelPath);

			var directoryPath = _fileManager.GetDirectoryPath(model.PhotoPath);
			_fileManager.DeleteDirectoryIfEmpty(directoryPath);

			_dbContext.ThreeDModels.Remove(model);
			await _dbContext.SaveChangesAsync();
			return true;
		}
	}
}