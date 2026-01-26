using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.DAL.Repo.Implementation;

public class CollegeRepository : GenericRepository<College>, ICollegeRepository
{
    public CollegeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<College>> GetByUniversityIdAsync(int universityId)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && c.UniversityId == universityId)
            .Include(c => c.Departments.Where(d => !d.IsDeleted))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<College?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && c.Id == id)
            .Include(c => c.University)
            .Include(c => c.Departments.Where(d => !d.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<int, int>> GetCountsByUniversityIdsAsync(List<int> universityIds)
    {
        if (!universityIds.Any()) return new Dictionary<int, int>();
        
        return await _dbSet
            .Where(c => !c.IsDeleted && universityIds.Contains(c.UniversityId))
            .GroupBy(c => c.UniversityId)
            .Select(g => new { UniversityId = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.UniversityId, x => x.Count);
    }
}
