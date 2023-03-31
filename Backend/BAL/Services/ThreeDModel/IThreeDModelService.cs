using DAL;

namespace BAL;

public interface IThreeDModelService
{
	Task<IEnumerable<ThreeDModelDto>> GetAllAsync();
	Task<ThreeDModelDto> GetByIdAsync(int id);
	Task<IEnumerable<ThreeDModel>> SearchModels(string searchTerm);
	Task<int> CreateAsync(CreateThreeDModelDto dto);
	Task<bool> UpdateAsync(int id,CreateThreeDModelDto dto);
	Task<bool> DeleteAsync(int id);
}