using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.DTOs;

public class CreateEnrollmentDTO
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public Guid CourseClassId { get; set; }
}

public class BulkCreateEnrollmentDTO
{
    [Required]
    public List<Guid> UserIds { get; set; } = new();

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public Guid CourseClassId { get; set; }
}

public class UpdateEnrollmentDTO
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public Guid CourseClassId { get; set; }
}

public class EnrollmentResponseDTO
{
    public Guid Id { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public Guid CourseClassId { get; set; }
    public string CourseClassName { get; set; } = string.Empty;
    public string TrainingProgramName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
