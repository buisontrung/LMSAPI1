using System.ComponentModel.DataAnnotations;
using LMSAPI1.Entities;

namespace LMSAPI1.DTOs;

// Exam DTOs
public class CreateExamDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public ExamType Type { get; set; }

    [Required]
    public Guid CourseId { get; set; }
}

public class UpdateExamDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public ExamType Type { get; set; }

    [Required]
    public Guid CourseId { get; set; }
}

public class ExamResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ExamType Type { get; set; }
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<QuestionResponseDTO> Questions { get; set; } = new();
}

// Question DTOs
public class CreateQuestionDTO
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid ExamId { get; set; }
}

public class UpdateQuestionDTO
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid ExamId { get; set; }
}

public class QuestionResponseDTO
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public List<AnswerResponseDTO> Answers { get; set; } = new();
}

// Answer DTOs
public class CreateAnswerDTO
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    [Required]
    public Guid QuestionId { get; set; }
}

public class UpdateAnswerDTO
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

public class AnswerResponseDTO
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public Guid QuestionId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

// Submission DTOs
public class CreateExamSubmissionDTO
{
    [Required]
    public Guid ExamId { get; set; }
    public string? Content { get; set; }
    public Microsoft.AspNetCore.Http.IFormFile? SubmissionFile { get; set; }
    public List<Guid>? AnswerIds { get; set; }
}

public class SubmitMCQDTO
{
    [Required]
    public Guid ExamId { get; set; }
    public List<Guid> AnswerIds { get; set; } = new();
}

public class SubmitEssayDTO
{
    [Required]
    public Guid ExamId { get; set; }
    public string? Content { get; set; }
    public Microsoft.AspNetCore.Http.IFormFile? SubmissionFile { get; set; }
}

public class GradeExamSubmissionDTO
{
    [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
    public double Score { get; set; }
    public string? TeacherFeedback { get; set; }
}

public class ExamSubmissionResponseDTO
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? SubmissionFileUrl { get; set; }
    public string? Content { get; set; }
    public double? Score { get; set; }
    public string? TeacherFeedback { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? CourseClassName { get; set; }
}
