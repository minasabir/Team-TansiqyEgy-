using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Entities;

namespace TansiqyV1.DAL.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<University> Universities { get; set; }
    public DbSet<College> Colleges { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UniversityBranch> UniversityBranches { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<News> News { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure University
        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.OfficialWebsite).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Fees).HasPrecision(18, 2);
            entity.Property(e => e.LastYearCoordination).HasPrecision(18, 2);
            entity.HasIndex(e => e.NameAr);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Governorate);
            entity.HasIndex(e => e.IsDeleted);
            // Composite index for common queries
            entity.HasIndex(e => new { e.Type, e.IsDeleted });
            entity.HasIndex(e => new { e.Governorate, e.IsDeleted });
        });

        // Configure College
        modelBuilder.Entity<College>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.OfficialWebsite).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Fees).HasPrecision(18, 2);
            entity.Property(e => e.LastYearCoordination).HasPrecision(18, 2);
            
            // مصروفات بفئات
            entity.Property(e => e.FeesCategoryA).HasPrecision(18, 2);
            entity.Property(e => e.FeesCategoryB).HasPrecision(18, 2);
            entity.Property(e => e.FeesCategoryC).HasPrecision(18, 2);
            
            // مصروفات بالساعة
            entity.Property(e => e.FeesPerHour).HasPrecision(18, 2);
            entity.Property(e => e.AdditionalFees).HasPrecision(18, 2);
            
            entity.HasOne(e => e.University)
                  .WithMany(u => u.Colleges)
                  .HasForeignKey(e => e.UniversityId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UniversityId);
            entity.HasIndex(e => e.NameAr);
            entity.HasIndex(e => e.IsDeleted);
            // Composite index for common queries
            entity.HasIndex(e => new { e.UniversityId, e.IsDeleted });
        });

        // Configure Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            
            entity.HasOne(e => e.College)
                  .WithMany(c => c.Departments)
                  .HasForeignKey(e => e.CollegeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.CollegeId);
            entity.HasIndex(e => e.NameAr);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.StudyType);
            // Composite index for common queries
            entity.HasIndex(e => new { e.CollegeId, e.IsDeleted });
        });

        // Configure UniversityBranch
        modelBuilder.Entity<UniversityBranch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NameAr).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Location).HasMaxLength(500);
            
            entity.HasOne(e => e.University)
                  .WithMany(u => u.Branches)
                  .HasForeignKey(e => e.UniversityId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => e.UniversityId);
            entity.HasIndex(e => e.IsDeleted);
            // Composite index for common queries
            entity.HasIndex(e => new { e.UniversityId, e.IsDeleted });
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Role)
                  .HasConversion<int>()
                  .HasDefaultValue(Enums.UserRole.Admin);
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Configure News
        modelBuilder.Entity<News>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired();
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.IsDeleted);
        });
    }
}





