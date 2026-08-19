using LMS_DotNETCore_MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InstructorsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public InstructorsController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Admin/Instructors
        public async Task<IActionResult> Index()
        {
            // Lấy những User có role Instructor
            var instructors = await _userManager
                .GetUsersInRoleAsync("Instructor");

            return View(instructors);
        }

        // GET: /Admin/Instructors/Details/ID
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var instructor = await _userManager.FindByIdAsync(id);

            if (instructor == null)
                return NotFound();

            return View(instructor);
        }

        // GET: /Admin/Instructors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Instructors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser instructor,
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Vui lòng nhập mật khẩu."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(instructor);
            }

            instructor.UserName = instructor.Email;
            instructor.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(
                instructor,
                password
            );

            if (result.Succeeded)
            {
                // Gán role Instructor
                await _userManager.AddToRoleAsync(
                    instructor,
                    "Instructor"
                );

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            return View(instructor);
        }

        // GET: /Admin/Instructors/Edit/ID
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var instructor = await _userManager.FindByIdAsync(id);

            if (instructor == null)
                return NotFound();

            return View(instructor);
        }

        // POST: /Admin/Instructors/Edit/ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            ApplicationUser model)
        {
            if (id != model.Id)
                return NotFound();

            var instructor = await _userManager.FindByIdAsync(id);

            if (instructor == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            instructor.FullName = model.FullName;
            instructor.Email = model.Email;
            instructor.UserName = model.Email;
            instructor.PhoneNumber = model.PhoneNumber;
            instructor.DateOfBirth = model.DateOfBirth;
            instructor.AvatarPath = model.AvatarPath;
            instructor.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(instructor);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            return View(model);
        }

        // GET: /Admin/Instructors/Delete/ID
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var instructor = await _userManager.FindByIdAsync(id);

            if (instructor == null)
                return NotFound();

            return View(instructor);
        }

        // POST: /Admin/Instructors/Delete/ID
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var instructor = await _userManager.FindByIdAsync(id);

            if (instructor == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(instructor);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            return RedirectToAction(nameof(Index));
        }
    }
}