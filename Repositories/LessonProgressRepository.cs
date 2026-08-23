using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Repositories
{
    public class LessonProgressRepository : ILessonProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public LessonProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<int>> GetCompletedLessonIdsAsync(string studentId, int courseId)
        {
            return await _context.LessonProgresses
                .Where(lp => lp.StudentId == studentId && lp.Lesson!.CourseId == courseId && lp.IsCompleted)
                .Select(lp => lp.LessonId)
                .ToListAsync();
        }

        public async Task<bool> ToggleLessonProgressAsync(string studentId, int lessonId)
        {
            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.Now
                };
                await _context.LessonProgresses.AddAsync(progress);
            }
            else
            {
                progress.IsCompleted = !progress.IsCompleted;
                progress.CompletedAt = progress.IsCompleted ? DateTime.Now : null;
                _context.LessonProgresses.Update(progress);
            }

            await _context.SaveChangesAsync();
            return progress.IsCompleted;
        }

        public async Task<double> GetCourseCompletionPercentageAsync(string studentId, int courseId)
        {
            int totalLessons = await _context.Lessons.CountAsync(l => l.CourseId == courseId);
            if (totalLessons == 0) return 0;

            int completedLessons = await _context.LessonProgresses
                .CountAsync(lp => lp.StudentId == studentId && lp.Lesson!.CourseId == courseId && lp.IsCompleted);

            return Math.Round(((double)completedLessons / totalLessons) * 100, 1);
        }
    }
}
