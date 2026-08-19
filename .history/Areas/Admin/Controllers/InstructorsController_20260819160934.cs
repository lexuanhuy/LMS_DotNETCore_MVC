using LMS_DotNETCore_MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS_DotNETCore_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InstructorsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public InstructorsController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =====================================================
        // DANH SÁCH GIẢNG VIÊN
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instructors =
                await _userManager.GetUsersInRoleAsync("Instructor");

            return View(instructors);
        }


        // =====================================================
        // THÊM GIẢNG VIÊN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser instructor,
            string password)
        {
            // -----------------------------
            // KIỂM TRA HỌ TÊN
            // -----------------------------
            if (string.IsNullOrWhiteSpace(instructor.FullName))
            {
                TempData["Error"] =
                    "Vui lòng nhập họ và tên giảng viên.";

                return RedirectToAction(nameof(Index));
            }

            // -----------------------------
            // KIỂM TRA EMAIL
            // -----------------------------
            if (string.IsNullOrWhiteSpace(instructor.Email))
            {
                TempData["Error"] =
                    "Vui lòng nhập email.";

                return RedirectToAction(nameof(Index));
            }

            // -----------------------------
            // KIỂM TRA PASSWORD
            // -----------------------------
            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] =
                    "Vui lòng nhập mật khẩu.";

                return RedirectToAction(nameof(Index));
            }

            // -----------------------------
            // KIỂM TRA EMAIL ĐÃ TỒN TẠI
            // -----------------------------
            var existingUser =
                await _userManager.FindByEmailAsync(
                    instructor.Email.Trim()
                );

            if (existingUser != null)
            {
                TempData["Error"] =
                    "Email này đã được sử dụng.";

                return RedirectToAction(nameof(Index));
            }

            // -----------------------------
            // THIẾT LẬP USER
            // -----------------------------
            instructor.Id = Guid.NewGuid().ToString();
            instructor.Email =
                instructor.Email.Trim();

            instructor.UserName =
                instructor.Email;

            instructor.EmailConfirmed = true;

            instructor.CreatedAt =
                DateTime.UtcNow;

            instructor.UpdatedAt =
                DateTime.UtcNow;


            // -----------------------------
            // TẠO USER
            // -----------------------------
            var result =
                await _userManager.CreateAsync(
                    instructor,
                    password
                );

            if (!result.Succeeded)
            {
                TempData["Error"] =
                    string.Join(
                        " | ",
                        result.Errors.Select(
                            e => e.Description
                        )
                    );

                return RedirectToAction(nameof(Index));
            }


            // =================================================
            // KIỂM TRA ROLE INSTRUCTOR
            // =================================================

            if (!await _roleManager.RoleExistsAsync("Instructor"))
            {
                var createRoleResult =
                    await _roleManager.CreateAsync(
                        new IdentityRole("Instructor")
                    );

                if (!createRoleResult.Succeeded)
                {
                    // Không tạo được role -> xóa user
                    await _userManager.DeleteAsync(instructor);

                    TempData["Error"] =
                        "Không thể tạo quyền Instructor.";

                    return RedirectToAction(nameof(Index));
                }
            }


            // =================================================
            // GÁN ROLE INSTRUCTOR
            // =================================================

            var roleResult =
                await _userManager.AddToRoleAsync(
                    instructor,
                    "Instructor"
                );

            if (!roleResult.Succeeded)
            {
                // Nếu gán role thất bại -> xóa user
                await _userManager.DeleteAsync(instructor);

                TempData["Error"] =
                    string.Join(
                        " | ",
                        roleResult.Errors.Select(
                            e => e.Description
                        )
                    );

                return RedirectToAction(nameof(Index));
            }


            // =================================================
            // THÀNH CÔNG
            // =================================================

            TempData["Success"] =
                "Thêm giảng viên thành công!";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // SỬA GIẢNG VIÊN
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            ApplicationUser model)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] =
                    "Không tìm thấy giảng viên.";

                return RedirectToAction(nameof(Index));
            }


            var instructor =
                await _userManager.FindByIdAsync(id);

            if (instructor == null)
            {
                TempData["Error"] =
                    "Không tìm thấy giảng viên.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------
            // KIỂM TRA HỌ TÊN
            // -----------------------------
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                TempData["Error"] =
                    "Họ và tên không được để trống.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------
            // KIỂM TRA EMAIL
            // -----------------------------
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                TempData["Error"] =
                    "Email không được để trống.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------
            // KIỂM TRA EMAIL TRÙNG
            // -----------------------------
            var emailUser =
                await _userManager.FindByEmailAsync(
                    model.Email.Trim()
                );

            if (emailUser != null &&
                emailUser.Id != instructor.Id)
            {
                TempData["Error"] =
                    "Email này đã được sử dụng bởi tài khoản khác.";

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------
            // CẬP NHẬT
            // -----------------------------
            instructor.FullName =
                model.FullName.Trim();

            instructor.Email =
                model.Email.Trim();

            instructor.UserName =
                model.Email.Trim();

            instructor.PhoneNumber =
                model.PhoneNumber;

            instructor.Description =
                model.Description;

            instructor.DateOfBirth =
                model.DateOfBirth;

            instructor.AvatarPath =
                model.AvatarPath;

            instructor.UpdatedAt =
                DateTime.UtcNow;


            var result =
                await _userManager.UpdateAsync(
                    instructor
                );


            if (!result.Succeeded)
            {
                TempData["Error"] =
                    string.Join(
                        " | ",
                        result.Errors.Select(
                            e => e.Description
                        )
                    );

                return RedirectToAction(nameof(Index));
            }


            TempData["Success"] =
                "Cập nhật giảng viên thành công!";

            return RedirectToAction(nameof(Index));
        }


      // =====================================================
    /   / XÓA GIẢNG VIÊN
// POST: /Admin/Instructors/Delete
// =====================================================
[HttpPost]
[ValidateAntiForgeryToken]
[Route("Admin/Instructors/Delete")]
public async Task<IActionResult> Delete(string id)
{
    if (string.IsNullOrWhiteSpace(id))
    {
        TempData["Error"] = "Không tìm thấy giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    var instructor = await _userManager.FindByIdAsync(id);

    if (instructor == null)
    {
        TempData["Error"] = "Giảng viên không tồn tại.";
        return RedirectToAction(nameof(Index));
    }

    // Kiểm tra đúng là Instructor
    var isInstructor = await _userManager.IsInRoleAsync(
        instructor,
        "Instructor"
    );

    if (!isInstructor)
    {
        TempData["Error"] = "Tài khoản này không phải giảng viên.";
        return RedirectToAction(nameof(Index));
    }

    var result = await _userManager.DeleteAsync(instructor);

    if (!result.Succeeded)
    {
        TempData["Error"] = string.Join(
            " | ",
            result.Errors.Select(e => e.Description)
        );

        return RedirectToAction(nameof(Index));
    }

    TempData["Success"] = "Xóa giảng viên thành công!";

    return RedirectToAction(nameof(Index));
}
    }
}