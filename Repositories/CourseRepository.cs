using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS_DotNETCore_MVC.Repositories
{
    public class CourseRepository: ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.Include(c => c.Lessons).Include(c => c.Instructor).ToListAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCourseAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCourseAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<(IEnumerable<Course> Courses, int TotalPages)> GetCoursesPaginatedAsync(string searchString, int pageNumber, int pageSize)
        {
            // 1. Khởi tạo truy vấn ban đầu (kèm thông tin Giảng viên nếu cần)
            var query = _context.Courses
                .Include(c => c.Instructor)
                .AsQueryable();

            // 2. Xử lý tìm kiếm nếu người dùng nhập từ khóa
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(c => c.Title.Contains(searchString) || c.Description.Contains(searchString));
            }

            // 3. Tính tổng số lượng bản ghi sau khi lọc
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 4. Thực hiện phân trang (Skip & Take)
            var courses = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (courses, totalPages);
        }
    }
}
