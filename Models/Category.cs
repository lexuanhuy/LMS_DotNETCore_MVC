using System.ComponentModel.DataAnnotations;

namespace LMS_DotNETCore_MVC.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IconClass { get; set; } = "bi bi-folder"; // Bootstrap icon class or emoji

        public int DisplayOrder { get; set; } = 0;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
