using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

// 1. Academic Structure

public class TrainingProgram : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., CT6

    // Navigation properties
    // TrainingProgram 1 - n Class
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    
    // TrainingProgram 1 - n CourseClass
    public ICollection<CourseClass> CourseClasses { get; set; } = new List<CourseClass>();
    
    // TrainingProgram 1 - n AppUser
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}

public class Class : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., CT06A

    public Guid TrainingProgramId { get; set; }
    public TrainingProgram TrainingProgram { get; set; } = null!;

    // Navigation properties
    // Class 1 - n Student (AppUser)
    public ICollection<AppUser> Students { get; set; } = new List<AppUser>();
}
