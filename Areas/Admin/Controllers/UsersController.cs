using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILessonProgressRepository _progressRepository;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILessonProgressRepository progressRepository)
        {
            _userManager = userManager;
            _context = context;
            _progressRepository = progressRepository;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index(string? search)
        {
            // Lấy tất cả người dùng trong role Student
            var studentUsers = await _userManager.GetUsersInRoleAsync(SD.Role_Student);

            // Lọc theo tìm kiếm nếu có
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                studentUsers = studentUsers
                    .Where(u => (u.FullName ?? "").ToLower().Contains(search) ||
                                (u.Email ?? "").ToLower().Contains(search))
                    .ToList();
            }

            // Lấy thống kê số khóa học đã đăng ký của từng học viên
            var enrollmentCounts = await _context.Enrollments
                .GroupBy(e => e.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StudentId, x => x.Count);

            ViewBag.EnrollmentCounts = enrollmentCounts;
            ViewBag.Search = search;

            return View(studentUsers);
        }

        // POST: /Admin/Users/ToggleLock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (await _userManager.IsLockedOutAsync(user))
            {
                // Mở khóa tài khoản
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddSeconds(-1));
                TempData["Success"] = $"Đã mở khóa tài khoản {user.Email}.";
            }
            else
            {
                // Khóa tài khoản 100 năm = vô thời hạn
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["Success"] = $"Đã khóa tài khoản {user.Email}.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Users/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Đã đặt lại mật khẩu cho {user.Email} thành công.";
            }
            else
            {
                TempData["Error"] = "Lỗi đặt lại mật khẩu: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Users/Progress/{userId} — JSON for modal
        public async Task<IActionResult> GetUserProgress(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == userId)
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .ToListAsync();

            var result = new List<object>();
            foreach (var e in enrollments)
            {
                var percentage = e.Course?.Lessons?.Any() == true
                    ? await _progressRepository.GetCourseCompletionPercentageAsync(userId, e.CourseId)
                    : 0;

                result.Add(new
                {
                    courseTitle = e.Course?.Title ?? "Không rõ",
                    lessonCount = e.Course?.Lessons?.Count ?? 0,
                    percentage = Math.Round(percentage, 1),
                    enrolledAt = e.EnrollDate.ToString("dd/MM/yyyy")
                });
            }

            return Json(new
            {
                fullName = user.FullName ?? user.Email,
                email = user.Email,
                createdAt = user.CreatedAt.ToString("dd/MM/yyyy"),
                courses = result
            });
        }
    }
}
