using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;
using TansiqyV1.DAL.Helpers;
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

    public async Task<IEnumerable<College>> SearchByNameIntelligentAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<College>();

        var normalizedSearchTerm = ArabicTextNormalizer.Normalize(searchTerm);
        var likePatterns = ArabicTextNormalizer.GenerateSearchVariations(searchTerm)
            .Select(v => "%" + v + "%")
            .ToList();

        return await _dbSet
            .Where(c => !c.IsDeleted && (
                likePatterns.Any(pattern => EF.Functions.Like(c.NormalizedNameAr, pattern)) ||
                likePatterns.Any(pattern => EF.Functions.Like(c.NameAr, pattern)) ||
                (c.NameEn != null && EF.Functions.Like(c.NameEn, "%" + searchTerm + "%"))
            ))
            .Include(c => c.University)
            .Include(c => c.Departments.Where(d => !d.IsDeleted))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<College>> SearchIntelligentWithFiltersAsync(
        string? searchTerm,
        UniversityType? type,
        Governorate? governorate,
        StudyType? studyType,
        decimal? minFees,
        decimal? maxFees,
        decimal? minCoordination,
        decimal? maxCoordination,
        string? collegeName = null)
    {
        // Start with basic query - only include university for joins
        var query = _dbSet
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .AsQueryable();

        // Apply numeric filters first (fastest)
        if (minFees.HasValue)
            query = query.Where(c => c.Fees.HasValue && c.Fees >= minFees.Value);

        if (maxFees.HasValue)
            query = query.Where(c => c.Fees.HasValue && c.Fees <= maxFees.Value);

        if (minCoordination.HasValue)
            query = query.Where(c => c.LastYearCoordination.HasValue && c.LastYearCoordination >= minCoordination.Value);

        if (maxCoordination.HasValue)
            query = query.Where(c => c.LastYearCoordination.HasValue && c.LastYearCoordination <= maxCoordination.Value);

        // Apply text filters
        if (!string.IsNullOrWhiteSpace(collegeName))
        {
            var collegePatterns = ArabicTextNormalizer.GenerateSearchVariations(collegeName);
            // Use single OR condition instead of Any with multiple patterns
            query = query.Where(c => 
                EF.Functions.Like(c.NormalizedNameAr, $"%{ArabicTextNormalizer.Normalize(collegeName)}%") ||
                EF.Functions.Like(c.NameAr, $"%{collegeName}%") ||
                (c.NameEn != null && EF.Functions.Like(c.NameEn, $"%{collegeName}%"))
            );
        }

        // Apply university filters - join with universities
        if (type.HasValue || governorate.HasValue || !string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Include(c => c.University);

            if (type.HasValue)
                query = query.Where(c => c.University.Type == type.Value);

            if (governorate.HasValue)
                query = query.Where(c => c.University.Governorate == governorate.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedTerm = ArabicTextNormalizer.Normalize(searchTerm);
                query = query.Where(c => 
                    EF.Functions.Like(c.University.NormalizedNameAr, $"%{normalizedTerm}%") ||
                    EF.Functions.Like(c.University.NameAr, $"%{searchTerm}%") ||
                    (c.University.NameEn != null && EF.Functions.Like(c.University.NameEn, $"%{searchTerm}%"))
                );
            }
        }

        // Study type filter - only if needed
        if (studyType.HasValue)
        {
            query = query.Where(c => c.Departments.Any(d => d.StudyType == studyType.Value && !d.IsDeleted));
        }

        // Execute query with split query optimization
        var colleges = await query
            .Include(c => c.University)
            .AsSplitQuery()
            .ToListAsync();

        // Load departments separately only for filtered results
        if (colleges.Any())
        {
            var collegeIds = colleges.Select(c => c.Id).ToList();
            var departments = await _context.Set<Department>()
                .Where(d => collegeIds.Contains(d.CollegeId) && !d.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            // Attach departments to colleges
            var departmentsByCollege = departments.GroupBy(d => d.CollegeId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var college in colleges)
            {
                if (departmentsByCollege.TryGetValue(college.Id, out var collegeDepts))
                {
                    college.Departments = collegeDepts;
                }
            }
        }

        return colleges;
    }
}
