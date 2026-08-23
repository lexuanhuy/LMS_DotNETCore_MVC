using LMS_DotNETCore_MVC.Models;
using LMS_DotNETCore_MVC.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LMS_DotNETCore_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(
            ILogger<HomeController> logger,
            ICourseRepository courseRepository,
            ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseRepository.GetAllCoursesAsync();
            ViewBag.Categories = await _categoryRepository.GetAllCategoriesAsync();
            return View(courses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
