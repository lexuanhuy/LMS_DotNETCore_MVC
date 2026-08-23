using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using LMS_DotNETCore_MVC.Models;

namespace LMS_DotNETCore_MVC.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Mô tả giảng viên
        public string? Description { get; set; }

        // Hồ sơ
        public string? AvatarPath { get; set; }

        public DateTime? DateOfBirth { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Quan hệ
        public ICollection<Course> Courses { get; set; }
            = new HashSet<Course>();

        public ICollection<Enrollment> Enrollments { get; set; }
            = new HashSet<Enrollment>();

        public ICollection<LessonProgress> LessonProgresses { get; set; }
            = new HashSet<LessonProgress>();

        public ICollection<CourseReview> Reviews { get; set; }
            = new HashSet<CourseReview>();
    }
}