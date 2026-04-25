using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

// 4. Exam

public class Exam : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public ExamType Type { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    // Navigation properties
    // Exam 1 - n Question
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}

public class Question : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    // Navigation properties
    // Question 1 - n Answer (only for MCQ)
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class ExamSubmission : BaseEntity
{
    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public string? SubmissionFileUrl { get; set; } // for Essay
    public string? Content { get; set; } // for text responses

    public double? Score { get; set; }
    public string? TeacherFeedback { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class Answer : BaseEntity
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}
