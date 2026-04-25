using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.DTOs;

public class CreateCourseClassDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public Guid TrainingProgramId { get; set; }

    // Giảng viên phụ trách (optional khi tạo)
    public Guid? TeacherId { get; set; }
}

public class UpdateCourseClassDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    public Guid TrainingProgramId { get; set; }

    // Giảng viên phụ trách (nullable để xóa giảng viên)
    public Guid? TeacherId { get; set; }
}

public class AssignTeacherDTO
{
    [Required]
    public Guid TeacherId { get; set; }
}

public class CourseClassResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public Guid TrainingProgramId { get; set; }
    public string TrainingProgramName { get; set; } = string.Empty;
    // Teacher info
    public Guid? TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
