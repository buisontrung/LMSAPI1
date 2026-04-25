using Microsoft.AspNetCore.Identity;

namespace LMSAPI1.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }

    // Student specific properties
    public string? StudentCode { get; set; }
    
    public Guid? ClassId { get; set; }
    public Class? Class { get; set; }

    public Guid? TrainingProgramId { get; set; }
    public TrainingProgram? TrainingProgram { get; set; }

    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
