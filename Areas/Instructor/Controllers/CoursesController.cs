using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ICourseRepository courseRepository, UserManager<ApplicationUser> userManager)
        {
            _courseRepository = courseRepository;
            _userManager = userManager;
        }

        // 1. HIỂN THỊ DANH SÁCH KHÓA HỌC (GET: Courses)
        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAllCoursesAsync();
            return View(courses);
        }

        // 2. XEM CHI TIẾT KHÓA HỌC & BÀI HỌC (GET: Courses/Details/5)
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // 3. HIỂN THỊ FORM TẠO KHÓA HỌC MỚI (GET: Courses/Create)
        // [Authorize] -> Bật dòng này nếu muốn bắt buộc phải đăng nhập mới được tạo khóa học
        public IActionResult Create()
        {
            return View();
        }

        // XỬ LÝ LƯU KHÓA HỌC MỚI (POST: Courses/Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(Course course, IFormFile imageUrl)
        {
            // 1. Lấy ID của User đang đăng nhập gắn vào khóa học
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge(); // Nếu chưa đăng nhập thì bắt đăng nhập lại
            }
            course.InstructorId = userId; // Gán InstructorId bằng ID của user hiện tại
            // Loại bỏ ModelState cho InstructorId nếu form không gửi trường này lên (tránh lỗi validation)
            ModelState.Remove(nameof(course.InstructorId));
            ModelState.Remove(nameof(course.Enrollments));
            ModelState.Remove(nameof(course.Instructor));
            ModelState.Remove(nameof(course.Lessons));
            // Gán tạm InstructorId hoặc lấy từ User đang đăng nhập nếu có Identity
            // Ở đây để mặc định hoặc test, bạn có thể truyền ID cố định hoặc bắt buộc nhập
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    course.ImageUrl = await SaveImage(imageUrl);
                }

                await _courseRepository.AddCourseAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // 4. HIỂN THỊ FORM CHỈNH SỬA KHÓA HỌC (GET: Courses/Edit/5)
        public async Task<IActionResult> Update(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // XỬ LÝ LƯU CẬP NHẬT (POST: Courses/Edit/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Course course, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl");

            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingCourse = await _courseRepository.GetCourseByIdAsync(id);

                if (imageUrl == null)
                {
                    course.ImageUrl = existingCourse.ImageUrl;
                }
                else
                {
                    // Lưu hình ảnh mới
                    course.ImageUrl = await SaveImage(imageUrl);
                }

                await _courseRepository.UpdateCourseAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // 5. XÓA KHÓA HỌC (GET: Courses/Delete/5 - Xác nhận xóa)
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // XỬ LÝ XÓA THỰC TẾ (POST: Courses/Delete/5)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _courseRepository.DeleteCourseAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            //Thay đổi đường dẫn theo cấu hình của bạn
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }
    }
}
