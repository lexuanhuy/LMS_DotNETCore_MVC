using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LMS_DotNETCore_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // DANH SÁCH KHÓA HỌC
        // GET: /Admin/Courses
        // =========================================================
       public async Task<IActionResult> Index()
{
    var courses = await _context.Courses
        .Include(c => c.Instructor)
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync();

    // Load danh sách giảng viên cho modal Thêm/Sửa
    await LoadInstructors();

    return View(courses);
}


        // =========================================================
        // CHI TIẾT KHÓA HỌC
        // GET: /Admin/Courses/Details/5
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }


        // =========================================================
        // THÊM KHÓA HỌC
        // GET: /Admin/Courses/Create
        // =========================================================
        public async Task<IActionResult> Create()
        {
            await LoadInstructors();

            return View();
        }


        // =========================================================
        // LƯU KHÓA HỌC
        // POST: /Admin/Courses/Create
        // =========================================================
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    Course course,
    IFormFile? imageFile)
{
    ModelState.Remove("Instructor");
    ModelState.Remove("Lessons");
    ModelState.Remove("Enrollments");

    if (ModelState.IsValid)
    {
        // Nếu upload file thì lưu file
        if (imageFile != null && imageFile.Length > 0)
        {
            course.ImageUrl = await SaveImage(imageFile);
        }

        // Nếu không upload file thì giữ ImageUrl từ form
        course.CreatedAt = DateTime.Now;

        _context.Courses.Add(course);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    await LoadInstructors();

    return View(course);
}

        // =========================================================
        // CHỈNH SỬA KHÓA HỌC
        // GET: /Admin/Courses/Edit/5
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            await LoadInstructors();

            return View(course);
        }


        // =========================================================
        // LƯU CHỈNH SỬA
        // POST: /Admin/Courses/Edit/5
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Course course,
            IFormFile? imageFile)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Instructor");
            ModelState.Remove("Lessons");
            ModelState.Remove("Enrollments");
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy khóa học cũ
                    var existingCourse = await _context.Courses
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin
                    existingCourse.Title = course.Title;
                    existingCourse.Description = course.Description;
                    existingCourse.Price = course.Price;
                    existingCourse.InstructorId = course.InstructorId;

                    // Nếu có ảnh mới
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        existingCourse.ImageUrl =
                            await SaveImage(imageFile);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Courses.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            await LoadInstructors();

            return View(course);
        }


        // =========================================================
        // XÁC NHẬN XÓA
        // GET: /Admin/Courses/Delete/5
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }


        // =========================================================
        // XÓA KHÓA HỌC
        // POST: /Admin/Courses/Delete/5
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);

            if (course != null)
            {
                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // LOAD DANH SÁCH GIẢNG VIÊN
        // =========================================================
        private async Task LoadInstructors()
        {
            var instructors = await _userManager.Users
                .Where(u => u.Email != null)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.Instructors = instructors;
        }


        // =========================================================
        // LƯU HÌNH ẢNH
        // =========================================================
        private async Task<string> SaveImage(IFormFile image)
        {
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "courses"
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var extension =
                Path.GetExtension(image.FileName);

            var fileName =
                Guid.NewGuid().ToString() + extension;

            var filePath =
                Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return "/images/courses/" + fileName;
        }
    }
}