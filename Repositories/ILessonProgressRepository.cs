using LMS_DotNETCore_MVC.Models;

namespace LMS_DotNETCore_MVC.Repositories
{
    public interface ILessonProgressRepository
    {
        Task<IEnumerable<int>> GetCompletedLessonIdsAsync(string studentId, int courseId);
        Task<bool> ToggleLessonProgressAsync(string studentId, int lessonId);
        Task<double> GetCourseCompletionPercentageAsync(string studentId, int courseId);
    }
}
