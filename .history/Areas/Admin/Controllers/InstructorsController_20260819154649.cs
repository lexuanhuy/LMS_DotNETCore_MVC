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
            var instructors =
                await _userManager.GetUsersInRoleAsync("Instructor");

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
            // Kiểm tra họ tên
            if (string.IsNullOrWhiteSpace(instructor.FullName))
            {
                TempData["Error"] =
                    "Vui lòng nhập họ và tên giảng viên.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra email
            if (string.IsNullOrWhiteSpace(instructor.Email))
            {
                TempData["Error"] =
                    "Vui lòng nhập email.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra password
            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] =
                    "Vui lòng nhập mật khẩu.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra email đã tồn tại
            var existingUser =
                await _userManager.FindByEmailAsync(
                    instructor.Email
                );

            if (existingUser != null)
            {
                TempData["Error"] =
                    "Email này đã được sử dụng.";

                return RedirectToAction(nameof(Index));
            }


            // =================================================
            // TẠO TÀI KHOẢN
            // =================================================

            instructor.UserName = instructor.Email;
            instructor.EmailConfirmed = true;
            instructor.CreatedAt = DateTime.UtcNow;
            instructor.UpdatedAt = DateTime.UtcNow;


            var result =
                await _userManager.CreateAsync(
                    instructor,
                    password
                );


            // =================================================
            // TẠO THẤT BẠI
            // =================================================

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
            // GÁN ROLE INSTRUCTOR
            // =================================================

            var roleResult =
                await _userManager.AddToRoleAsync(
                    instructor,
                    "Instructor"
                );


            if (!roleResult.Succeeded)
            {
                // Nếu gán role thất bại thì xóa user vừa tạo
                await _userManager.DeleteAsync(instructor);

                TempData["Error"] =
                    "Không thể cấp quyền Giảng viên.";

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


            // =================================================
            // KIỂM TRA DỮ LIỆU
            // =================================================

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                TempData["Error"] =
                    "Họ và tên không được để trống.";

                return RedirectToAction(nameof(Index));
            }


            if (string.IsNullOrWhiteSpace(model.Email))
            {
                TempData["Error"] =
                    "Email không được để trống.";

                return RedirectToAction(nameof(Index));
            }


            // =================================================
            // KIỂM TRA EMAIL TRÙNG
            // =================================================

            var emailUser =
                await _userManager.FindByEmailAsync(
                    model.Email
                );


            if (emailUser != null &&
                emailUser.Id != instructor.Id)
            {
                TempData["Error"] =
                    "Email này đã được sử dụng bởi tài khoản khác.";

                return RedirectToAction(nameof(Index));
            }


            // =================================================
            // CẬP NHẬT
            // =================================================

            instructor.FullName =
                model.FullName;

            instructor.Email =
                model.Email;

            instructor.UserName =
                model.Email;

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


            // =================================================
            // THẤT BẠI
            // =================================================

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
            // THÀNH CÔNG
            // =================================================

            TempData["Success"] =
                "Cập nhật giảng viên thành công!";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // XÓA GIẢNG VIÊN
        // POST: /Admin/Instructors/Delete
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            string id)
        {
            if (string.IsNullOrEmpty(id))
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
                    "Giảng viên không tồn tại.";

                return RedirectToAction(nameof(Index));
            }


            var result =
                await _userManager.DeleteAsync(
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
                "Xóa giảng viên thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}