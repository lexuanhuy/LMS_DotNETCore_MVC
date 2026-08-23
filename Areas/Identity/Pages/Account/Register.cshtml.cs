// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;

namespace LMS_DotNETCore_MVC.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(
            100,
            ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;


        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare(
            "Password",
            ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? ConfirmPassword { get; set; }


        [Display(Name = "Vai trò")]
        public string? Role { get; set; }


        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }


    // =========================================================
    // GET - HIỂN THỊ TRANG ĐĂNG KÝ
    // =========================================================

    public async Task OnGetAsync(string? returnUrl = null)
    {
        // Tạo Role nếu chưa tồn tại
        if (!await _roleManager.RoleExistsAsync(SD.Role_Instructor))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(SD.Role_Instructor));

            await _roleManager.CreateAsync(
                new IdentityRole(SD.Role_Student));
        }
        if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
        {
            await _roleManager.CreateAsync(
               new IdentityRole(SD.Role_Admin));
        }

            Input = new InputModel
        {
            RoleList = _roleManager.Roles
                .Select(x => x.Name)
                .Where(x => x != null)
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                })
        };

        ReturnUrl = returnUrl;

        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();
    }


    // =========================================================
    // POST - ĐĂNG KÝ
    // =========================================================

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();


        // =====================================================
        // KIỂM TRA DỮ LIỆU
        // =====================================================

        if (!ModelState.IsValid)
        {
            await LoadRoles();
            return Page();
        }


        // =====================================================
        // KIỂM TRA EMAIL ĐÃ TỒN TẠI
        // =====================================================

        var existingUser =
            await _userManager.FindByEmailAsync(Input.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                "Input.Email",
                "Email này đã được sử dụng. Vui lòng sử dụng email khác.");

            await LoadRoles();

            return Page();
        }


        // =====================================================
        // TẠO USER
        // =====================================================

        var user = CreateUser();

        user.FullName = Input.FullName;

        await _userStore.SetUserNameAsync(
            user,
            Input.Email,
            CancellationToken.None);

        await _emailStore.SetEmailAsync(
            user,
            Input.Email,
            CancellationToken.None);


        // =====================================================
        // TẠO TÀI KHOẢN
        // =====================================================

        var result =
            await _userManager.CreateAsync(
                user,
                Input.Password);


        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Người dùng đã tạo tài khoản mới.");


            // =================================================
            // GÁN ROLE
            // =================================================

            if (!string.IsNullOrEmpty(Input.Role))
            {
                await _userManager.AddToRoleAsync(
                    user,
                    Input.Role);
            }
            else
            {
                await _userManager.AddToRoleAsync(
                    user,
                    SD.Role_Instructor);
            }


            // =================================================
            // XÁC NHẬN EMAIL
            // =================================================

            var userId =
                await _userManager.GetUserIdAsync(user);

            var code =
                await _userManager
                    .GenerateEmailConfirmationTokenAsync(user);

            code =
                WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(code));


            var callbackUrl =
                Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new
                    {
                        area = "Identity",
                        userId = userId,
                        code = code,
                        returnUrl = returnUrl
                    },
                    protocol: Request.Scheme)!;


            // =================================================
            // GỬI EMAIL
            // =================================================

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Xác nhận tài khoản LMS",
                $"""
                <h3>Chào mừng bạn đến với LMS!</h3>

                <p>
                    Cảm ơn bạn đã đăng ký tài khoản.
                </p>

                <p>
                    Vui lòng nhấn vào liên kết bên dưới để xác nhận email:
                </p>

                <p>
                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>
                        Xác nhận tài khoản
                    </a>
                </p>
                """);


            // =================================================
            // YÊU CẦU XÁC NHẬN EMAIL
            // =================================================

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                TempData["SuccessMessage"] =
                    "Đăng ký tài khoản thành công! Vui lòng kiểm tra email để xác nhận tài khoản.";

                return RedirectToPage(
                    "RegisterConfirmation",
                    new
                    {
                        email = Input.Email,
                        returnUrl = returnUrl
                    });
            }


            // =================================================
            // ĐĂNG NHẬP LUÔN
            // =================================================

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);


            TempData["SuccessMessage"] =
                "Đăng ký tài khoản thành công!";

            return LocalRedirect(returnUrl);
        }


        // =====================================================
        // CHUYỂN LỖI IDENTITY SANG TIẾNG VIỆT
        // =====================================================

        foreach (var error in result.Errors)
        {
            string message = error.Code switch
            {
                "DuplicateUserName" =>
                    "Tên đăng nhập hoặc email đã tồn tại.",

                "DuplicateEmail" =>
                    "Email này đã được sử dụng.",

                "InvalidUserName" =>
                    "Tên đăng nhập không hợp lệ.",

                "PasswordTooShort" =>
                    "Mật khẩu phải có ít nhất 6 ký tự.",

                "PasswordRequiresDigit" =>
                    "Mật khẩu phải có ít nhất một chữ số.",

                "PasswordRequiresLower" =>
                    "Mật khẩu phải có ít nhất một chữ cái thường.",

                "PasswordRequiresUpper" =>
                    "Mật khẩu phải có ít nhất một chữ cái in hoa.",

                "PasswordRequiresNonAlphanumeric" =>
                    "Mật khẩu phải có ít nhất một ký tự đặc biệt.",

                "PasswordRequiresUniqueChars" =>
                    "Mật khẩu phải có đủ số lượng ký tự khác nhau.",

                _ =>
                    "Đăng ký tài khoản không thành công. Vui lòng kiểm tra lại thông tin."
            };

            ModelState.AddModelError(
                string.Empty,
                message);
        }


        await LoadRoles();

        return Page();
    }


    // =========================================================
    // LOAD ROLE
    // =========================================================

    private async Task LoadRoles()
    {
        Input.RoleList =
            _roleManager.Roles
                .Select(x => x.Name)
                .Where(x => x != null)
                .Select(x => new SelectListItem
                {
                    Text = x,
                    Value = x
                });

        await Task.CompletedTask;
    }


    // =========================================================
    // CREATE USER
    // =========================================================

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException(
                $"Không thể tạo tài khoản '{nameof(ApplicationUser)}'. " +
                $"Hãy kiểm tra ApplicationUser có constructor không tham số.");
        }
    }


    // =========================================================
    // EMAIL STORE
    // =========================================================

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException(
                "Hệ thống Identity chưa hỗ trợ email.");
        }

        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}