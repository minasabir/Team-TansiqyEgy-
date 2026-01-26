using Microsoft.EntityFrameworkCore;
using TansiqyV1.DAL.Database;
using TansiqyV1.DAL.Repo.Abstraction;
using TansiqyV1.DAL.Repo.Implementation;
using TansiqyV1.BLL.Services.Abstraction;
using TansiqyV1.BLL.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Response Caching
builder.Services.AddResponseCaching();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=TansiqyDB;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();

// Register Services
builder.Services.AddScoped<IUniversityService, UniversityService>();

var app = builder.Build();

// Database already seeded - data is in SQL Server
// To re-seed data, uncomment the following code:
/*
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DataSeeder.SeedDataAsync(context);
        Console.WriteLine("Database seeded successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use Response Caching
app.UseResponseCaching();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
