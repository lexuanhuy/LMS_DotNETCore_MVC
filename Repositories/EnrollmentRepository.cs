using LMS_DotNETCore_MVC.Data;
using LMS_DotNETCore_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_DotNETCore_MVC.Repositories
{
    public class EnrollmentRepository: IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;

        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync()
        {
            // Lấy tất cả danh sách đăng ký kèm theo thông tin học viên và khóa học
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .ToListAsync();
        }

        public async Task<Enrollment> GetEnrollmentByIdAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(string studentId)
        {
            // Dùng cho trang "Khóa học của tôi" của học viên
            return await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course) // Lấy thông tin chi tiết khóa học mà học viên đã đăng ký
                .ThenInclude(c => c.Instructor) // Kèm luôn thông tin giảng viên dạy khóa đó
                .Include(e => e.Course)
                .ThenInclude(c => c.Category)
                .ToListAsync();
        }

        public async Task AddEnrollmentAsync(Enrollment enrollment)
        {
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEnrollmentAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
