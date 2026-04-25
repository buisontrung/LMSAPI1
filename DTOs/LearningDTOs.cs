using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LMSAPI1.DTOs;

// Course DTOs
public class CreateCourseDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public IFormFile? ImageFile { get; set; }
}

public class UpdateCourseDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public IFormFile? ImageFile { get; set; }
}

public class CourseResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? ImageUrl { get; set; }
    // Thống kê đánh giá
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<ChapterResponseDTO>? Chapters { get; set; }
}

// Chapter DTOs
public class CreateChapterDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    [Required]
    public Guid CourseId { get; set; }
}

public class UpdateChapterDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    [Required]
    public Guid CourseId { get; set; }
}

public class ChapterResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid CourseId { get; set; }
    public List<LessonResponseDTO> Lessons { get; set; } = new List<LessonResponseDTO>();
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    // Tiến độ học của người dùng
    public int CompletedLessons { get; set; } = 0;
    public bool IsCompleted => Lessons.Count > 0 && CompletedLessons >= Lessons.Count;
}

// Lesson DTOs
public class CreateLessonDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>File video upload (mp4, webm, ...). Bắt buộc khi tạo mới.</summary>
    public IFormFile? VideoFile { get; set; }

    public int Order { get; set; }

    [Required]
    public Guid ChapterId { get; set; }
}

public class UpdateLessonDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>File video mới (tuỳ chọn). Nếu không gửi, giữ nguyên video cũ.</summary>
    public IFormFile? VideoFile { get; set; }

    public int Order { get; set; }

    [Required]
    public Guid ChapterId { get; set; }
}

public class LessonResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public int Order { get; set; }
    public Guid ChapterId { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    // Trạng thái học của người dùng
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}

public class UserCourseDetailResponseDTO
{
    public Guid Id { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CourseClassId { get; set; }
    public string CourseClassName { get; set; } = string.Empty;
    public string TrainingProgramName { get; set; } = string.Empty;
    // Giảng viên phụ trách
    public Guid? TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    // Số sinh viên đăng ký khóa học này
    public int StudentCount { get; set; }
    // Đánh giá sao
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    // Đánh giá của người dùng hiện tại (null = chưa đánh giá)
    public int? UserRating { get; set; }
    public string? UserComment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<ChapterResponseDTO> Chapters { get; set; } = new List<ChapterResponseDTO>();
    public ExamResponseDTO? Exam { get; set; }
}

public class SubmitRatingDTO
{
    [System.ComponentModel.DataAnnotations.Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5 sao.")]
    public int Rating { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Comment { get; set; }
}

/// <summary>
/// Thông tin một đánh giá riêng lẻ khi liệt kê ratings của course.
/// </summary>
public class RatingResponseDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
