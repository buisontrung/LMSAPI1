using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMSAPI1.Controllers;
    
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExamSubmissionsController : ControllerBase
{
    private readonly LMSDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ExamSubmissionsController(LMSDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    /// <summary>
    /// Kiểm tra xem sinh viên hiện tại đã nộp bài này chưa.
    /// </summary>
    [HttpGet("my-submission/{examId}")]
    public async Task<IActionResult> GetMySubmission(Guid examId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var submission = await _context.ExamSubmissions
            .FirstOrDefaultAsync(s => s.ExamId == examId && s.UserId == userId);

        if (submission == null) return Ok(null);

        return Ok(new ExamSubmissionResponseDTO
        {
            Id = submission.Id,
            ExamId = submission.ExamId,
            UserId = submission.UserId,
            SubmissionFileUrl = submission.SubmissionFileUrl,
            Content = submission.Content,
            Score = submission.Score,
            TeacherFeedback = submission.TeacherFeedback,
            SubmittedAt = submission.SubmittedAt
        });
    }

    /// <summary>
    /// Sinh viên nộp bài kiểm tra.
    /// </summary>
    /// <summary>
    /// Sinh viên nộp bài trắc nghiệm (JSON).
    /// </summary>
    [HttpPost("submit/mcq")]
    public async Task<IActionResult> SubmitMCQ([FromBody] SubmitMCQDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var exam = await _context.Exams.FindAsync(dto.ExamId);
        if (exam == null) return NotFound("Không tìm thấy bài kiểm tra.");
        if (exam.Type != ExamType.MCQ) return BadRequest("Bài thi này không phải trắc nghiệm.");

        if (dto.AnswerIds == null || dto.AnswerIds.Count == 0)
            return BadRequest("Vui lòng chọn đáp án.");

        var submission = new ExamSubmission
        {
            ExamId = dto.ExamId,
            UserId = userId,
            SubmittedAt = DateTime.UtcNow
        };

        var questions = await _context.Questions
            .Include(q => q.Answers)
            .Where(q => q.ExamId == exam.Id)
            .ToListAsync();

        if (questions.Any())
        {
            int totalQuestions = questions.Count;
            int correctCount = 0;
            string details = "";

            foreach (var q in questions)
            {
                var correctAnsIds = q.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
                var studentAnswersForQ = dto.AnswerIds.Intersect(q.Answers.Select(a => a.Id)).ToList();
                
                bool isCorrect = studentAnswersForQ.Count > 0 && 
                                studentAnswersForQ.Count == correctAnsIds.Count && 
                                studentAnswersForQ.All(id => correctAnsIds.Contains(id));

                if (isCorrect) correctCount++;
                
                var ans = q.Answers.FirstOrDefault(a => studentAnswersForQ.Contains(a.Id));
                var corr = q.Answers.FirstOrDefault(a => a.IsCorrect);
				details += $"Câu: {(isCorrect ? "Đúng" : "Sai")} (Chọn: {ans?.Content ?? "N/A"}{(!isCorrect ? $", Đ/A đúng: {corr?.Content}" : "")})\n";
			}

            double scorePerQuestion = 10.0 / totalQuestions;
            submission.Score = Math.Round(correctCount * scorePerQuestion, 2);
            submission.Content = $"[Hệ thống chấm tự động]\nĐúng {correctCount}/{totalQuestions} câu.\n\nChi tiết:\n{details}";
            submission.TeacherFeedback = $"[Chấm tự động] Đúng {correctCount}/{totalQuestions} câu. (Mỗi câu {Math.Round(scorePerQuestion, 2)} điểm)";
        }

        _context.ExamSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã nộp bài trắc nghiệm thành công.", Score = submission.Score });
    }

    /// <summary>
    /// Sinh viên nộp bài tự luận (FormForm có file).
    /// </summary>
    [HttpPost("submit/essay")]
    public async Task<IActionResult> SubmitEssay([FromForm] SubmitEssayDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var exam = await _context.Exams.FindAsync(dto.ExamId);
        if (exam == null) return NotFound("Không tìm thấy bài kiểm tra.");
        if (exam.Type != ExamType.Essay) return BadRequest("Bài thi này không phải tự luận.");

        if (string.IsNullOrWhiteSpace(dto.Content) && dto.SubmissionFile == null)
            return BadRequest("Vui lòng nhập nội dung hoặc đính kèm file.");

        string? fileUrl = null;
        if (dto.SubmissionFile != null)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.SubmissionFile.FileName);
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "submissions", "exams");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.SubmissionFile.CopyToAsync(stream);
            }
            fileUrl = "/submissions/exams/" + fileName;
        }

        var submission = new ExamSubmission
        {
            ExamId = dto.ExamId,
            UserId = userId,
            Content = dto.Content,
            SubmissionFileUrl = fileUrl,
            SubmittedAt = DateTime.UtcNow,
            Score = null,
            TeacherFeedback = "Đang chờ giảng viên chấm bài."
        };

        _context.ExamSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Đã nộp bài tự luận thành công." });
    }

    /// <summary>
    /// Giảng viên lấy danh sách bài nộp của lớp mình phụ trách.
    /// </summary>
    [HttpGet("teacher/list")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetForTeacher()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        bool isAdmin = User.IsInRole("Admin");

        // Lấy danh sách bài nộp
        var query = _context.ExamSubmissions
            .Include(s => s.Exam)
            .Include(s => s.User)
            .AsQueryable();

        if (!isAdmin)
        {
            // Nếu là Teacher, chỉ lấy sinh viên thuộc lớp mình dạy cho khoá học đó
            // Phải join qua Enrollment và CourseClass
            query = query.Where(s => _context.Enrollments.Any(e => 
                e.UserId == s.UserId && 
                e.CourseId == s.Exam.CourseId && 
                e.CourseClass.TeacherId == userId));
        }

        var results = await query
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        // Map to DTO and include class info
        var response = new List<ExamSubmissionResponseDTO>();
        foreach (var s in results)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.CourseClass)
                .FirstOrDefaultAsync(e => e.UserId == s.UserId && e.CourseId == s.Exam.CourseId);

            response.Add(new ExamSubmissionResponseDTO
            {
                Id = s.Id,
                ExamId = s.ExamId,
                ExamTitle = s.Exam.Title,
                UserId = s.UserId,
                UserName = s.User?.FullName ?? "Unknown",
                SubmissionFileUrl = s.SubmissionFileUrl,
                Content = s.Content,
                Score = s.Score,
                TeacherFeedback = s.TeacherFeedback,
                SubmittedAt = s.SubmittedAt,
                CourseClassName = enrollment?.CourseClass?.Name
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Giảng viên chấm điểm bài nộp.
    /// </summary>
    [HttpPut("{id}/grade")]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Grade(Guid id, GradeExamSubmissionDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var submission = await _context.ExamSubmissions
            .Include(s => s.Exam)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (submission == null) return NotFound("Không tìm thấy bài nộp.");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        bool isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            // Kiểm tra xem Teacher có dạy lớp của sinh viên này không
            var hasAccess = await _context.Enrollments.AnyAsync(e => 
                e.UserId == submission.UserId && 
                e.CourseId == submission.Exam.CourseId && 
                e.CourseClass.TeacherId == userId);

            if (!hasAccess) return Forbid("Bạn không có quyền chấm bài cho sinh viên này.");
        }

        submission.Score = dto.Score;
        submission.TeacherFeedback = dto.TeacherFeedback;
        submission.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã lưu điểm thành công." });
    }

    /// <summary>
    /// Lấy danh sách các bài nộp gần đây của một khóa học (dùng cho trang CourseDetail).
    /// </summary>
    [HttpGet("course/{courseId}/recent-submissions")]
    public async Task<IActionResult> GetRecentSubmissions(Guid courseId)
    {
        var submissions = await _context.ExamSubmissions
            .Include(s => s.User)
            .Include(s => s.Exam)
            .Where(s => s.Exam.CourseId == courseId)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5) // Lấy 5 bài nộp gần nhất
            .Select(s => new
            {
                Id = s.Id,
                StudentName = s.User.FullName,
                SubmittedAt = s.SubmittedAt,
                Score = s.Score,
                Content = s.Content // Có thể dùng để hiển thị "Đúng X/Y câu"
            })
            .ToListAsync();

        return Ok(submissions);
    }
}
