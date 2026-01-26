using Microsoft.EntityFrameworkCore;
using TansiqyV1.BLL.Helpers;
using TansiqyV1.BLL.ModelVM;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.DAL.Entities;
using TansiqyV1.DAL.Enums;
using TansiqyV1.DAL.Helpers;
using TansiqyV1.DAL.Repo.Abstraction;

namespace TansiqyV1.BLL.Services.Implementation;

public class UniversityService : IUniversityService
{
    private readonly IUniversityRepository _universityRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly IGenericRepository<Department> _departmentRepository;
    private readonly IGenericRepository<UniversityBranch> _branchRepository;

    public UniversityService(
        IUniversityRepository universityRepository,
        ICollegeRepository collegeRepository,
        IGenericRepository<Department> departmentRepository,
        IGenericRepository<UniversityBranch> branchRepository)
    {
        _universityRepository = universityRepository;
        _collegeRepository = collegeRepository;
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
    }

    public async Task<IEnumerable<UniversityTypeViewModel>> GetUniversityTypesAsync()
    {
        var counts = await _universityRepository.GetUniversityCountsByTypeAsync();
        
        return Enum.GetValues<UniversityType>()
            .OrderBy(type => (int)type) // ترتيب حسب رقم الـ enum من 1 إلى 6
            .Select(type => new UniversityTypeViewModel
            {
                TypeNameAr = type.GetDescription(),
                TotalUniversities = counts.GetValueOrDefault(type, 0)
            });
    }

    public async Task<IEnumerable<UniversityViewModel>> GetUniversitiesByTypeAsync(UniversityType type)
    {
        var universities = await _universityRepository.GetByTypeAsync(type);
        
        // Get counts in batch to avoid N+1 queries
        var universityIds = universities.Select(u => u.Id).ToList();
        var collegeCounts = await _collegeRepository.GetCountsByUniversityIdsAsync(universityIds);
        var branchCounts = await _universityRepository.GetBranchCountsByUniversityIdsAsync(universityIds);
        
        return universities.Select(u => new UniversityViewModel
        {
            Id = u.Id,
            NameAr = u.NameAr,
            NameEn = u.NameEn,
            Type = (int)u.Type,
            TypeAr = u.Type.GetDescription(),
            OfficialWebsite = u.OfficialWebsite,
            Location = u.Location,
            Governorate = (int)u.Governorate,
            GovernorateAr = u.Governorate.GetDescription(),
            LastYearCoordination = u.LastYearCoordination,
            Fees = u.Fees,
            InformationSources = u.InformationSources,
            Description = u.Description,
            CollegesCount = collegeCounts.GetValueOrDefault(u.Id, 0),
            BranchesCount = branchCounts.GetValueOrDefault(u.Id, 0),
            Colleges = new List<CollegeViewModel>(),
            Branches = new List<BranchViewModel>()
        });
    }

    public async Task<UniversityViewModel?> GetUniversityByIdAsync(int id)
    {
        var university = await _universityRepository.GetByIdWithDetailsAsync(id);
        if (university == null) return null;

        return new UniversityViewModel
        {
            Id = university.Id,
            NameAr = university.NameAr,
            NameEn = university.NameEn,
            Type = (int)university.Type,
            TypeAr = university.Type.GetDescription(),
            OfficialWebsite = university.OfficialWebsite,
            Location = university.Location,
            Governorate = (int)university.Governorate,
            GovernorateAr = university.Governorate.GetDescription(),
            LastYearCoordination = university.LastYearCoordination,
            Fees = university.Fees,
            InformationSources = university.InformationSources,
            Description = university.Description,
            CollegesCount = university.Colleges.Count,
            BranchesCount = university.Branches.Count,
            Colleges = university.Colleges.Select(c => new CollegeViewModel
            {
                Id = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                UniversityId = c.UniversityId,
                OfficialWebsite = c.OfficialWebsite,
                Location = c.Location,
                Fees = c.Fees,
                LastYearCoordination = c.LastYearCoordination,
                FeesCategoryA = c.FeesCategoryA,
                FeesCategoryB = c.FeesCategoryB,
                FeesCategoryC = c.FeesCategoryC,
                FeesPerHour = c.FeesPerHour,
                MinimumHoursPerSemester = c.MinimumHoursPerSemester,
                AdditionalFees = c.AdditionalFees,
                DepartmentsCount = c.Departments.Count
            }).ToList(),
            Branches = university.Branches.Select(b => new BranchViewModel
            {
                Id = b.Id,
                NameAr = b.NameAr,
                NameEn = b.NameEn,
                Location = b.Location,
                Governorate = (int)b.Governorate,
                GovernorateAr = b.Governorate.GetDescription()
            }).ToList()
        };
    }

