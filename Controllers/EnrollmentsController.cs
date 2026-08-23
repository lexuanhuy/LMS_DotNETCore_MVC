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
        private readonly ILessonProgressRepository _progressRepository;

        public EnrollmentsController(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            ILessonProgressRepository progressRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _progressRepository = progressRepository;
        }

        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Index()
        {
            var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
            return View(enrollments);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            if (User.IsInRole(SD.Role_Admin))
            {
                TempData["Error"] = "Tài khoản Admin không đăng ký học viên. Vui lòng sử dụng trang Quản trị.";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            if (User.IsInRole(SD.Role_Instructor))
            {
                TempData["Error"] = "Tài khoản Giảng viên không đăng ký học viên. Vui lòng sử dụng Kênh Giảng viên.";
                return RedirectToAction("Index", "Dashboard", new { area = "Instructor" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var existingEnrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(userId);
            if (existingEnrollments.Any(e => e.CourseId == courseId))
            {
                TempData["Success"] = "Bạn đã đăng ký khóa học này trước đó rồi!";
                return RedirectToAction(nameof(MyCourses));
            }

            var enrollment = new Enrollment
            {
                StudentId = userId,
                CourseId = courseId,
                EnrollDate = DateTime.Now
            };

            await _enrollmentRepository.AddEnrollmentAsync(enrollment);
            TempData["Success"] = "Đăng ký khóa học thành công! Chúc bạn học tốt.";
            return RedirectToAction(nameof(MyCourses));
        }

        [Authorize]
        public async Task<IActionResult> MyCourses()
        {
            if (User.IsInRole(SD.Role_Admin))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            if (User.IsInRole(SD.Role_Instructor))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Instructor" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            var myEnrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(userId);

            var progressDict = new Dictionary<int, double>();
            foreach (var item in myEnrollments)
            {
                double pct = await _progressRepository.GetCourseCompletionPercentageAsync(userId, item.CourseId);
                progressDict[item.CourseId] = pct;
            }

            ViewBag.ProgressDict = progressDict;
            return View(myEnrollments);
        }
    }
}
