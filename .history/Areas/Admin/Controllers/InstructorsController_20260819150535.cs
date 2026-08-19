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

        public InstructorsController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin/Instructors
        public async Task<IActionResult> Index()
        {
            var instructors = new List<ApplicationUser>();

            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Instructor"))
                {
                    instructors.Add(user);
                }
            }

            return View(instructors);
        }

        // POST: Thêm giảng viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser instructor,
            string password)
        {
            if (string.IsNullOrWhiteSpace(instructor.Email))
            {
                TempData["Error"] = "Vui lòng nhập email.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Vui lòng nhập mật khẩu.";
                return RedirectToAction(nameof(Index));
            }

            var existingUser =
                await _userManager.FindByEmailAsync(instructor.Email);

            if (existingUser != null)
            {
                TempData["Error"] =
                    "Email này đã tồn tại trong hệ thống.";

                return RedirectToAction(nameof(Index));
            }

            instructor.UserName = instructor.Email;

            var result = await _userManager.CreateAsync(
                instructor,
                password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    instructor,
                    "Instructor");

                TempData["Success"] =
                    "Thêm giảng viên thành công.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = string.Join(
                " ",
                result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Index));
        }

        // POST: Sửa giảng viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            string fullName,
            string email,
            string? phoneNumber,
            string? description)
        {
            var instructor =
                await _userManager.FindByIdAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            instructor.FullName = fullName;
            instructor.Email = email;
            instructor.UserName = email;
            instructor.PhoneNumber = phoneNumber;
            instructor.Description = description;

            var result =
                await _userManager.UpdateAsync(instructor);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Cập nhật giảng viên thành công.";
            }
            else
            {
                TempData["Error"] = string.Join(
                    " ",
                    result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa giảng viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var instructor =
                await _userManager.FindByIdAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            var result =
                await _userManager.DeleteAsync(instructor);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Đã xóa giảng viên.";
            }
            else
            {
                TempData["Error"] =
                    "Không thể xóa giảng viên.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}