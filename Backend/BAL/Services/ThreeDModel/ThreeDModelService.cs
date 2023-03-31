using AutoMapper;
using DAL;
using Microsoft.AspNetCore.Hosting;
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

			var photoFilePath = await _fileManager.SaveFile(dto.Photo, directoryPath);
			model.PhotoPath = Path.Combine("/Uploads", Path.GetFileName(directoryPath), Path.GetFileName(photoFilePath))
				.Replace("\\", "/");

			var modelFilePath = await _fileManager.SaveFile(dto.Model, directoryPath);
			model.ModelPath = Path.Combine("/Uploads", Path.GetFileName(directoryPath), Path.GetFileName(modelFilePath))
				.Replace("\\", "/");

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

			_mapper.Map(dto, model);

			var directoryPath = _fileManager.GetDirectoryPath(model.PhotoPath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			if (dto.Photo != null)
			{
				var oldPhotoFilePath = _fileManager.GetFilePath(model.PhotoPath);
				if (File.Exists(oldPhotoFilePath))
				{
					File.Delete(oldPhotoFilePath);
				}

				model.PhotoPath = await _fileManager.SaveFile(dto.Photo, directoryPath);
			}

			if (dto.Model != null)
			{
				var oldModelFilePath = _fileManager.GetFilePath(model.ModelPath);
				if (File.Exists(oldModelFilePath))
				{
					File.Delete(oldModelFilePath);
				}

				model.ModelPath = await _fileManager.SaveFile(dto.Model, directoryPath);
			}

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