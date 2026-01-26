using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.DAL.Repo.Implementation;

public class UniversityRepository : GenericRepository<University>, IUniversityRepository
{
    public UniversityRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<University>> GetByTypeAsync(UniversityType type)
    {
        // Optimized: Don't load Colleges/Branches collections, only count them
        return await _dbSet
            .Where(u => !u.IsDeleted && u.Type == type)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<University>> GetByGovernorateAsync(Governorate governorate)
    {
        // Optimized: Don't load collections for list view
        return await _dbSet
            .Where(u => !u.IsDeleted && u.Governorate == governorate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<University>> SearchAsync(string? searchTerm, UniversityType? type, Governorate? governorate, decimal? minFees, decimal? maxFees, StudyType? studyType = null, decimal? minCoordination = null, decimal? maxCoordination = null, string? collegeName = null)
    {
        var query = _dbSet.Where(u => !u.IsDeleted).AsQueryable();

        // University name search (Arabic and English)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => 
                EF.Functions.Like(u.NameAr, $"%{searchTerm}%") ||
                (u.NameEn != null && EF.Functions.Like(u.NameEn, $"%{searchTerm}%"))
            );
        }

        // University type filter
        if (type.HasValue)
            query = query.Where(u => u.Type == type.Value);

        // Governorate filter
        if (governorate.HasValue)
            query = query.Where(u => u.Governorate == governorate.Value);

        // Fees range filter
        if (minFees.HasValue)
            query = query.Where(u => u.Fees.HasValue && u.Fees >= minFees.Value);

        if (maxFees.HasValue)
            query = query.Where(u => u.Fees.HasValue && u.Fees <= maxFees.Value);

        // Coordination range filter
        if (minCoordination.HasValue)
            query = query.Where(u => u.LastYearCoordination.HasValue && u.LastYearCoordination >= minCoordination.Value);

        if (maxCoordination.HasValue)
            query = query.Where(u => u.LastYearCoordination.HasValue && u.LastYearCoordination <= maxCoordination.Value);

        // Study type filter - filter universities that have departments with the specified study type
        if (studyType.HasValue)
        {
            query = query.Where(u => 
                u.Colleges.Any(c => 
                    c.Departments.Any(d => d.StudyType == studyType.Value && !d.IsDeleted) && !c.IsDeleted
                )
            );
        }

        // College name filter - filter universities that have colleges matching the name
        if (!string.IsNullOrWhiteSpace(collegeName))
        {
            query = query.Where(u => 
                u.Colleges.Any(c => 
                    !c.IsDeleted &&
                    (EF.Functions.Like(c.NameAr, $"%{collegeName}%") ||
                    (c.NameEn != null && EF.Functions.Like(c.NameEn, $"%{collegeName}%")))
                )
            );
        }

        // Load all related data for the final result
        return await query
            .Include(u => u.Colleges)
                .ThenInclude(c => c.Departments)
            .Include(u => u.Branches)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<University>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<University>();

        return await _dbSet
            .Where(u => !u.IsDeleted && (
                EF.Functions.Like(u.NameAr, $"%{searchTerm}%") ||
                (u.NameEn != null && EF.Functions.Like(u.NameEn, $"%{searchTerm}%"))
            ))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<UniversityType, int>> GetUniversityCountsByTypeAsync()
    {
        return await _dbSet
            .Where(u => !u.IsDeleted)
            .GroupBy(u => u.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Type, x => x.Count);
    }

    public async Task<University?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Where(u => !u.IsDeleted && u.Id == id)
            .Include(u => u.Colleges.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Departments.Where(d => !d.IsDeleted))
            .Include(u => u.Branches.Where(b => !b.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<int, int>> GetBranchCountsByUniversityIdsAsync(List<int> universityIds)
    {
        if (!universityIds.Any()) return new Dictionary<int, int>();
        
        return await _context.Set<UniversityBranch>()
            .Where(b => !b.IsDeleted && universityIds.Contains(b.UniversityId))
            .GroupBy(b => b.UniversityId)
            .Select(g => new { UniversityId = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToDictionaryAsync(x => x.UniversityId, x => x.Count);
    }
}
