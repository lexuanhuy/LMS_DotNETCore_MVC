using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LMS_DotNETCore_MVC.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;

        public CoursesController(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 6; // Số lượng khóa học hiển thị trên 1 trang (bạn có thể đổi thành 9, 12...)

            // Gọi Repository lấy danh sách và tổng số trang
            var result = await _courseRepository.GetCoursesPaginatedAsync(searchString, page, pageSize);

            // Truyền dữ liệu phân trang và từ khóa tìm kiếm ra giao diện View qua ViewBag
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;

            return View(result.Courses);
        }

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
        public async Task<IActionResult> Create(Course course, IFormFile imageUrl)
        {
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
        public async Task<IActionResult> Edit(int id)
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
        public async Task<IActionResult> Edit(int id, Course course, IFormFile imageUrl)
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
