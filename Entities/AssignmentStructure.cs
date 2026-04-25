using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

// 3. Assignment

public class Assignment : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime DueDate { get; set; }

    // Optional for each lesson or course
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }

    public Guid? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    // Navigation properties
    // Assignment 1 - n Submission
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

public class Submission : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string SubmissionFileUrl { get; set; } = string.Empty;

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
}
