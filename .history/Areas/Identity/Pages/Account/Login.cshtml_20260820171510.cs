// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using LMS_DotNETCore_MVC.Data;

namespace LMS_DotNETCore_MVC.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }


    // =========================================================
    // INPUT
    // =========================================================

    [BindProperty]
    public InputModel Input { get; set; } = new();


    // =========================================================
    // EXTERNAL LOGIN
    // =========================================================

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }


    // =========================================================
    // RETURN URL
    // =========================================================

    public string? ReturnUrl { get; set; }


    // =========================================================
    // ERROR MESSAGE
    // =========================================================

    [TempData]
    public string? ErrorMessage { get; set; }


    // =========================================================
    // INPUT MODEL
    // =========================================================

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;


        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }


    // =========================================================
    // GET - HIỂN THỊ TRANG ĐĂNG NHẬP
    // =========================================================

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(
                string.Empty,
                ErrorMessage);
        }


        returnUrl ??= Url.Content("~/");


        // Xóa cookie đăng nhập bên ngoài
        await HttpContext.SignOutAsync(
            IdentityConstants.ExternalScheme);


        ExternalLogins =
            (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();


        ReturnUrl = returnUrl;
    }


    // =========================================================
    // POST - ĐĂNG NHẬP
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
        // KIỂM TRA FORM
        // =====================================================

        if (!ModelState.IsValid)
        {
            return Page();
        }


        // =====================================================
        // ĐĂNG NHẬP
        // =====================================================

        var result =
            await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);


        // =====================================================
        // ĐĂNG NHẬP THÀNH CÔNG
        // =====================================================

        if (result.Succeeded)
        {
            _logger.LogInformation(
                "Người dùng đã đăng nhập.");

            return LocalRedirect(returnUrl);
        }


        // =====================================================
        // XÁC THỰC 2 BƯỚC
        // =====================================================

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage(
                "./LoginWith2fa",
                new
                {
                    ReturnUrl = returnUrl,
                    RememberMe = Input.RememberMe
                });
        }


        // =====================================================
        // TÀI KHOẢN BỊ KHÓA
        // =====================================================

        if (result.IsLockedOut)
        {
            _logger.LogWarning(
                "Tài khoản người dùng đã bị khóa.");

            ModelState.AddModelError(
                string.Empty,
                "Tài khoản của bạn đã bị khóa. Vui lòng thử lại sau.");

            return Page();
        }


        // =====================================================
        // ĐĂNG NHẬP THẤT BẠI
        // =====================================================

        ModelState.AddModelError(
            string.Empty,
            "Email hoặc mật khẩu không chính xác.");

        return Page();
    }
}