using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Repo.Abstraction;

public interface IUniversityRepository : IGenericRepository<University>
{
    Task<IEnumerable<University>> GetByTypeAsync(UniversityType type);
    Task<IEnumerable<University>> GetByGovernorateAsync(Governorate governorate);
    Task<IEnumerable<University>> SearchAsync(string? searchTerm, UniversityType? type, Governorate? governorate, decimal? minFees, decimal? maxFees, StudyType? studyType = null, decimal? minCoordination = null, decimal? maxCoordination = null, string? collegeName = null);
    Task<IEnumerable<University>> SearchByNameAsync(string searchTerm);
    Task<University?> GetByIdWithDetailsAsync(int id);
    Task<Dictionary<int, int>> GetBranchCountsByUniversityIdsAsync(List<int> universityIds);
    Task<Dictionary<UniversityType, int>> GetUniversityCountsByTypeAsync();
}





