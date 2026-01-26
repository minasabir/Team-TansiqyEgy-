using TansiqyV1.DAL.Entities;

namespace TansiqyV1.DAL.Repo.Abstraction;

public interface ICollegeRepository : IGenericRepository<College>
{
    Task<IEnumerable<College>> GetByUniversityIdAsync(int universityId);
    Task<College?> GetByIdWithDetailsAsync(int id);
    Task<Dictionary<int, int>> GetCountsByUniversityIdsAsync(List<int> universityIds);
}





