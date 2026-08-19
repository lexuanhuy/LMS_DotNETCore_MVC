using LMS_DotNETCore_MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        // =====================================================
        // DANH SÁCH GIẢNG VIÊN
        // GET: /Admin/Instructors
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var instructors = await _userManager
                .GetUsersInRoleAsync("Instructor");

            return View(instructors);
        }


        // =====================================================
        // THÊM GIẢNG VIÊN
        // POST: /Admin/Instructors/Create
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser instructor,
            string password)
        {
            // Kiểm tra mật khẩu
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "Vui lòng nhập mật khẩu."
                );
            }

            if (string.IsNullOrWhiteSpace(instructor.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Vui lòng nhập email."
                );
            }

            if (string.IsNullOrWhiteSpace(instructor.FullName))
            {
                ModelState.AddModelError(
                    "FullName",
                    "Vui lòng nhập họ tên."
                );
            }

            if (!ModelState.IsValid)
            {
                var instructors = await _userManager
                    .GetUsersInRoleAsync("Instructor");

                return View("Index", instructors);
            }

            // Thiết lập tài khoản
            instructor.UserName = instructor.Email;
            instructor.EmailConfirmed = true;
            instructor.CreatedAt = DateTime.UtcNow;
            instructor.UpdatedAt = DateTime.UtcNow;

            // Tạo tài khoản Identity
            var result = await _userManager.CreateAsync(
                instructor,
                password
            );

            if (result.Succeeded)
            {
                // Gán role Instructor
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        instructor,
                        "Instructor"
                    );

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description
                        );
                    }

                    await _userManager.DeleteAsync(instructor);

                    var instructors =
                        await _userManager.GetUsersInRoleAsync("Instructor");

                    return View("Index", instructors);
                }

                return RedirectToAction(nameof(Index));
            }

            // Hiển thị lỗi Identity
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            var list =
                await _userManager.GetUsersInRoleAsync("Instructor");

            return View("Index", list);
        }


        // =====================================================
        // SỬA GIẢNG VIÊN
        // POST: /Admin/Instructors/Edit
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            ApplicationUser model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var instructor =
                await _userManager.FindByIdAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            // Không validate các thuộc tính Identity
            ModelState.Remove("UserName");
            ModelState.Remove("Email");
            ModelState.Remove("PasswordHash");
            ModelState.Remove("SecurityStamp");
            ModelState.Remove("ConcurrencyStamp");

            if (!ModelState.IsValid)
            {
                var instructors =
                    await _userManager.GetUsersInRoleAsync("Instructor");

                return View("Index", instructors);
            }

            instructor.FullName = model.FullName;
            instructor.Email = model.Email;
            instructor.UserName = model.Email;
            instructor.PhoneNumber = model.PhoneNumber;
            instructor.DateOfBirth = model.DateOfBirth;
            instructor.AvatarPath = model.AvatarPath;
            instructor.Description = model.Description;
            instructor.UpdatedAt = DateTime.UtcNow;

            var result =
                await _userManager.UpdateAsync(instructor);

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

            var list =
                await _userManager.GetUsersInRoleAsync("Instructor");

            return View("Index", list);
        }


        // =====================================================
        // XÓA GIẢNG VIÊN
        // POST: /Admin/Instructors/Delete
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var instructor =
                await _userManager.FindByIdAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            var result =
                await _userManager.DeleteAsync(instructor);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description
                    );
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}