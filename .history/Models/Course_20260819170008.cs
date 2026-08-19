using LMS_DotNETCore_MVC.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_DotNETCore_MVC.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Giảng viên
        [Required]
        public string InstructorId { get; set; } = string.Empty;

        [ForeignKey(nameof(InstructorId))]
        public ApplicationUser? Instructor { get; set; }

        // Bài học
        public ICollection<Lesson> Lessons { get; set; }
            = new List<Lesson>();

        // Học viên đăng ký
        public ICollection<Enrollment> Enrollments { get; set; }
            = new List<Enrollment>();
    }
}