    public async Task<IEnumerable<UniversityViewModel>> SearchUniversitiesAsync(
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
        var universities = await _universityRepository.SearchAsync(
            searchTerm, 
            type, 
            governorate, 
            minFees, 
            maxFees, 
            studyType, 
            minCoordination, 
            maxCoordination, 
            collegeName);

        return universities.Select(u => new UniversityViewModel
        {
            Id = u.Id,
            NameAr = u.NameAr,
            NameEn = u.NameEn,
            Type = (int)u.Type,
            TypeAr = u.Type.GetDescription(),
            OfficialWebsite = u.OfficialWebsite,
            Location = u.Location,
            Governorate = (int)u.Governorate,
            GovernorateAr = u.Governorate.GetDescription(),
            LastYearCoordination = u.LastYearCoordination,
            Fees = u.Fees,
            InformationSources = u.InformationSources,
            Description = u.Description,
            CollegesCount = !string.IsNullOrWhiteSpace(collegeName) ? 
                u.Colleges.Count(c => c.NameAr.Contains(collegeName) || (c.NameEn != null && c.NameEn.Contains(collegeName))) : 
                u.Colleges.Count,
            BranchesCount = u.Branches.Count,
            Colleges = u.Colleges.Where(c => !string.IsNullOrWhiteSpace(collegeName) ? 
                (c.NameAr.Contains(collegeName) ||
                (c.NameEn != null && c.NameEn.Contains(collegeName))) : true)
                .Select(c => new CollegeViewModel
                {
                    Id = c.Id,
                    NameAr = c.NameAr,
                    UniversityId = c.UniversityId,
                    Departments = c.Departments.Select(d => new DepartmentViewModel 
                    { 
                        Id = d.Id, 
                        NameAr = d.NameAr, 
                        StudyType = d.StudyType.HasValue ? (int?)d.StudyType.Value : null,
                        StudyTypeAr = d.StudyType.HasValue ? d.StudyType.Value.GetDescription() : null
                    })
                    .ToList()
                })
                .ToList()
        });
    }

    public async Task<IEnumerable<UniversityViewModel>> SearchUniversitiesByNameAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<UniversityViewModel>();

        var universities = await _universityRepository.SearchByNameAsync(searchTerm);
        
        var universityIds = universities.Select(u => u.Id).ToList();
        var collegeCounts = await _collegeRepository.GetCountsByUniversityIdsAsync(universityIds);
        var branchCounts = await _universityRepository.GetBranchCountsByUniversityIdsAsync(universityIds);

