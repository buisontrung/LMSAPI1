using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

// CourseClass: each Course can have multiple class groups
// e.g., Course "Mathematics" has CourseClass for CT06A, CT06B, etc.

public class CourseClass : BaseEntity
{
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public Guid TrainingProgramId { get; set; }
    public TrainingProgram TrainingProgram { get; set; } = null!;

    // Giảng viên phụ trách (lấy từ AspNetUsers có Role = Teacher)
    public Guid? TeacherId { get; set; }
    public AppUser? Teacher { get; set; }

    // Navigation properties
    // CourseClass 1 - n Enrollment
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
