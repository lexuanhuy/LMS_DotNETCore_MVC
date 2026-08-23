using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using LMS_DotNETCore_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS_DotNETCore_MVC.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILessonProgressRepository _progressRepository;
        private readonly ICourseReviewRepository _reviewRepository;
        private readonly IFileStorageService _fileStorageService;

        public CoursesController(
            ICourseRepository courseRepository,
            ICategoryRepository categoryRepository,
            ILessonProgressRepository progressRepository,
            ICourseReviewRepository reviewRepository,
            IFileStorageService fileStorageService)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _progressRepository = progressRepository;
            _reviewRepository = reviewRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId, int page = 1)
        {
            int pageSize = 6;
            var result = await _courseRepository.GetCoursesPaginatedAsync(searchString, page, pageSize);

            var courses = result.Courses;
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                courses = courses.Where(c => c.CategoryId == categoryId.Value).ToList();
            }

            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = result.TotalPages;

            return View(courses);
        }

        public async Task<IActionResult> Details(int id, int? lessonId)
        {
            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isEnrolled = false;
            double completionPercentage = 0;
            IEnumerable<int> completedLessonIds = new List<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var enrollments = course.Enrollments;
                isEnrolled = enrollments != null && enrollments.Any(e => e.StudentId == userId);
                if (isEnrolled)
                {
                    completedLessonIds = await _progressRepository.GetCompletedLessonIdsAsync(userId, id);
                    completionPercentage = await _progressRepository.GetCourseCompletionPercentageAsync(userId, id);
                }
            }

            var reviews = await _reviewRepository.GetReviewsByCourseIdAsync(id);
            double avgRating = await _reviewRepository.GetAverageRatingAsync(id);

            ViewBag.IsEnrolled = isEnrolled;
            ViewBag.CompletedLessonIds = completedLessonIds;
            ViewBag.CompletionPercentage = completionPercentage;
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = avgRating;
            ViewBag.CurrentLessonId = lessonId ?? course.Lessons.FirstOrDefault()?.Id;

            return View(course);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProgress(int lessonId, int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            await _progressRepository.ToggleLessonProgressAsync(userId, lessonId);
            return RedirectToAction(nameof(Details), new { id = courseId, lessonId = lessonId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int courseId, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var review = new CourseReview
            {
                CourseId = courseId,
                StudentId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            await _reviewRepository.AddReviewAsync(review);
            TempData["Success"] = "Cảm ơn bạn đã gửi đánh giá cho khóa học!";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    course.ImageUrl = await _fileStorageService.SaveFileAsync(imageUrl, "images");
                }

                await _courseRepository.AddCourseAsync(course);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View(course);
        }
    }
}
