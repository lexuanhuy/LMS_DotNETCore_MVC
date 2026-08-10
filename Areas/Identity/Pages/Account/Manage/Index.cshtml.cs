// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS_DotNETCore_MVC.Data;

namespace LMS_DotNETCore_MVC.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _environment;
    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = default!;

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public ApplicationUser CurrentUser { get; set; } = default!;
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }
        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        Username = userName;

        CurrentUser = user;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        bool isProfileChanged = false;

        // 1. Cập nhật FullName
        if (Input.FullName != user.FullName)
        {
            user.FullName = Input.FullName;
            isProfileChanged = true;
        }

        // 2. Cập nhật DateOfBirth
        if (Input.DateOfBirth != user.DateOfBirth)
        {
            user.DateOfBirth = Input.DateOfBirth;
            isProfileChanged = true;
        }

        // --- XỬ LÝ UPLOAD AVATAR MỚI ---
        if (Input.AvatarFile != null)
        {
            // 1. Kiểm tra định dạng file (chỉ cho phép ảnh)
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(Input.AvatarFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !Array.Exists(permittedExtensions, e => e == ext))
            {
                ModelState.AddModelError("Input.AvatarFile", "Chỉ chấp nhận các file ảnh định dạng .jpg, .jpeg, .png, .gif");
                await LoadAsync(user);
                return Page();
            }

            // 2. Tạo tên file duy nhất để tránh trùng lặp (ví dụ: guid_tenfile.jpg)
            string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(Input.AvatarFile.FileName)}";

            // 3. Định nghĩa đường dẫn lưu file vật lý trên server (trong wwwroot/images/avatars/)
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "avatars");
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. Tạo thư mục nếu nó chưa tồn tại
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 5. Lưu file vật lý xuống server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Input.AvatarFile.CopyToAsync(fileStream);
            }

            // 6. Xóa avatar cũ (nếu không phải là ảnh mặc định) để đỡ tốn dung lượng
            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                // Chuyển đổi đường dẫn web (ví dụ: /images/avatars/abc.jpg) thành đường dẫn tuyệt đối
                string oldFilePath = Path.Combine(_environment.WebRootPath, user.AvatarPath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    // Chỉ xóa nếu không phải file ảnh mặc định (nếu bạn có ảnh mặc định)
                    // if (!oldFilePath.EndsWith("default-avatar.png")) 
                    // {
                    System.IO.File.Delete(oldFilePath);
                    // }
                }
            }

            // 7. Cập nhật đường dẫn tương đối vào thuộc tính AvatarPath của user
            user.AvatarPath = $"/images/avatars/{uniqueFileName}";
            isProfileChanged = true;
        }

        // Nếu có thay đổi thông tin cá nhân thì cập nhật UpdatedAt và gọi UpdateAsync
        if (isProfileChanged)
        {
            user.UpdatedAt = DateTime.UtcNow; // Cập nhật mốc thời gian sửa đổi
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }
        }

        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}
