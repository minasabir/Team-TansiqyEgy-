using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.DAL.Repo.Abstraction;

public interface ICollegeRepository : IGenericRepository<College>
{
    Task<IEnumerable<College>> GetByUniversityIdAsync(int universityId);
    Task<College?> GetByIdWithDetailsAsync(int id);
    Task<Dictionary<int, int>> GetCountsByUniversityIdsAsync(List<int> universityIds);

    // Intelligent Arabic Search Methods
    Task<IEnumerable<College>> SearchByNameIntelligentAsync(string searchTerm);
    
    // Search colleges with all filters
    Task<IEnumerable<College>> SearchIntelligentWithFiltersAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null);
}





