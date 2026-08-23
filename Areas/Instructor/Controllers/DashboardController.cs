using LMS_DotNETCore_MVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            var userId = User.Identity?.Name;
            // Prefer NameIdentifier for id if available
            if (User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) is var idClaim && idClaim != null)
            {
                userId = idClaim.Value;
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Courses owned by this instructor
            var instructorCourses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .ToListAsync();

            var totalCourses = instructorCourses.Count;
            var totalLessons = instructorCourses.Sum(c => c.Lessons?.Count ?? 0);
            // distinct students across all courses
            var totalStudents = instructorCourses
                .SelectMany(c => c.Enrollments)
                .Select(e => e.StudentId)
                .Distinct()
                .Count();

            // Revenue: sum of (course.Price * enrollmentsCount) where price != null
            decimal revenue = 0m;
            foreach (var c in instructorCourses)
            {
                var price = c.Price ?? 0m;
                var enrollCount = c.Enrollments?.Count ?? 0;
                revenue += price * enrollCount;
            }

            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalLessons = totalLessons;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.Revenue = revenue == 0m ? "0" : revenue.ToString("C");

            return View();
        }
    }
}