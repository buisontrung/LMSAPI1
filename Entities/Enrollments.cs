namespace LMSAPI1.Entities;

// 5. Enrollment
// A student enrolls in a Course and is assigned to exactly one CourseClass

public class Enrollment : BaseEntity
{
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public Guid CourseClassId { get; set; }
    public CourseClass CourseClass { get; set; } = null!;
}
