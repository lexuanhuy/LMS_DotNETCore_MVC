using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using LMS_DotNETCore_MVC.Models;

namespace LMS_DotNETCore_MVC.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; }

    // Profile fields
    public string? AvatarPath { get; set; }        // relative URL to avatar, e.g. "/uploads/avatars/..."
    public DateTime? DateOfBirth { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Quan hệ với các bảng khác
    public ICollection<Course> Courses { get; set; } = new HashSet<Course>();
    public ICollection<Enrollment> Enrollments { get; set; } = new HashSet<Enrollment>();
}
