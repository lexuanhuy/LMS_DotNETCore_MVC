using LMS_DotNETCore_MVC.Models;

namespace LMS_DotNETCore_MVC.Repositories
{
    public interface ICourseReviewRepository
    {
        Task<IEnumerable<CourseReview>> GetReviewsByCourseIdAsync(int courseId);
        Task AddReviewAsync(CourseReview review);
        Task<double> GetAverageRatingAsync(int courseId);
    }
}
