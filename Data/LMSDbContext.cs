using LMSAPI1.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Data;

public class LMSDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options)
    {
    }

    public DbSet<TrainingProgram> TrainingPrograms { get; set; } = null!;
    public DbSet<Class> Classes { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Chapter> Chapters { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;
    public DbSet<Exam> Exams { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<Answer> Answers { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<CourseClass> CourseClasses { get; set; } = null!;
    public DbSet<UserLesson> UserLessons { get; set; } = null!;
    public DbSet<CourseRating> CourseRatings { get; set; } = null!;
    public DbSet<ExamSubmission> ExamSubmissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Academic Structure
        builder.Entity<TrainingProgram>()
            .HasMany(tp => tp.Classes)
            .WithOne(c => c.TrainingProgram)
            .HasForeignKey(c => c.TrainingProgramId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Class>()
            .HasMany(c => c.Students)
            .WithOne(u => u.Class)
            .HasForeignKey(u => u.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Learning
        builder.Entity<Course>()
            .HasMany(c => c.Chapters)
            .WithOne(ch => ch.Course)
            .HasForeignKey(ch => ch.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Chapter>()
            .HasMany(ch => ch.Lessons)
            .WithOne(l => l.Chapter)
            .HasForeignKey(l => l.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Assignments
        builder.Entity<Course>()
            .HasMany(c => c.Assignments)
            .WithOne(a => a.Course)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Assignment>()
            .HasOne(a => a.Lesson)
            .WithMany(l => l.Assignments)
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.Entity<Assignment>()
            .HasMany(a => a.Submissions)
            .WithOne(s => s.Assignment)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Submissions
        builder.Entity<AppUser>()
            .HasMany(u => u.Submissions)
            .WithOne(sub => sub.User)
            .HasForeignKey(sub => sub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Exam
        builder.Entity<Course>()
            .HasOne(c => c.Exam)
            .WithOne(e => e.Course)
            .HasForeignKey<Exam>(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Exam>()
            .HasMany(e => e.Questions)
            .WithOne(q => q.Exam)
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // CourseClass (Course 1 - N CourseClass, Class 1 - N CourseClass)
        builder.Entity<CourseClass>()
            .HasOne(cc => cc.Course)
            .WithMany(c => c.CourseClasses)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseClass>()
            .HasOne(cc => cc.TrainingProgram)
            .WithMany(tp => tp.CourseClasses)
            .HasForeignKey(cc => cc.TrainingProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teacher (AppUser with role Teacher) assigned to CourseClass
        builder.Entity<CourseClass>()
            .HasOne(cc => cc.Teacher)
            .WithMany()
            .HasForeignKey(cc => cc.TeacherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(u => u.TrainingProgram)
            .WithMany(tp => tp.Users)
            .HasForeignKey(u => u.TrainingProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrollment (User + Course + CourseClass)
        builder.Entity<Enrollment>()
            .HasKey(e => e.Id);

        builder.Entity<Enrollment>()
            .HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Enrollment>()
            .HasOne(e => e.CourseClass)
            .WithMany(cc => cc.Enrollments)
            .HasForeignKey(e => e.CourseClassId)
            .OnDelete(DeleteBehavior.NoAction);

        // UserLesson: track per-user lesson completion
        builder.Entity<UserLesson>()
            .HasKey(ul => ul.Id);

        builder.Entity<UserLesson>()
            .HasIndex(ul => new { ul.UserId, ul.LessonId })
            .IsUnique(); // Mỗi cặp User+Lesson chỉ có 1 record

        builder.Entity<UserLesson>()
            .HasOne(ul => ul.User)
            .WithMany()
            .HasForeignKey(ul => ul.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserLesson>()
            .HasOne(ul => ul.Lesson)
            .WithMany()
            .HasForeignKey(ul => ul.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        // CourseRating: user rates a course (1-5 stars), one rating per user per course
        builder.Entity<CourseRating>()
            .HasKey(r => r.Id);

        builder.Entity<CourseRating>()
            .HasIndex(r => new { r.UserId, r.CourseId })
            .IsUnique();

        builder.Entity<CourseRating>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CourseRating>()
            .HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
