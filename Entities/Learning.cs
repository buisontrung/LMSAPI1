using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

// 2. Learning

public class Course : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    // Thống kê đánh giáproperties
    // Course 1 - n Chapter
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    
    // Course 1 - n Assignment (optional)
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    
    // Course 1 - 1 Exam (optional)
    public Exam? Exam { get; set; } 
    
    // Course 1 - n CourseClass
    public ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();

    // Student n - n Course (via Enrollment)
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Chapter : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int Order { get; set; }

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

public class Lesson : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty; // PDF file

    public int Order { get; set; }

    public Guid ChapterId { get; set; }
    public Chapter Chapter { get; set; } = null!;

    // Navigation properties
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
