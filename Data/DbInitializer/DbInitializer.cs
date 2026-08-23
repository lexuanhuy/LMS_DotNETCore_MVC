using LMS_DotNETCore_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Data.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if ((await _db.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _db.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                // Console or log exception
                Console.WriteLine($"Error applying migrations: {ex.Message}");
            }

            // Seed Roles if not existing
            if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Instructor));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Student));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Guest));

                // Create Default Admin User
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@lms.com",
                    Email = "admin@lms.com",
                    FullName = "Quản trị viên Hệ thống",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                }

                // Create Default Instructor User
                var instructorUser = new ApplicationUser
                {
                    UserName = "instructor@lms.com",
                    Email = "instructor@lms.com",
                    FullName = "Giảng viên Mẫu",
                    Description = "Giảng viên với nhiều năm kinh nghiệm lập trình .NET",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var instResult = await _userManager.CreateAsync(instructorUser, "Instructor@123");
                if (instResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(instructorUser, SD.Role_Instructor);
                }
            }

            // Seed Categories if not existing
            if (!await _db.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Lập trình", Description = "Web, Mobile, Software Development", IconClass = "💻", DisplayOrder = 1 },
                    new Category { Name = "Cơ sở dữ liệu", Description = "SQL, MySQL, SQL Server, MongoDB", IconClass = "🗄️", DisplayOrder = 2 },
                    new Category { Name = "Thiết kế", Description = "UI/UX, Figma, Photoshop, Illustrator", IconClass = "🎨", DisplayOrder = 3 },
                    new Category { Name = "Kinh doanh", Description = "Marketing, Sales, Business Strategy", IconClass = "📊", DisplayOrder = 4 }
                };

                await _db.Categories.AddRangeAsync(categories);
                await _db.SaveChangesAsync();
            }
        }
    }
}
