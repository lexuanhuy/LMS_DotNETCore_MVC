using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // DANH SÁCH KHÓA HỌC
        // GET: /Admin/Courses
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Danh sách người dùng để chọn làm giảng viên
            var instructors = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View(courses);
        }


        // =====================================================
        // THÊM KHÓA HỌC
        // POST: /Admin/Courses/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            // Không bind các navigation property
            ModelState.Remove("Instructor");
            ModelState.Remove("Lessons");
            ModelState.Remove("Enrollments");

            if (string.IsNullOrWhiteSpace(course.InstructorId))
            {
                ModelState.AddModelError(
                    "InstructorId",
                    "Vui lòng chọn giảng viên."
                );
            }

            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.Now;

                _context.Courses.Add(course);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Thêm khóa học thành công!";

                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi validation thì load lại danh sách
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var instructors = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View("Index", courses);
        }


        // =====================================================
        // SỬA KHÓA HỌC
        // POST: /Admin/Courses/Edit
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            ModelState.Remove("Instructor");
            ModelState.Remove("Lessons");
            ModelState.Remove("Enrollments");

            if (id != course.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(course.InstructorId))
            {
                ModelState.AddModelError(
                    "InstructorId",
                    "Vui lòng chọn giảng viên."
                );
            }

            if (ModelState.IsValid)
            {
                var existingCourse =
                    await _context.Courses.FindAsync(id);

                if (existingCourse == null)
                {
                    return NotFound();
                }

                // Chỉ cập nhật những thông tin cần thiết
                existingCourse.Title = course.Title;
                existingCourse.Description = course.Description;
                existingCourse.Price = course.Price;
                existingCourse.ImageUrl = course.ImageUrl;
                existingCourse.InstructorId = course.InstructorId;

                // Không thay đổi CreatedAt

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Cập nhật khóa học thành công!";

                return RedirectToAction(nameof(Index));
            }

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var instructors = await _context.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Instructors = instructors;

            return View("Index", courses);
        }


        // =====================================================
        // XÓA KHÓA HỌC
        // POST: /Admin/Courses/Delete
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Đã xóa khóa học thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}