        return universities.Select(u => new UniversityViewModel
        {
            Id = u.Id,
            NameAr = u.NameAr,
            NameEn = u.NameEn,
            Type = (int)u.Type,
            TypeAr = u.Type.GetDescription(),
            OfficialWebsite = u.OfficialWebsite,
            Location = u.Location,
            Governorate = (int)u.Governorate,
            GovernorateAr = u.Governorate.GetDescription(),
            LastYearCoordination = u.LastYearCoordination,
            Fees = u.Fees,
            InformationSources = u.InformationSources,
            Description = u.Description,
            CollegesCount = collegeCounts.GetValueOrDefault(u.Id, 0),
            BranchesCount = branchCounts.GetValueOrDefault(u.Id, 0),
            Colleges = new List<CollegeViewModel>(),
            Branches = new List<BranchViewModel>()
        });
    }

    public async Task<IEnumerable<CollegeViewModel>> GetCollegesByUniversityIdAsync(int universityId)
    {
        var colleges = await _collegeRepository.GetByUniversityIdAsync(universityId);
        
        // Get university info once
        var university = await _universityRepository.GetByIdAsync(universityId);
        
        return colleges.Select(c => new CollegeViewModel
        {
            Id = c.Id,
            NameAr = c.NameAr,
            NameEn = c.NameEn,
            UniversityId = c.UniversityId,
            OfficialWebsite = c.OfficialWebsite,
            Location = c.Location,
            Fees = c.Fees,
            LastYearCoordination = c.LastYearCoordination,
            Description = c.Description,
            FeesCategoryA = c.FeesCategoryA,
            FeesCategoryB = c.FeesCategoryB,
            FeesCategoryC = c.FeesCategoryC,
            FeesPerHour = c.FeesPerHour,
            MinimumHoursPerSemester = c.MinimumHoursPerSemester,
            AdditionalFees = c.AdditionalFees,
            DepartmentsCount = c.Departments.Count,
            University = university != null ? new UniversityBasicViewModel
            {
                Id = university.Id,
                NameAr = university.NameAr,
                Type = (int)university.Type,
                TypeAr = university.Type.GetDescription()
            } : null,
            Departments = c.Departments.Select(d => new DepartmentViewModel
            {
                Id = d.Id,
                NameAr = d.NameAr,
                NameEn = d.NameEn,
                StudyType = d.StudyType.HasValue ? (int?)d.StudyType.Value : null,
                StudyTypeAr = d.StudyType.HasValue ? d.StudyType.Value.GetDescription() : null,
                Description = d.Description
            }).ToList()
        });
    }

    public async Task<CollegeViewModel?> GetCollegeByIdAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdWithDetailsAsync(collegeId);
        if (college == null) return null;

        return new CollegeViewModel
        {
            Id = college.Id,
            NameAr = college.NameAr,
            NameEn = college.NameEn,
            UniversityId = college.UniversityId,
            OfficialWebsite = college.OfficialWebsite,
            Location = college.Location,
            Fees = college.Fees,
            LastYearCoordination = college.LastYearCoordination,
            Description = college.Description,
            FeesCategoryA = college.FeesCategoryA,
            FeesCategoryB = college.FeesCategoryB,
            FeesCategoryC = college.FeesCategoryC,
            FeesPerHour = college.FeesPerHour,
            MinimumHoursPerSemester = college.MinimumHoursPerSemester,
            AdditionalFees = college.AdditionalFees,
            DepartmentsCount = college.Departments.Count,
            University = new UniversityBasicViewModel
            {
                Id = college.University.Id,
                NameAr = college.University.NameAr,
                Type = (int)college.University.Type,
                TypeAr = college.University.Type.GetDescription()
            },
            Departments = college.Departments.Select(d => new DepartmentViewModel
            {
                Id = d.Id,
                NameAr = d.NameAr,
                NameEn = d.NameEn,
                StudyType = d.StudyType.HasValue ? (int?)d.StudyType.Value : null,
                StudyTypeAr = d.StudyType.HasValue ? d.StudyType.Value.GetDescription() : null,
                Description = d.Description
            }).ToList()
        };
    }

    // Create Methods
    public async Task<UniversityViewModel> CreateUniversityAsync(CreateUniversityDto dto)
    {
        var university = new University
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Type = dto.Type,
            OfficialWebsite = dto.OfficialWebsite,
            Location = dto.Location,
            Governorate = dto.Governorate,
            LastYearCoordination = dto.LastYearCoordination,
            Fees = dto.Fees,
            InformationSources = dto.InformationSources,
            Description = dto.Description,
            CreatedAt = DateTime.Now
        };

        var createdUniversity = await _universityRepository.AddAsync(university);

        // إضافة الفروع إذا كانت موجودة
        if (dto.Branches != null && dto.Branches.Any())
        {
            var branches = dto.Branches.Select(b => new UniversityBranch
            {
                UniversityId = createdUniversity.Id,
                NameAr = b.NameAr,
                NameEn = b.NameEn,
                Location = b.Location,
                Governorate = b.Governorate,
                CreatedAt = DateTime.Now
            }).ToList();

            await _branchRepository.AddRangeAsync(branches);
        }

        return await GetUniversityByIdAsync(createdUniversity.Id) ?? 
               throw new Exception("Failed to retrieve created university");
    }

    public async Task<CollegeViewModel> CreateCollegeAsync(CreateCollegeDto dto)
    {
        // التحقق من وجود الجامعة
        var university = await _universityRepository.GetByIdAsync(dto.UniversityId);
        if (university == null)
            throw new ArgumentException($"University with ID {dto.UniversityId} not found");

        var college = new College
        {
            UniversityId = dto.UniversityId,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            OfficialWebsite = dto.OfficialWebsite,
            Location = dto.Location,
            Description = dto.Description,
            Fees = dto.Fees,
            LastYearCoordination = dto.LastYearCoordination,
            FeesCategoryA = dto.FeesCategoryA,
            FeesCategoryB = dto.FeesCategoryB,
            FeesCategoryC = dto.FeesCategoryC,
            FeesPerHour = dto.FeesPerHour,
            MinimumHoursPerSemester = dto.MinimumHoursPerSemester,
            AdditionalFees = dto.AdditionalFees,
            CreatedAt = DateTime.Now
        };

        var createdCollege = await _collegeRepository.AddAsync(college);

        // إضافة التخصصات إذا كانت موجودة
        if (dto.Departments != null && dto.Departments.Any())
        {
            var departments = dto.Departments.Select(d => new Department
            {
                CollegeId = createdCollege.Id,
                NameAr = d.NameAr,
                NameEn = d.NameEn,
                Description = d.Description,
                StudyType = d.StudyType,
                CreatedAt = DateTime.Now
            }).ToList();

            await _departmentRepository.AddRangeAsync(departments);
        }

        return await GetCollegeByIdAsync(createdCollege.Id) ?? 
               throw new Exception("Failed to retrieve created college");
    }

    public async Task<DepartmentViewModel> CreateDepartmentAsync(CreateDepartmentDto dto)
    {
        // التحقق من وجود الكلية
        var college = await _collegeRepository.GetByIdAsync(dto.CollegeId);
        if (college == null)
            throw new ArgumentException($"College with ID {dto.CollegeId} not found");

        var department = new Department
        {
            CollegeId = dto.CollegeId,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Description = dto.Description,
            StudyType = dto.StudyType,
            CreatedAt = DateTime.Now
        };

        var createdDepartment = await _departmentRepository.AddAsync(department);

        return new DepartmentViewModel
        {
            Id = createdDepartment.Id,
            NameAr = createdDepartment.NameAr,
            NameEn = createdDepartment.NameEn,
            Description = createdDepartment.Description,
            StudyType = createdDepartment.StudyType.HasValue ? (int?)createdDepartment.StudyType.Value : null,
            StudyTypeAr = createdDepartment.StudyType.HasValue ? createdDepartment.StudyType.Value.GetDescription() : null
        };
    }

    public async Task<BranchViewModel> CreateBranchAsync(int universityId, CreateBranchDto dto)
    {
        // التحقق من وجود الجامعة
        var university = await _universityRepository.GetByIdAsync(universityId);
        if (university == null)
            throw new ArgumentException($"University with ID {universityId} not found");

        var branch = new UniversityBranch
        {
            UniversityId = universityId,
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Location = dto.Location,
            Governorate = dto.Governorate,
            CreatedAt = DateTime.Now
        };

        var createdBranch = await _branchRepository.AddAsync(branch);

        return new BranchViewModel
        {
            Id = createdBranch.Id,
            NameAr = createdBranch.NameAr,
            NameEn = createdBranch.NameEn,
            Location = createdBranch.Location,
            Governorate = (int)createdBranch.Governorate,
            GovernorateAr = createdBranch.Governorate.GetDescription()
        };
    }

    // Update Methods
    public async Task<UniversityViewModel> UpdateUniversityAsync(UpdateUniversityDto dto)
    {
        var university = await _universityRepository.GetByIdAsync(dto.Id);
        if (university == null)
            throw new ArgumentException($"University with ID {dto.Id} not found");

        university.NameAr = dto.NameAr;
        university.NameEn = dto.NameEn;
        university.Type = dto.Type;
        university.OfficialWebsite = dto.OfficialWebsite;
        university.Location = dto.Location;
        university.Governorate = dto.Governorate;
        university.LastYearCoordination = dto.LastYearCoordination;
        university.Fees = dto.Fees;
        university.InformationSources = dto.InformationSources;
        university.Description = dto.Description;
        university.UpdatedAt = DateTime.Now;

        await _universityRepository.UpdateAsync(university);

        return await GetUniversityByIdAsync(university.Id) ?? 
               throw new Exception("Failed to retrieve updated university");
    }

    public async Task<CollegeViewModel> UpdateCollegeAsync(UpdateCollegeDto dto)
    {
        // التحقق من وجود الكلية
        var college = await _collegeRepository.GetByIdAsync(dto.Id);
        if (college == null)
            throw new ArgumentException($"College with ID {dto.Id} not found");

        // التحقق من وجود الجامعة
        var university = await _universityRepository.GetByIdAsync(dto.UniversityId);
        if (university == null)
            throw new ArgumentException($"University with ID {dto.UniversityId} not found");

        college.UniversityId = dto.UniversityId;
        college.NameAr = dto.NameAr;
        college.NameEn = dto.NameEn;
        college.OfficialWebsite = dto.OfficialWebsite;
        college.Location = dto.Location;
        college.Description = dto.Description;
        college.Fees = dto.Fees;
        college.LastYearCoordination = dto.LastYearCoordination;
        college.FeesCategoryA = dto.FeesCategoryA;
        college.FeesCategoryB = dto.FeesCategoryB;
        college.FeesCategoryC = dto.FeesCategoryC;
        college.FeesPerHour = dto.FeesPerHour;
        college.MinimumHoursPerSemester = dto.MinimumHoursPerSemester;
        college.AdditionalFees = dto.AdditionalFees;
        college.UpdatedAt = DateTime.Now;

        await _collegeRepository.UpdateAsync(college);

        return await GetCollegeByIdAsync(college.Id) ?? 
               throw new Exception("Failed to retrieve updated college");
    }

    public async Task<DepartmentViewModel> UpdateDepartmentAsync(UpdateDepartmentDto dto)
    {
        // التحقق من وجود التخصص
        var department = await _departmentRepository.GetByIdAsync(dto.Id);
        if (department == null)
            throw new ArgumentException($"Department with ID {dto.Id} not found");

        // التحقق من وجود الكلية
        var college = await _collegeRepository.GetByIdAsync(dto.CollegeId);
        if (college == null)
            throw new ArgumentException($"College with ID {dto.CollegeId} not found");

        department.CollegeId = dto.CollegeId;
        department.NameAr = dto.NameAr;
        department.NameEn = dto.NameEn;
        department.Description = dto.Description;
        department.StudyType = dto.StudyType;
        department.UpdatedAt = DateTime.Now;

        await _departmentRepository.UpdateAsync(department);

        return new DepartmentViewModel
        {
            Id = department.Id,
            NameAr = department.NameAr,
            NameEn = department.NameEn,
            Description = department.Description,
            StudyType = department.StudyType.HasValue ? (int?)department.StudyType.Value : null,
            StudyTypeAr = department.StudyType.HasValue ? department.StudyType.Value.GetDescription() : null
        };
    }

    public async Task<BranchViewModel> UpdateBranchAsync(int universityId, UpdateBranchDto dto)
    {
        // التحقق من وجود الفرع
        var branch = await _branchRepository.GetByIdAsync(dto.Id);
        if (branch == null)
            throw new ArgumentException($"Branch with ID {dto.Id} not found");

        // التحقق من وجود الجامعة
        var university = await _universityRepository.GetByIdAsync(universityId);
        if (university == null)
            throw new ArgumentException($"University with ID {universityId} not found");

        // التحقق من أن الفرع يتبع هذه الجامعة
        if (branch.UniversityId != universityId)
            throw new ArgumentException($"Branch {dto.Id} does not belong to University {universityId}");

        branch.NameAr = dto.NameAr;
        branch.NameEn = dto.NameEn;
        branch.Location = dto.Location;
        branch.Governorate = dto.Governorate;
        branch.UpdatedAt = DateTime.Now;

        await _branchRepository.UpdateAsync(branch);

        return new BranchViewModel
        {
            Id = branch.Id,
            NameAr = branch.NameAr,
            NameEn = branch.NameEn,
            Location = branch.Location,
            Governorate = (int)branch.Governorate,
            GovernorateAr = branch.Governorate.GetDescription()
        };
    }

    // Delete Methods
    public async Task<bool> DeleteUniversityAsync(int id)
    {
        var university = await _universityRepository.GetByIdAsync(id);
        if (university == null)
            return false;

        await _universityRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> DeleteCollegeAsync(int id)
    {
        var college = await _collegeRepository.GetByIdAsync(id);
        if (college == null)
            return false;

        await _collegeRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
            return false;

        await _departmentRepository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> DeleteBranchAsync(int id)
    {
        var branch = await _branchRepository.GetByIdAsync(id);
        if (branch == null)
            return false;

        await _branchRepository.DeleteAsync(id);
        return true;
    }
}
