using TansiqyV1.BLL.ModelVM;
using TansiqyV1.DAL.Enums;

namespace TansiqyV1.BLL.Services.Abstraction;

public interface IUniversityService
{
    // Get Methods
    Task<IEnumerable<UniversityTypeViewModel>> GetUniversityTypesAsync();
    Task<IEnumerable<UniversityViewModel>> GetUniversitiesByTypeAsync(UniversityType type);
    Task<UniversityViewModel?> GetUniversityByIdAsync(int id);
    Task<IEnumerable<UniversityViewModel>> SearchUniversitiesAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null
    );
    Task<IEnumerable<UniversityViewModel>> SearchUniversitiesByNameAsync(string searchTerm);
    
    // Intelligent Arabic Search Methods (new)
    Task<IEnumerable<UniversityViewModel>> SearchUniversitiesIntelligentAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null
    );

    // Combined search by name only - returns both universities and colleges
    Task<SearchResultViewModel> SearchByNameIntelligentAsync(string searchTerm);

    // Search colleges with filters - returns colleges directly
    Task<IEnumerable<CollegeViewModel>> SearchCollegesIntelligentAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null
    );
    
    Task<IEnumerable<CollegeViewModel>> GetCollegesByUniversityIdAsync(int universityId);
    Task<CollegeViewModel?> GetCollegeByIdAsync(int collegeId);
    Task<SimpleDepartmentViewModel?> GetDepartmentByIdAsync(int id);
    Task<BranchViewModel?> GetBranchByIdAsync(int id);

    // Create Methods
    Task<UniversityViewModel> CreateUniversityAsync(CreateUniversityDto dto);
    Task<CollegeViewModel> CreateCollegeAsync(CreateCollegeDto dto);
    Task<DepartmentViewModel> CreateDepartmentAsync(CreateDepartmentDto dto);
    Task<BranchViewModel> CreateBranchAsync(int universityId, CreateBranchDto dto);

    // Update Methods
    Task<UniversityViewModel> UpdateUniversityAsync(UpdateUniversityDto dto);
    Task<CollegeViewModel> UpdateCollegeAsync(UpdateCollegeDto dto);
    Task<DepartmentViewModel> UpdateDepartmentAsync(UpdateDepartmentDto dto);
    Task<BranchViewModel> UpdateBranchAsync(int universityId, UpdateBranchDto dto);

    // Delete Methods
    Task<bool> DeleteUniversityAsync(int id);
    Task<bool> DeleteCollegeAsync(int id);
    Task<bool> DeleteDepartmentAsync(int id);
    Task<bool> DeleteBranchAsync(int id);
}
