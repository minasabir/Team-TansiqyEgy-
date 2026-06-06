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
        var query = _dbSet
            .Where(c => !c.IsDeleted && !c.University.IsDeleted)
            .AsNoTracking()
            .AsQueryable();

        if (minFees.HasValue)
            query = query.Where(c => c.Fees.HasValue && c.Fees >= minFees.Value);

        if (maxFees.HasValue)
            query = query.Where(c => c.Fees.HasValue && c.Fees <= maxFees.Value);

        if (minCoordination.HasValue)
            query = query.Where(c => c.LastYearCoordination.HasValue && c.LastYearCoordination >= minCoordination.Value);

        if (maxCoordination.HasValue)
            query = query.Where(c => c.LastYearCoordination.HasValue && c.LastYearCoordination <= maxCoordination.Value);

        if (type.HasValue)
            query = query.Where(c => c.University.Type == type.Value);

        if (governorate.HasValue)
            query = query.Where(c => c.University.Governorate == governorate.Value);

        if (!string.IsNullOrWhiteSpace(collegeName))
            query = ApplyCollegeNameFilter(query, collegeName);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = ApplyCollegeOrUniversityNameFilter(query, searchTerm);

        if (studyType.HasValue)
            query = query.Where(c => c.Departments.Any(d => d.StudyType == studyType.Value && !d.IsDeleted));

        var colleges = await query
            .Include(c => c.University)
            .AsSplitQuery()
            .ToListAsync();

        if (colleges.Count == 0)
            return colleges;

        var collegeIds = colleges.Select(c => c.Id).ToList();
        var departmentQuery = _context.Set<Department>()
            .Where(d => collegeIds.Contains(d.CollegeId) && !d.IsDeleted);

        if (studyType.HasValue)
            departmentQuery = departmentQuery.Where(d => d.StudyType == studyType.Value);

        var departments = await departmentQuery.AsNoTracking().ToListAsync();
        var departmentsByCollege = departments.GroupBy(d => d.CollegeId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var college in colleges)
        {
            college.Departments = departmentsByCollege.GetValueOrDefault(college.Id) ?? new List<Department>();
        }

        return colleges;
    }

    private static IQueryable<College> ApplyCollegeNameFilter(IQueryable<College> query, string collegeName)
    {
        var likePatterns = ArabicTextNormalizer.GenerateSearchVariations(collegeName)
            .Select(v => "%" + v + "%")
            .ToList();

        return query.Where(c =>
            !c.IsDeleted &&
            (likePatterns.Any(pattern => EF.Functions.Like(c.NormalizedNameAr, pattern)) ||
             likePatterns.Any(pattern => EF.Functions.Like(c.NameAr, pattern)) ||
             (c.NameEn != null && EF.Functions.Like(c.NameEn, "%" + collegeName + "%"))));
    }

    private static IQueryable<College> ApplyCollegeOrUniversityNameFilter(IQueryable<College> query, string searchTerm)
    {
        var likePatterns = ArabicTextNormalizer.GenerateSearchVariations(searchTerm)
            .Select(v => "%" + v + "%")
            .ToList();

        return query.Where(c =>
            likePatterns.Any(pattern => EF.Functions.Like(c.NormalizedNameAr, pattern)) ||
            likePatterns.Any(pattern => EF.Functions.Like(c.NameAr, pattern)) ||
            (c.NameEn != null && EF.Functions.Like(c.NameEn, "%" + searchTerm + "%")) ||
            likePatterns.Any(pattern => EF.Functions.Like(c.University.NormalizedNameAr, pattern)) ||
            likePatterns.Any(pattern => EF.Functions.Like(c.University.NameAr, pattern)) ||
            (c.University.NameEn != null && EF.Functions.Like(c.University.NameEn, "%" + searchTerm + "%")));
    }
}
