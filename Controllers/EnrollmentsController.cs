using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS_DotNETCore_MVC.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;

        public EnrollmentsController(IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
        }

        // 1. Hiển thị danh sách tất cả các lượt đăng ký (Dành cho Admin - GET: Enrollments)
        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
            return View(enrollments);
        }

        // 2. Chức năng học viên bấm nút "Đăng ký khóa học" (POST: Enrollments/Enroll)
        [HttpPost]
        [Authorize] // Bắt buộc phải đăng nhập mới được đăng ký học
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            // Lấy ID của user đang đăng nhập từ hệ thống Identity
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge(); // Chuyển hướng về trang đăng nhập nếu chưa login
            }

            // Kiểm tra xem khóa học có tồn tại không
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // Tạo bản ghi đăng ký mới
            var enrollment = new Enrollment
            {
                StudentId = userId,
                CourseId = courseId
            };

            // Lưu vào DB (Bạn nhớ viết thêm hàm AddEnrollmentAsync trong IEnrollmentRepository nhé)
            await _enrollmentRepository.AddEnrollmentAsync(enrollment);

            // Đăng ký xong chuyển hướng về trang "Khóa học của tôi"
            return RedirectToAction(nameof(MyCourses));
        }

        // 3. Trang "Khóa học của tôi" - Hiển thị danh sách khóa học mà user hiện tại đã đăng ký (GET: Enrollments/MyCourses)
        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var myEnrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(userId);
            return View(myEnrollments);
        }
    }
}
