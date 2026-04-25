using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.DTOs;

// TrainingProgram DTOs
public class CreateTrainingProgramDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateTrainingProgramDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class TrainingProgramResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

// Class DTOs
public class CreateClassDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid TrainingProgramId { get; set; }
}

public class UpdateClassDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid TrainingProgramId { get; set; }
}

public class ClassResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TrainingProgramId { get; set; }
    public string TrainingProgramName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

// Student DTOs
public class CreateStudentDTO
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string StudentCode { get; set; } = string.Empty;

    [Required]
    public Guid ClassId { get; set; }
}

public class UpdateStudentDTO
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string StudentCode { get; set; } = string.Empty;

    [Required]
    public Guid ClassId { get; set; }
}

public class StudentResponseDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
