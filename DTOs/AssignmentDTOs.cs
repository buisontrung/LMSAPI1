using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.DTOs;

// Assignment DTOs
public class CreateAssignmentDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime DueDate { get; set; }

    public Guid? CourseId { get; set; }
    public Guid? LessonId { get; set; }
}

public class UpdateAssignmentDTO
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime DueDate { get; set; }

    public Guid? CourseId { get; set; }
    public Guid? LessonId { get; set; }
}

public class AssignmentResponseDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public Guid? CourseId { get; set; }
    public string? CourseName { get; set; }
    public Guid? LessonId { get; set; }
    public string? LessonName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

// Submission DTOs
public class CreateSubmissionDTO
{
    [Required]
    [MaxLength(500)]
    public string SubmissionFileUrl { get; set; } = string.Empty;

    [Required]
    public Guid AssignmentId { get; set; }

    [Required]
    public Guid UserId { get; set; }
}

public class UpdateSubmissionDTO
{
    [Required]
    [MaxLength(500)]
    public string SubmissionFileUrl { get; set; } = string.Empty;
}

public class SubmissionResponseDTO
{
    public Guid Id { get; set; }
    public string SubmissionFileUrl { get; set; } = string.Empty;
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
