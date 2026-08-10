using LMS_DotNETCore_MVC.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS_DotNETCore_MVC.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết đến Học viên (User)
        [Required]
        public string StudentId { get; set; }

        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        // Khóa ngoại liên kết đến Khóa học
        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public DateTime EnrollDate { get; set; } = DateTime.Now;
    }
}
