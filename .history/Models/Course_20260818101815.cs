using LMS_DotNETCore_MVC.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_DotNETCore_MVC.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal? Price { get; set; } // Giá khóa học (nếu có)

        public string? ImageUrl { get; set; } // Ảnh bìa khóa học

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Khóa ngoại liên kết đến Giảng viên (User)
        [Required]
        public string InstructorId { get; set; }

        [ForeignKey("InstructorId")]
        public ApplicationUser Instructor { get; set; }

        // Quan hệ: Một Khóa học có nhiều Bài học
        public ICollection<Lesson> Lessons { get; set; }

        // Quan hệ: Một Khóa học có nhiều Học viên đăng ký
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
