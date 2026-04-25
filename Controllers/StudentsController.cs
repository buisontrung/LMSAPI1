using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StudentsController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly LMSDbContext _context;

    public StudentsController(UserManager<AppUser> userManager, LMSDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.GetUsersInRoleAsync("Student");
        foreach(var u in users) {
             await _context.Entry(u).Reference(c => c.Class).LoadAsync();
        }

        return Ok(users.Select(e => new StudentResponseDTO
        {
            Id = e.Id,
            FullName = e.FullName,
            StudentCode = e.StudentCode ?? string.Empty,
            ClassId = e.ClassId ?? Guid.Empty,
            ClassName = e.Class?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _userManager.FindByIdAsync(id.ToString());
        if (e == null) return NotFound();
        await _context.Entry(e).Reference(c => c.Class).LoadAsync();

        return Ok(new StudentResponseDTO
        {
            Id = e.Id,
            FullName = e.FullName,
            StudentCode = e.StudentCode ?? string.Empty,
            ClassId = e.ClassId ?? Guid.Empty,
            ClassName = e.Class?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStudentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new AppUser
        {
            UserName = dto.StudentCode,
            Email = dto.StudentCode + "@student.edu",
            FullName = dto.FullName,
            StudentCode = dto.StudentCode,
            ClassId = dto.ClassId,
            CreatedDate = DateTime.UtcNow
        };
        
        var result = await _userManager.CreateAsync(entity, "Student@123!");
        if (!result.Succeeded) return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(entity, "Student");
        await _context.Entry(entity).Reference(c => c.Class).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new StudentResponseDTO
        {
            Id = entity.Id,
            FullName = entity.FullName,
            StudentCode = entity.StudentCode ?? string.Empty,
            ClassId = entity.ClassId ?? Guid.Empty,
            ClassName = entity.Class?.Name ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateStudentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _userManager.FindByIdAsync(id.ToString());
        if (entity == null) return NotFound();
        
        entity.FullName = dto.FullName;
        entity.StudentCode = dto.StudentCode;
        entity.ClassId = dto.ClassId;
        entity.UpdatedDate = DateTime.UtcNow;
        
        await _userManager.UpdateAsync(entity);
        return Ok();
    }

    [HttpGet("{studentId}/courses")]
    public async Task<IActionResult> GetCoursesByStudent(Guid studentId)
    {
        var student = await _userManager.FindByIdAsync(studentId.ToString());
        if (student == null) return NotFound("Student not found.");

        var enrollments = await _context.Enrollments
            .Where(e => e.UserId == studentId)
            .Include(e => e.Course)
            .Include(e => e.CourseClass)
                .ThenInclude(cc => cc.TrainingProgram)
            .Include(e => e.CourseClass)
                .ThenInclude(cc => cc.Teacher)
            .ToListAsync();

        return Ok(enrollments.Select(e => new EnrollmentResponseDTO
        {
            Id = e.Id,
            EnrollmentDate = e.EnrollmentDate,
            UserId = e.UserId,
            UserName = student.FullName,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title ?? string.Empty,
            CourseClassId = e.CourseClassId,
            CourseClassName = e.CourseClass?.Name ?? string.Empty,
            TrainingProgramName = e.CourseClass?.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{studentId}/courses/{courseId}")]
    public async Task<IActionResult> GetCourseDetailForStudent(Guid studentId, Guid courseId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
            .Include(e => e.Course)
                .ThenInclude(c => c.Exam)
                    .ThenInclude(ex => ex!.Questions)
                        .ThenInclude(q => q.Answers)
            .Include(e => e.CourseClass)
                .ThenInclude(cc => cc.TrainingProgram)
            .Include(e => e.CourseClass)
                .ThenInclude(cc => cc.Teacher)
            .FirstOrDefaultAsync(e => e.UserId == studentId && e.CourseId == courseId);

        if (enrollment == null) return NotFound("Student is not enrolled in this course.");

        var course = enrollment.Course;

        // Lấy danh sách UserLesson của student cho toàn bộ lessons trong course này
        var lessonIds = course.Chapters.SelectMany(ch => ch.Lessons).Select(l => l.Id).ToList();
        var userLessons = await _context.UserLessons
            .Where(ul => ul.UserId == studentId && lessonIds.Contains(ul.LessonId))
            .ToDictionaryAsync(ul => ul.LessonId);

        // Số sinh viên đã đăng ký khoá học này
        var studentCount = await _context.Enrollments
            .CountAsync(e => e.CourseId == courseId);

        // Lấy thống kê đánh giá
        var ratings = await _context.CourseRatings
            .Where(r => r.CourseId == courseId)
            .ToListAsync();
        var userRating = ratings.FirstOrDefault(r => r.UserId == studentId);
        var avgRating = ratings.Any() ? ratings.Average(r => r.Rating) : 0.0;

        return Ok(new UserCourseDetailResponseDTO
        {
            Id = enrollment.Id,
            EnrollmentDate = enrollment.EnrollmentDate,
            UserId = enrollment.UserId,
            UserName = enrollment.User?.FullName ?? string.Empty,
            CourseId = course.Id,
            CourseName = course.Title,
            Description = course.Description,
            ImageUrl = course.ImageUrl,
            CourseClassId = enrollment.CourseClassId,
            CourseClassName = enrollment.CourseClass?.Name ?? string.Empty,
            TrainingProgramName = enrollment.CourseClass?.TrainingProgram?.Name ?? string.Empty,
            TeacherId = enrollment.CourseClass?.TeacherId,
            TeacherName = enrollment.CourseClass?.Teacher?.FullName ?? string.Empty,
            TeacherEmail = enrollment.CourseClass?.Teacher?.Email ?? string.Empty,
            StudentCount = studentCount,
            AverageRating = Math.Round(avgRating, 1),
            RatingCount = ratings.Count,
            UserRating = userRating?.Rating,
            UserComment = userRating?.Comment,
            CreatedDate = enrollment.CreatedDate,
            UpdatedDate = enrollment.UpdatedDate,
            Chapters = course.Chapters.OrderBy(ch => ch.Order).Select(ch =>
            {
                var lessons = ch.Lessons.OrderBy(l => l.Order).Select(l =>
                {
                    var ul = userLessons.GetValueOrDefault(l.Id);
                    return new LessonResponseDTO
                    {
                        Id = l.Id,
                        Title = l.Title,
                        FileUrl = l.FileUrl,
                        Order = l.Order,
                        ChapterId = l.ChapterId,
                        ChapterTitle = ch.Title,
                        CreatedDate = l.CreatedDate,
                        UpdatedDate = l.UpdatedDate,
                        IsCompleted = ul?.IsCompleted ?? false,
                        CompletedAt = ul?.CompletedAt
                    };
                }).ToList();

                return new ChapterResponseDTO
                {
                    Id = ch.Id,
                    Title = ch.Title,
                    Order = ch.Order,
                    CourseId = ch.CourseId,
                    CreatedDate = ch.CreatedDate,
                    UpdatedDate = ch.UpdatedDate,
                    Lessons = lessons,
                    CompletedLessons = lessons.Count(l => l.IsCompleted)
                };
            }).ToList(),
            Exam = course.Exam == null ? null : new ExamResponseDTO
            {
                Id = course.Exam.Id,
                Title = course.Exam.Title,
                Type = course.Exam.Type,
                CourseId = course.Exam.CourseId,
                CreatedDate = course.Exam.CreatedDate,
                UpdatedDate = course.Exam.UpdatedDate,
                Questions = course.Exam.Questions.Select(q => new QuestionResponseDTO
                {
                    Id = q.Id,
                    Content = q.Content,
                    ExamId = q.ExamId,
                    CreatedDate = q.CreatedDate,
                    UpdatedDate = q.UpdatedDate,
                    Answers = q.Answers.Select(a => new AnswerResponseDTO
                    {
                        Id = a.Id,
                        Content = a.Content,
                        IsCorrect = a.IsCorrect,
                        QuestionId = a.QuestionId,
                        CreatedDate = a.CreatedDate,
                        UpdatedDate = a.UpdatedDate
                    }).ToList()
                }).ToList()
            }
        });
    }

    /// <summary>
    /// Đánh dấu một bài học là đã hoàn thành.
    /// POST /api/Students/{studentId}/lessons/{lessonId}/complete
    /// </summary>
    [HttpPost("{studentId}/lessons/{lessonId}/complete")]
    public async Task<IActionResult> CompleteLesson(Guid studentId, Guid lessonId)
    {
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson == null) return NotFound("Bài học không tồn tại.");

        var existing = await _context.UserLessons
            .FirstOrDefaultAsync(ul => ul.UserId == studentId && ul.LessonId == lessonId);

        if (existing != null)
        {
            if (existing.IsCompleted)
                return Ok(new { Message = "Bài học đã được đánh dấu hoàn thành trước đó.", IsCompleted = true, CompletedAt = existing.CompletedAt });

            existing.IsCompleted = true;
            existing.CompletedAt = DateTime.UtcNow;
            existing.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            _context.UserLessons.Add(new UserLesson
            {
                UserId = studentId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã đánh dấu hoàn thành bài học.", IsCompleted = true, CompletedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Bỏ đánh dấu hoàn thành một bài học.
    /// DELETE /api/Students/{studentId}/lessons/{lessonId}/complete
    /// </summary>
    [HttpDelete("{studentId}/lessons/{lessonId}/complete")]
    public async Task<IActionResult> UncompleteLesson(Guid studentId, Guid lessonId)
    {
        var existing = await _context.UserLessons
            .FirstOrDefaultAsync(ul => ul.UserId == studentId && ul.LessonId == lessonId);

        if (existing == null || !existing.IsCompleted)
            return Ok(new { Message = "Bài học chưa được học.", IsCompleted = false });

        existing.IsCompleted = false;
        existing.CompletedAt = null;
        existing.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã bỏ đánh dấu hoàn thành.", IsCompleted = false });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _userManager.FindByIdAsync(id.ToString());
        if (entity == null) return NotFound();
        await _userManager.DeleteAsync(entity);
        return Ok();
    }

    /// <summary>
    /// Gửi hoặc cập nhật đánh giá sao cho khoá học.
    /// POST /api/Students/{studentId}/courses/{courseId}/rating
    /// </summary>
    [HttpPost("{studentId}/courses/{courseId}/rating")]
    public async Task<IActionResult> SubmitRating(Guid studentId, Guid courseId, [FromBody] SubmitRatingDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Kiểm tra student có đăng ký khoá học không
        var enrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == studentId && e.CourseId == courseId);
        if (!enrolled) return BadRequest("Bạn chưa đăng ký khoá học này.");

        var existing = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == studentId && r.CourseId == courseId);

        if (existing != null)
        {
            existing.Rating = dto.Rating;
            existing.Comment = dto.Comment;
            existing.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            _context.CourseRatings.Add(new CourseRating
            {
                UserId = studentId,
                CourseId = courseId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedDate = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // Tính lại trung bình
        var avgRating = await _context.CourseRatings
            .Where(r => r.CourseId == courseId)
            .AverageAsync(r => (double)r.Rating);

        return Ok(new
        {
            Message = "Đã gửi đánh giá thành công.",
            UserRating = dto.Rating,
            AverageRating = Math.Round(avgRating, 1),
            Comment = dto.Comment
        });
    }

    /// <summary>
    /// Xoá đánh giá của người dùng cho khoá học.
    /// DELETE /api/Students/{studentId}/courses/{courseId}/rating
    /// </summary>
    [HttpDelete("{studentId}/courses/{courseId}/rating")]
    public async Task<IActionResult> DeleteRating(Guid studentId, Guid courseId)
    {
        var existing = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.UserId == studentId && r.CourseId == courseId);

        if (existing == null) return NotFound("Chưa có đánh giá nào để xoá.");

        _context.CourseRatings.Remove(existing);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xoá đánh giá." });
    }
}
