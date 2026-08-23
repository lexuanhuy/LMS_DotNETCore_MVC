using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = SD.Role_Instructor)]
    public class LessonsController : Controller
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public LessonsController(ApplicationDbContext context, ILessonRepository lessonRepository, ICourseRepository courseRepository, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            if (courseId == null)
            {
                // Nếu không có courseId truyền vào, đá về trang danh sách khóa học
                return RedirectToAction("Index", "Courses");
            }

            // Tìm khóa học để lấy tên hiển thị ra tiêu đề trang (nếu muốn)
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Không tìm thấy khóa học này.");
            }

            // Truyền course sang View để hiển thị tiêu đề khóa học hoặc dùng ViewBag lưu courseId
            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.Title;

            // Chỉ lấy danh sách bài học thuộc đúng courseId này
            var lessons = course.Lessons.OrderBy(l => l.OrderIndex).ToList();

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
        public async Task<IActionResult> Create(int? courseId)
        {
            if (courseId == null)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            // Khởi tạo model mới và gán sẵn CourseId để binding ra form ẩn (hidden input)
            var lesson = new Lesson { CourseId = courseId.Value };
            ViewBag.CourseTitle = course.Title;

            return View(lesson);
        }

        // Xử lý lưu bài học mới (POST: Lessons/Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lesson lesson, IFormFile? lessonFile)
        {
            // Bỏ qua validate các trường điều hướng hệ thống
            ModelState.Remove(nameof(lesson.Course));
            ModelState.Remove(nameof(lesson.ContentUrl));
            ModelState.Remove(nameof(lesson.Description)); // Cho phép HTML từ Summernote

            if (ModelState.IsValid)
            {
                // Xử lý upload file (Video hoặc PDF) vào thư mục wwwroot/lessons/{courseId}
                if (lessonFile != null && lessonFile.Length > 0)
                {
                    lesson.ContentUrl = await SaveLessonFile(lessonFile, lesson.CourseId);
                }

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
            }

            var course = await _context.Courses.FindAsync(lesson.CourseId);
            ViewBag.CourseTitle = course?.Title;

            return View(lesson);
        }

        // 4. HIỂN THỊ FORM CHỈNH SỬA BÀI HỌC (GET: Instructor/Lessons/Update/5)
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.CourseTitle = lesson.Course?.Title;
            return View(lesson);
        }

        // 5. XỬ LÝ LƯU CẬP NHẬT BÀI HỌC & FILE (POST: Instructor/Lessons/Update/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Lesson lesson, IFormFile? lessonFile)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }

            // Bỏ qua validate các trường hệ thống không gửi từ form cập nhật
            ModelState.Remove(nameof(lesson.Course));
            ModelState.Remove(nameof(lesson.ContentUrl));
            ModelState.Remove(nameof(lesson.Description)); // Cho phép HTML từ Summernote

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bài học hiện tại trong DB ra để đối chiếu file cũ
                    var existingLesson = await _context.Lessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
                    if (existingLesson == null)
                    {
                        return NotFound();
                    }

                    // Nếu người dùng KHÔNG chọn file mới -> giữ nguyên FileUrl cũ
                    if (lessonFile == null || lessonFile.Length == 0)
                    {
                        lesson.ContentUrl = existingLesson.ContentUrl;
                    }
                    else
                    {
                        // Nếu có chọn file mới -> Upload file mới và thay thế
                        lesson.ContentUrl = await SaveLessonFile(lessonFile, lesson.CourseId);

                        // (Tùy chọn) Xóa file cũ đi nếu muốn dọn ổ đĩa
                        if (!string.IsNullOrEmpty(existingLesson.ContentUrl))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingLesson.ContentUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                    }
                    //_context.Entry(lesson).State = EntityState.Modified;

                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Lessons.Any(e => e.Id == lesson.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Cập nhật xong chuyển hướng về trang danh sách bài học của khóa học đó
                return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
            }

            // Nếu lỗi ModelState, load lại tên khóa học rồi trả về form
            var course = await _context.Courses.FindAsync(lesson.CourseId);
            ViewBag.CourseTitle = course?.Title;

            return View(lesson);
        }

        // 4. Xóa bài học (POST: Lessons/Delete/5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            // (Tùy chọn) Xóa file vật lý trong thư mục wwwroot/lessons/{courseId} nếu có
            if (!string.IsNullOrEmpty(lesson.ContentUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, lesson.ContentUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            await _lessonRepository.DeleteLessonAsync(id);
            return RedirectToAction("Index", "Lessons", new { courseId = courseId });
        }

        private async Task<string> SaveLessonFile(IFormFile file, int courseId)
        {
            // Tạo tên file độc nhất tránh trùng lặp (kết hợp Guid và tên gốc)
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);

            // Đường dẫn thư mục: wwwroot/lessons/{courseId}
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "lessons", courseId.ToString());

            // Nếu thư mục chưa tồn tại thì tự động tạo mới
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn tương đối để lưu vào Database (VD: /lessons/5/abc_xyz.mp4)
            return $"/lessons/{courseId}/{uniqueFileName}";
        }
    }
}
