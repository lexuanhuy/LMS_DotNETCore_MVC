using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using LMS_DotNETCore_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS_DotNETCore_MVC.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = SD.Role_Instructor)]
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(
            ICourseRepository courseRepository,
            ICategoryRepository categoryRepository,
            IFileStorageService fileStorageService,
            UserManager<ApplicationUser> userManager)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _fileStorageService = fileStorageService;
            _userManager = userManager;
        }

        // 1. HIỂN THỊ DANH SÁCH KHÓA HỌC CỦA GIẢNG VIÊN (GET: Instructor/Courses)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var courses = await _courseRepository.GetAllCoursesAsync();
            var myCourses = courses.Where(c => c.InstructorId == userId).ToList();
            return View(myCourses);
        }

        // 2. XEM CHI TIẾT KHÓA HỌC & BÀI HỌC
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // 3. HIỂN THỊ FORM TẠO KHÓA HỌC MỚI
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View();
        }

        // XỬ LÝ LƯU KHÓA HỌC MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? imageUrl)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            course.InstructorId = userId;
            ModelState.Remove(nameof(course.InstructorId));
            ModelState.Remove(nameof(course.Enrollments));
            ModelState.Remove(nameof(course.Instructor));
            ModelState.Remove(nameof(course.Lessons));
            ModelState.Remove(nameof(course.Category));

            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    course.ImageUrl = await _fileStorageService.SaveFileAsync(imageUrl, "images/courses");
                }

                await _courseRepository.AddCourseAsync(course);
                TempData["Success"] = "Tạo khóa học mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View(course);
        }

        // 4. HIỂN THỊ FORM CHỈNH SỬA KHÓA HỌC
        public async Task<IActionResult> Update(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            var userId = _userManager.GetUserId(User);
            if (course == null || course.InstructorId != userId)
            {
                return NotFound();
            }

            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View(course);
        }

        // XỬ LÝ LƯU CẬP NHẬT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Course course, IFormFile? imageUrl)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Bỏ qua các ModelState không cần validate từ form gửi lên
            ModelState.Remove("ImageUrl");
            ModelState.Remove(nameof(course.InstructorId));
            ModelState.Remove(nameof(course.Enrollments));
            ModelState.Remove(nameof(course.Instructor));
            ModelState.Remove(nameof(course.Lessons));
            ModelState.Remove(nameof(course.Category));

            if (ModelState.IsValid)
            {
                // 1. Lấy course gốc đang bị track bởi DbContext
                var existingCourse = await _courseRepository.GetCourseByIdAsync(id);
                if (existingCourse == null || existingCourse.InstructorId != userId)
                {
                    return NotFound();
                }

                // 2. Cập nhật các trường thông tin thay đổi từ form vào object đang được track
                existingCourse.Title = course.Title;
                existingCourse.Description = course.Description;
                existingCourse.Price = course.Price; // (Đảm bảo model Course của bạn có thuộc tính Price, nếu tên khác bạn chỉnh lại cho khớp)
                existingCourse.CategoryId = course.CategoryId;

                // 3. Xử lý lưu file ảnh (nếu có upload ảnh mới)
                if (imageUrl != null)
                {
                    existingCourse.ImageUrl = await _fileStorageService.SaveFileAsync(imageUrl, "images/courses");
                }
                // Nếu không upload ảnh mới thì giữ nguyên existingCourse.ImageUrl cũ, không làm gì cả.

                // 4. Gọi repository cập nhật (truyền existingCourse thay vì course)
                await _courseRepository.UpdateCourseAsync(existingCourse);

                TempData["Success"] = "Cập nhật khóa học thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View(course);
        }

        // 5. XÓA KHÓA HỌC
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            var userId = _userManager.GetUserId(User);
            if (course == null || course.InstructorId != userId)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            var userId = _userManager.GetUserId(User);
            if (course != null && course.InstructorId == userId)
            {
                await _courseRepository.DeleteCourseAsync(id);
                TempData["Success"] = "Xóa khóa học thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
