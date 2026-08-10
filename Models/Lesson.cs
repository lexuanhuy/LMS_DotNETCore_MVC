using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_DotNETCore_MVC.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }

        public string? ContentUrl { get; set; } // Link video bài giảng hoặc file tài liệu

        public int OrderIndex { get; set; } // Thứ tự bài học trong khóa học (Bài 1, Bài 2,...)

        // Khóa ngoại liên kết đến Khóa học
        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
