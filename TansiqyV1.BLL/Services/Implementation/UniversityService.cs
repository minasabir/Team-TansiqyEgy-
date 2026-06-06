using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    private readonly string _uploadsDirectory;
    private bool _uploadsDirectoryCreated = false;

    public UniversityService(
        IUniversityRepository universityRepository,
        ICollegeRepository collegeRepository,
        IGenericRepository<Department> departmentRepository,
        IGenericRepository<UniversityBranch> branchRepository,
        string uploadsPath)
    {
        _universityRepository = universityRepository;
        _collegeRepository = collegeRepository;
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _uploadsDirectory = uploadsPath;
    }

    private void EnsureUploadsDirectoryExists()
    {
        if (!_uploadsDirectoryCreated && !Directory.Exists(_uploadsDirectory))
        {
            try
            {
                Directory.CreateDirectory(_uploadsDirectory);
                _uploadsDirectoryCreated = true;
            }
            catch
            {
                // Silently ignore - we'll handle file save errors when they occur
            }
        }
    }

    private async Task<string?> SaveImageAsync(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            throw new ArgumentException("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");

        // Generate unique file name
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(extension))
            extension = ".jpg";

        var fileName = $"{Guid.NewGuid():N}{extension}";

        // Ensure directory exists before saving
        EnsureUploadsDirectoryExists();

        var filePath = Path.Combine(_uploadsDirectory, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        // Return relative path for database storage
        return $"/uploads/universities/{fileName}";
    }

    private void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        try
        {
            var fileName = Path.GetFileName(imagePath);
            var filePath = Path.Combine(_uploadsDirectory, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Silently ignore deletion errors
        }
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
            Image = u.Image,
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
            Image = university.Image,
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
            Image = u.Image,
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
            Image = u.Image,
            CollegesCount = collegeCounts.GetValueOrDefault(u.Id, 0),
            BranchesCount = branchCounts.GetValueOrDefault(u.Id, 0),
            Colleges = new List<CollegeViewModel>(),
            Branches = new List<BranchViewModel>()
        });
    }

    // =========================================================================
    // INTELLIGENT ARABIC SEARCH METHODS
    // =========================================================================

    public async Task<IEnumerable<UniversityViewModel>> SearchUniversitiesIntelligentAsync(
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
        var universities = await _universityRepository.SearchIntelligentAsync(
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
            Image = u.Image,
            CollegesCount = !string.IsNullOrWhiteSpace(collegeName) ?
                u.Colleges.Count(c => ArabicTextNormalizer.Contains(c.NameAr, collegeName)) :
                u.Colleges.Count,
            BranchesCount = u.Branches.Count,
            Colleges = u.Colleges.Where(c => !string.IsNullOrWhiteSpace(collegeName) ?
                ArabicTextNormalizer.Contains(c.NameAr, collegeName) : true)
                .Select(c => new CollegeViewModel
                {
                    Id = c.Id,
                    NameAr = c.NameAr,
                    UniversityId = c.UniversityId,
                    Fees = c.Fees,  // Use College fees, not University
                    LastYearCoordination = c.LastYearCoordination,  // Use College coordination, not University
                    FeesCategoryA = c.FeesCategoryA,
                    FeesCategoryB = c.FeesCategoryB,
                    FeesCategoryC = c.FeesCategoryC,
                    FeesPerHour = c.FeesPerHour,
                    MinimumHoursPerSemester = c.MinimumHoursPerSemester,
                    AdditionalFees = c.AdditionalFees,
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

    // Combined Intelligent Arabic Search - returns both universities and colleges
    public async Task<SearchResultViewModel> SearchByNameIntelligentAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new SearchResultViewModel
            {
                Universities = new List<UniversityViewModel>(),
                Colleges = new List<CollegeViewModel>()
            };

        // Search universities
        var universities = await _universityRepository.SearchByNameIntelligentAsync(searchTerm);
        var universityIds = universities.Select(u => u.Id).ToList();
        var collegeCounts = await _collegeRepository.GetCountsByUniversityIdsAsync(universityIds);
        var branchCounts = await _universityRepository.GetBranchCountsByUniversityIdsAsync(universityIds);

        var universityViewModels = universities.Select(u => new UniversityViewModel
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
            Image = u.Image,
            CollegesCount = collegeCounts.GetValueOrDefault(u.Id, 0),
            BranchesCount = branchCounts.GetValueOrDefault(u.Id, 0),
            Colleges = new List<CollegeViewModel>(),
            Branches = new List<BranchViewModel>()
        }).ToList();

        // Search colleges
        var colleges = await _collegeRepository.SearchByNameIntelligentAsync(searchTerm);
        var collegeViewModels = colleges.Select(MapCollegeToViewModel).ToList();

        return new SearchResultViewModel
        {
            Universities = universityViewModels,
            Colleges = collegeViewModels
        };
    }

    public async Task<IEnumerable<CollegeViewModel>> SearchCollegesIntelligentAsync(
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
        var colleges = await _collegeRepository.SearchIntelligentWithFiltersAsync(
            searchTerm,
            type,
            governorate,
            studyType,
            minFees,
            maxFees,
            minCoordination,
            maxCoordination,
            collegeName);

        return colleges.Select(MapCollegeToViewModel);
    }

    private static CollegeViewModel MapCollegeToViewModel(College c) => new()
    {
        Id = c.Id,
        NameAr = c.NameAr,
        NameEn = c.NameEn,
        UniversityId = c.UniversityId,
        OfficialWebsite = c.OfficialWebsite,
        Location = c.Location,
        Description = c.Description,
        Fees = c.Fees,
        LastYearCoordination = c.LastYearCoordination,
        FeesCategoryA = c.FeesCategoryA,
        FeesCategoryB = c.FeesCategoryB,
        FeesCategoryC = c.FeesCategoryC,
        FeesPerHour = c.FeesPerHour,
        MinimumHoursPerSemester = c.MinimumHoursPerSemester,
        AdditionalFees = c.AdditionalFees,
        DepartmentsCount = c.Departments.Count,
        University = c.University != null ? new UniversityBasicViewModel
        {
            Id = c.University.Id,
            NameAr = c.University.NameAr,
            Type = (int)c.University.Type,
            TypeAr = c.University.Type.GetDescription()
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
    };

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

    public async Task<SimpleDepartmentViewModel?> GetDepartmentByIdAsync(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null) return null;

        return new SimpleDepartmentViewModel
        {
            NameAr = department.NameAr,
            NameEn = department.NameEn,
            Description = department.Description
        };
    }

    public async Task<BranchViewModel?> GetBranchByIdAsync(int id)
    {
        var branch = await _branchRepository.GetByIdAsync(id);
        if (branch == null) return null;

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

    // Create Methods
    public async Task<UniversityViewModel> CreateUniversityAsync(CreateUniversityDto dto)
    {
        // Handle image upload if provided
        string? imagePath = null;
        if (dto.ImageFile != null)
        {
            imagePath = await SaveImageAsync(dto.ImageFile);
        }

        var university = new University
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            NormalizedNameAr = ArabicTextNormalizer.Normalize(dto.NameAr),
            Type = dto.Type,
            OfficialWebsite = dto.OfficialWebsite,
            Location = dto.Location,
            Governorate = dto.Governorate,
            LastYearCoordination = dto.LastYearCoordination,
            Fees = dto.Fees,
            InformationSources = dto.InformationSources,
            Description = dto.Description,
            Image = imagePath,
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
            NormalizedNameAr = ArabicTextNormalizer.Normalize(dto.NameAr),
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
                NormalizedNameAr = ArabicTextNormalizer.Normalize(d.NameAr),
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
            NormalizedNameAr = ArabicTextNormalizer.Normalize(dto.NameAr),
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

        // Handle image changes
        if (dto.RemoveImage && !string.IsNullOrEmpty(university.Image))
        {
            DeleteImage(university.Image);
            university.Image = null;
        }
        else if (dto.ImageFile != null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(university.Image))
            {
                DeleteImage(university.Image);
            }
            // Save new image
            university.Image = await SaveImageAsync(dto.ImageFile);
        }

        university.NameAr = dto.NameAr;
        university.NameEn = dto.NameEn;
        university.NormalizedNameAr = ArabicTextNormalizer.Normalize(dto.NameAr);
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
        college.NormalizedNameAr = ArabicTextNormalizer.Normalize(dto.NameAr);
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
        department.NormalizedNameAr = ArabicTextNormalizer.Normalize(dto.NameAr);
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

        // Delete associated image if exists
        if (!string.IsNullOrEmpty(university.Image))
        {
            DeleteImage(university.Image);
        }

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
