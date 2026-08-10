using LMS_DotNETCore_MVC.Models;

namespace LMS_DotNETCore_MVC.Repositories
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course> GetCourseByIdAsync(int id);
        Task AddCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(int id);
        Task<(IEnumerable<Course> Courses, int TotalPages)> GetCoursesPaginatedAsync(string searchString, int pageNumber, int pageSize);
    }
}
