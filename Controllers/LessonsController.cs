using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LMS_DotNETCore_MVC.Controllers
{
    public class LessonsController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;

        public LessonsController(ILessonRepository lessonRepository, ICourseRepository courseRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        // 1. Hiển thị danh sách tất cả bài học (GET: Lessons)
        public async Task<IActionResult> Index()
        {
            var lessons = await _lessonRepository.GetAllLessonsAsync();
            return View(lessons);
        }

        // 2. Xem chi tiết bài học (GET: Lessons/Details/5)
        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _lessonRepository.GetLessonByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        // 3. Hiển thị form tạo bài học mới (GET: Lessons/Create?courseId=5)
        public async Task<IActionResult> Create(int courseId)
        {
            // Kiểm tra xem khóa học có tồn tại không trước khi cho tạo bài học
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var lesson = new Lesson { CourseId = courseId };
            return View(lesson);
        }

        // Xử lý lưu bài học mới (POST: Lessons/Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                // Thêm bài học mới thông qua Repository trực tiếp hoặc bổ sung hàm Add trong LessonRepository
                // Ở đây gọi trực tiếp DbContext qua repo hoặc viết thêm hàm Add vào ILessonRepository
                // Giả sử bạn đã thêm hàm AddLessonAsync vào ILessonRepository:
                await _lessonRepository.AddLessonAsync(lesson);
                return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
            }
            return View(lesson);
        }

        // 4. Xóa bài học (POST: Lessons/Delete/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            await _lessonRepository.DeleteLessonAsync(id);
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
    }
}
