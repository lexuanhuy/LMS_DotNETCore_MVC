using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Repositories
{
    public class CourseReviewRepository : ICourseReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseReviewRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseReview>> GetReviewsByCourseIdAsync(int courseId)
        {
            return await _context.CourseReviews
                .Include(r => r.Student)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddReviewAsync(CourseReview review)
        {
            await _context.CourseReviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<double> GetAverageRatingAsync(int courseId)
        {
            var ratings = await _context.CourseReviews
                .Where(r => r.CourseId == courseId)
                .Select(r => r.Rating)
                .ToListAsync();

            if (!ratings.Any()) return 5.0; // Default rating if no reviews yet
            return Math.Round(ratings.Average(), 1);
        }
    }
}
